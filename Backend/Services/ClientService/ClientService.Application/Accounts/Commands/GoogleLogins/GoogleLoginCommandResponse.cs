using BaseService.Common.ApiEntities;
using ClientService.Application.Dtos;

namespace ClientService.Application.Accounts.Commands.GoogleLogins;

public record GoogleLoginCommandResponse : AbstractApiResponse<GoogleLoginResponseEntity>
{
    public override GoogleLoginResponseEntity Response { get; set; } = null!;
}

public class GoogleLoginResponseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public bool IsNewUser { get; set; } // Indicates if user was just created
}
