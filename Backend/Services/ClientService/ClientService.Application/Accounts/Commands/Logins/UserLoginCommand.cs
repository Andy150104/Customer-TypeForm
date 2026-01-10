using BuildingBlocks.CQRS;
using BaseService.Common.ApiEntities;

namespace ClientService.Application.Accounts.Commands.Logins;

public record UserLoginCommand(string UserName, string Password) : ICommand<UserLoginCommandResponse>;

public record UserLoginCommandResponse : AbstractApiResponse<UserLoginResponseEntity>
{
    public override UserLoginResponseEntity Response { get; set; } = null!;
}

public class UserLoginResponseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}
