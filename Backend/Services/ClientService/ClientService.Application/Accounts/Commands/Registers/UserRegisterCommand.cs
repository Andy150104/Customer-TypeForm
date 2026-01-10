using BuildingBlocks.CQRS;
using BaseService.Common.ApiEntities;

namespace ClientService.Application.Accounts.Commands.Registers;

/// <summary>
/// Command to register a new user
/// </summary>
/// <param name="Email"></param>
/// <param name="Name"></param>
/// <param name="Avatar"></param>
/// <param name="GoogleId"></param>
/// <param name="RoleId"></param>
public record UserRegisterCommand(
    string Email,
    string Password,
    string? Name,
    string? Avatar,
    string? GoogleId,
    Guid? RoleId
) : ICommand<UserRegisterCommandResponse>;

/// <summary>
/// Response for UserRegisterCommand
/// </summary>
public record UserRegisterCommandResponse : AbstractApiResponse<UserRegisterResponseEntity>
{
    public override UserRegisterResponseEntity Response { get; set; } = null!;
}

/// <summary>
/// Entity response for UserRegisterCommand
/// </summary>
public class UserRegisterResponseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Avatar { get; set; }
    public string? RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
}
