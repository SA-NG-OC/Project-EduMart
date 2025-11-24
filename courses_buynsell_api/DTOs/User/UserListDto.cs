namespace courses_buynsell_api.DTOs.User;

public class UserListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string PhoneNumber { get; set; } = String.Empty;
    public string Role { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
}