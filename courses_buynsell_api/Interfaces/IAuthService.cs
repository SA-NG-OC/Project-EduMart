using courses_buynsell_api.DTOs.Auth;

namespace courses_buynsell_api.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
}
