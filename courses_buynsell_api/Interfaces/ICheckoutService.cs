using courses_buynsell_api.DTOs.Momo;
namespace courses_buynsell_api.Interfaces;

public interface ICheckoutService
{
    Task<string> CreateMomoPaymentAsync(CreateMomoPaymentRequestDto request, int buyerId);
    Task HandleMomoCallbackAsync(Dictionary<string, string> formData);
}