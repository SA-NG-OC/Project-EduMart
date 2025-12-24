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
    private readonly ICartService _cartService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        IOptions<MomoOptions> momoOptions,
        AppDbContext context,
        INotificationService notificationService,
        IMemoryCache cache,
        ICartService cartService,
        ILogger<CheckoutService> logger)
    {
        _momo = momoOptions.Value;
        _context = context;
        _notificationService = notificationService;
        _cache = cache;
        _cartService = cartService;
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
        try
        {
            _logger.LogInformation($"🔔 Received MoMo callback: {JsonConvert.SerializeObject(formData)}");

            // 1. Kiểm tra có signature từ MoMo gửi về không
            if (!formData.TryGetValue("signature", out string? receivedSignature) || string.IsNullOrEmpty(receivedSignature))
            {
                _logger.LogError("❌ No signature in callback data");
                return;
            }

            // 2. Lấy các tham số cần thiết để hash
            // Lưu ý: Dùng TryGetValue hoặc lấy trực tiếp nhưng cần đảm bảo dữ liệu không null
            string partnerCode = formData.GetValueOrDefault("partnerCode", "");
            string orderId = formData.GetValueOrDefault("orderId", "");
            string requestId = formData.GetValueOrDefault("requestId", "");
            string amount = formData.GetValueOrDefault("amount", "");
            string orderInfo = formData.GetValueOrDefault("orderInfo", "");
            string orderType = formData.GetValueOrDefault("orderType", "");
            string transId = formData.GetValueOrDefault("transId", "");
            string resultCode = formData.GetValueOrDefault("resultCode", "");
            string message = formData.GetValueOrDefault("message", "");
            string payType = formData.GetValueOrDefault("payType", "");
            string responseTime = formData.GetValueOrDefault("responseTime", "");
            string extraData = formData.GetValueOrDefault("extraData", "");

            // 3. TẠO CHUỖI RAW HASH ĐÚNG CHUẨN MOMO
            // Quy tắc: Sắp xếp a-z. QUAN TRỌNG: Phải đưa _momo.AccessKey vào đầu tiên
            string rawHash = $"accessKey={_momo.AccessKey}" +
                             $"&amount={amount}" +
                             $"&extraData={extraData}" +
                             $"&message={message}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo={orderInfo}" +
                             $"&orderType={orderType}" +
                             $"&partnerCode={partnerCode}" +
                             $"&payType={payType}" +
                             $"&requestId={requestId}" +
                             $"&responseTime={responseTime}" +
                             $"&resultCode={resultCode}" +
                             $"&transId={transId}";

            _logger.LogInformation($"🔐 Raw hash for verification: {rawHash}");

            // 4. Tạo signature của mình để so sánh
            string mySignature = HmacSHA256(rawHash, _momo.SecretKey);

            _logger.LogInformation($"✅ My signature: {mySignature}");
            _logger.LogInformation($"📩 MoMo signature: {receivedSignature}");

            // 5. So sánh signature
            if (mySignature != receivedSignature)
            {
                _logger.LogError("❌ Signature verification FAILED! Mismatch detected.");
                return;
            }

            // 6. Kiểm tra resultCode (0 = Thành công)
            if (resultCode != "0")
            {
                _logger.LogWarning($"⚠️ Payment failed or pending. Result code: {resultCode}, Message: {message}");
                return;
            }

            _logger.LogInformation($"✅ Signature verified & Payment success for orderId: {orderId}");

            // 7. Lấy thông tin từ Cache
            if (!_cache.TryGetValue($"payment_{orderId}", out PaymentCacheInfo? paymentInfo) || paymentInfo == null)
            {
                _logger.LogError($"❌ Payment info not found in cache for orderId: {orderId}. Transaction might be lost.");
                return;
            }

            _logger.LogInformation($"💾 Retrieved info from cache: BuyerId={paymentInfo.BuyerId}, CourseIds={string.Join(",", paymentInfo.CourseIds)}");

            // 8. Lưu Transaction vào DB
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
            await _context.SaveChangesAsync(); // Lưu để lấy transaction ID

            _logger.LogInformation($"💰 Created transaction ID: {transaction.Id}");

            // 9. Lưu Transaction Detail và Gửi thông báo
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

                    // Gửi thông báo cho Seller
                    await _notificationService.SendPaymentSuccessNotificationAsync(
                        sellerId: course.SellerId,
                        amount: course.Price,
                        courseName: course.Title
                    );
                    await _cartService.RemoveFromCartAsync(paymentInfo.BuyerId, courseId);

                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ All transaction details saved successfully");

            // 10. Xóa cache
            _cache.Remove($"payment_{orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Exception in HandleMomoCallbackAsync");
            throw; // Ném lỗi để phía Controller biết đường trả về HTTP 500 nếu cần
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