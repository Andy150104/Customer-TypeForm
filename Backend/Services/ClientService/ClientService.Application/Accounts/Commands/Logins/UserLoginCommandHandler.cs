using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.AuthServices;

namespace ClientService.Application.Accounts.Commands.Logins;

/// <summary>
/// Handler for UserLoginCommand
/// </summary>
/// <param name="authService"></param>
public class UserLoginCommandHandler(IAuthService authService) : ICommandHandler<UserLoginCommand, UserLoginCommandResponse>
{
    public async Task<UserLoginCommandResponse> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request, cancellationToken);
    }
}

