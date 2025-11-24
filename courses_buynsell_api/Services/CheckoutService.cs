using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using courses_buynsell_api.Entities;
using courses_buynsell_api.DTOs.Momo;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Data;

public class CheckoutService : ICheckoutService
{
    private readonly MomoOptions _momo;
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        IOptions<MomoOptions> momoOptions,
        AppDbContext context,
        INotificationService notificationService,
        IMemoryCache cache,
        ILogger<CheckoutService> logger)
    {
        _momo = momoOptions.Value;
        _context = context;
        _notificationService = notificationService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> CreateMomoPaymentAsync(CreateMomoPaymentRequestDto request, int buyerId)
    {
        string orderId = Guid.NewGuid().ToString();
        string requestId = Guid.NewGuid().ToString();
        string amount = ((long)request.Amount).ToString();

        if (buyerId == -1)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // ✅ Lưu thông tin payment vào cache (tồn tại 30 phút)
        var paymentInfo = new PaymentCacheInfo
        {
            BuyerId = buyerId,
            CourseIds = request.CourseIds,
            Amount = request.Amount
        };

        _cache.Set($"payment_{orderId}", paymentInfo, TimeSpan.FromMinutes(30));
        _logger.LogInformation($"💾 Saved payment info to cache for orderId: {orderId}, BuyerId: {buyerId}, CourseIds: {string.Join(",", request.CourseIds)}");

        // Raw hash string
        string rawHash = $"accessKey={_momo.AccessKey}&amount={amount}&extraData=&ipnUrl={_momo.NotifyUrl}&orderId={orderId}&orderInfo=Thanh toan khoa hoc&partnerCode={_momo.PartnerCode}&redirectUrl={_momo.ReturnUrl}&requestId={requestId}&requestType={_momo.RequestType}";

        string signature = HmacSHA256(rawHash, _momo.SecretKey);

        var body = new
        {
            partnerCode = _momo.PartnerCode,
            accessKey = _momo.AccessKey,
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = "Thanh toan khoa hoc",
            redirectUrl = _momo.ReturnUrl,
            ipnUrl = _momo.NotifyUrl,
            extraData = "",
            requestType = _momo.RequestType,
            signature = signature,
            lang = "vi"
        };

        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_momo.ApiUrl, content);
        var result = await response.Content.ReadAsStringAsync();

        var momoResponse = JsonConvert.DeserializeObject<MomoPaymentResponseDto>(result);

        return momoResponse!.PayUrl;
    }

    public async Task HandleMomoCallbackAsync(Dictionary<string, string> formData)
    {
        _logger.LogInformation($"🔔 Received MoMo callback: {JsonConvert.SerializeObject(formData)}");

        if (!formData.ContainsKey("signature"))
        {
            _logger.LogError("❌ No signature in callback data");
            return;
        }

        string signature = formData["signature"];
        var formDataCopy = new Dictionary<string, string>(formData);
        formDataCopy.Remove("signature");

        var rawHash = string.Join("&", formDataCopy.OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}"));

        _logger.LogInformation($"🔐 Raw hash for verification: {rawHash}");

        string mySignature = HmacSHA256(rawHash, _momo.SecretKey);

        _logger.LogInformation($"✅ My signature: {mySignature}");
        _logger.LogInformation($"📩 MoMo signature: {signature}");
        _logger.LogInformation($"🎯 Result code: {formData.GetValueOrDefault("resultCode", "N/A")}");

        // ✅ Chỉ xử lý khi thanh toán THÀNH CÔNG
        if (mySignature != signature)
        {
            _logger.LogError("❌ Signature verification FAILED!");
            return;
        }

        if (!formData.ContainsKey("resultCode") || formData["resultCode"] != "0")
        {
            _logger.LogWarning($"⚠️ Payment failed or pending. Result code: {formData.GetValueOrDefault("resultCode", "N/A")}");
            return;
        }

        string orderId = formData["orderId"];
        _logger.LogInformation($"✅ Payment successful for orderId: {orderId}");

        // ✅ Lấy thông tin payment từ cache
        if (!_cache.TryGetValue($"payment_{orderId}", out PaymentCacheInfo? paymentInfo) || paymentInfo == null)
        {
            _logger.LogError($"❌ Payment info not found in cache for orderId: {orderId}");
            return;
        }

        _logger.LogInformation($"💾 Retrieved payment info from cache: BuyerId={paymentInfo.BuyerId}, CourseIds={string.Join(",", paymentInfo.CourseIds)}");

        try
        {
            // ✅ BẮT ĐẦU LƯU VÀO DATABASE SAU KHI THANH TOÁN THÀNH CÔNG
            var transaction = new Transaction
            {
                TransactionCode = orderId,
                PaymentMethod = "MOMO",
                TotalAmount = paymentInfo.Amount,
                BuyerId = paymentInfo.BuyerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"💰 Created transaction ID: {transaction.Id}");

            // ✅ Lưu TransactionDetails và gửi thông báo
            foreach (var courseId in paymentInfo.CourseIds)
            {
                var course = await _context.Courses
                    .Where(c => c.Id == courseId)
                    .Select(c => new { c.SellerId, c.Title, c.Price })
                    .FirstOrDefaultAsync();

                if (course != null)
                {
                    _context.TransactionDetails.Add(new TransactionDetail
                    {
                        TransactionId = transaction.Id,
                        CourseId = courseId,
                        Price = course.Price
                    });

                    _logger.LogInformation($"📦 Added transaction detail for course: {course.Title} (ID: {courseId})");

                    // ✅ Gửi thông báo cho seller
                    await _notificationService.SendPaymentSuccessNotificationAsync(
                        sellerId: course.SellerId,
                        amount: course.Price,
                        courseName: course.Title
                    );

                    _logger.LogInformation($"📧 Sent notification to seller ID: {course.SellerId}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Course not found: {courseId}");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ All transaction details saved successfully");

            // ✅ Xóa cache sau khi xử lý xong
            _cache.Remove($"payment_{orderId}");
            _logger.LogInformation($"🗑️ Removed payment info from cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error processing payment callback for orderId: {orderId}");
            throw;
        }
    }

    private static string HmacSHA256(string text, string key)
    {
        var encoding = new UTF8Encoding();
        byte[] keyByte = encoding.GetBytes(key);
        byte[] messageBytes = encoding.GetBytes(text);
        using var hmacsha256 = new HMACSHA256(keyByte);
        byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
    }
}

// ✅ Class để lưu vào cache (thay vì anonymous object)
public class PaymentCacheInfo
{
    public int BuyerId { get; set; }
    public List<int> CourseIds { get; set; } = new();
    public decimal Amount { get; set; }
}