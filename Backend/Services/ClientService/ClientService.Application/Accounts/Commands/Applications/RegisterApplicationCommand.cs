using BuildingBlocks.CQRS;
using BaseService.Common.ApiEntities;

namespace ClientService.Application.Accounts.Commands.Applications;

/// <summary>
/// Command to register a new OpenIddict Application (Client)
/// </summary>
/// <param name="ClientId"></param>
/// <param name="ClientSecret"></param>
/// <param name="DisplayName"></param>
/// <param name="ClientType"></param>
/// <param name="Permissions"></param>
/// <param name="RedirectUris"></param>
/// <param name="PostLogoutRedirectUris"></param>
public record RegisterApplicationCommand(
    string ClientId,
    string ClientSecret,
    string DisplayName,
    string? ClientType = "confidential",
    List<string>? Permissions = null,
    List<string>? RedirectUris = null,
    List<string>? PostLogoutRedirectUris = null
) : ICommand<RegisterApplicationCommandResponse>;

/// <summary>
/// Response for RegisterApplicationCommand
/// </summary>
public record RegisterApplicationCommandResponse : AbstractApiResponse<RegisterApplicationResponseEntity>
{
    public override RegisterApplicationResponseEntity Response { get; set; } = null!;
}

/// <summary>
/// Entity response for RegisterApplicationCommand
/// </summary>
public class RegisterApplicationResponseEntity
{
    public Guid ApplicationId { get; set; }
    public string ClientId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ClientType { get; set; }
    public DateTime CreatedAt { get; set; }
}
