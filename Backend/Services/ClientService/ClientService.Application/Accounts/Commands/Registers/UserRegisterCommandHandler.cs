using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.AuthServices;

namespace ClientService.Application.Accounts.Commands.Registers;

/// <summary>
/// Handler for UserRegisterCommand
/// </summary>
/// <param name="authService"></param>
public class UserRegisterCommandHandler(IAuthService authService) : ICommandHandler<UserRegisterCommand, UserRegisterCommandResponse>
{
    public async Task<UserRegisterCommandResponse> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterUserAsync(request, cancellationToken);
    }
}
