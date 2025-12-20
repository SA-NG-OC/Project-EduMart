using courses_buynsell_api.DTOs.Auth;

namespace courses_buynsell_api.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task VerifyEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string OTP, string newPassword, string email);
    Task CheckOTPAsync(string email, string OTP);
    Task<UserProfileDto> GetCurrentUserAsync(int userId);
    Task ResendVerificationEmailAsync(string email);

}