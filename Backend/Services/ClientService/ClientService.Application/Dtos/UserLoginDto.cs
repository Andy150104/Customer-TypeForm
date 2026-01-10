namespace ClientService.Application.Dtos;

public class UserLoginDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}
