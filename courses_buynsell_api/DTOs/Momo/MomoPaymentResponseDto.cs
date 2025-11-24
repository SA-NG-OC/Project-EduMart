namespace courses_buynsell_api.DTOs.Momo;

public class MomoPaymentResponseDto
{
    public string PartnerCode { get; set; } = "";
    public string OrderId { get; set; } = "";
    public string RequestId { get; set; } = "";
    public long Amount { get; set; }
    public string ResponseTime { get; set; } = "";
    public string Message { get; set; } = "";
    public string ResultCode { get; set; } = "";
    public string PayUrl { get; set; } = "";
}
