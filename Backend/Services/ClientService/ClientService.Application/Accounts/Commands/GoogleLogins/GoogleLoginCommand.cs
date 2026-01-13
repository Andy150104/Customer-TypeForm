using BuildingBlocks.CQRS;

namespace ClientService.Application.Accounts.Commands.GoogleLogins;

public class GoogleLoginCommand : ICommand<GoogleLoginCommandResponse>
{
    public string GoogleId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? Email { get; set; }
}
