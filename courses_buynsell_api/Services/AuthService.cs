using courses_buynsell_api.Config;
using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs.Auth;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Helpers;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace courses_buynsell_api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwt;
    private readonly IEmailService _emailService;
    private readonly int _refreshTokenExpiryDays = 7;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwt, IEmailService emailService)
    {
        _context = context;
        _jwt = jwt.Value;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new BadRequestException("Email already exists.");

        var verificationToken = TokenHelper.GenerateRefreshToken();

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            Role = dto.Role,
            IsEmailVerified = true,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        //await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

        // KHÔNG TRẢ TOKEN, chỉ trả thông tin cơ bản
        return new AuthResponseDto
        {
            Token = string.Empty,
            RefreshToken = string.Empty,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Incorrect email or password.");

        if (!user.IsEmailVerified)
            throw new UnauthorizedException("Please verify your email first.");

        return await GenerateJwtToken(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token. Please log in again.");
        return await GenerateJwtToken(user);
    }

    public async Task VerifyEmailAsync(string token)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null || user.EmailVerificationTokenExpiry <= DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired verification token.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return; // Don't reveal if email exists

        var otp = OtpHelper.GenerateOtp(); // Tạo OTP 6 số
        user.PasswordResetToken = otp;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15); // OTP có hiệu lực 15 phút
        await _context.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(email, otp);
    }

    public async Task CheckOTPAsync(string email, string OTP)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == OTP);

        if (user == null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired OTP.");
    }

    public async Task ResetPasswordAsync(string OTP, string newPassword, string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == OTP && u.Email == email);

        if (user == null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            throw new BadRequestException("Invalid or expired reset token.");

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _context.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified
            })
            .FirstOrDefaultAsync();

        if (user == null)
            throw new NotFoundException("User not found.");

        return user;
    }

    private async Task<AuthResponseDto> GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds
        );

        var refreshToken = TokenHelper.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? "" : user.PhoneNumber,
            Image = string.IsNullOrEmpty(user.AvatarUrl) ? "" : user.AvatarUrl
        };
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) return; // Không lộ info email tồn tại hay không
        if (user.IsEmailVerified) return; // Đã verify thì không gửi nữa

        // Tạo token mới
        var newToken = TokenHelper.GenerateRefreshToken();
        user.EmailVerificationToken = newToken;
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await _context.SaveChangesAsync();

        // Gửi email
        await _emailService.SendVerificationEmailAsync(user.Email, newToken);
    }

}