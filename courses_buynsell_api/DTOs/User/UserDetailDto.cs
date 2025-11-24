namespace courses_buynsell_api.DTOs.User;

public class UserDetailDto
{
    public string FullName { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = String.Empty;
    public string? AvatarUrl { get; set; }
}