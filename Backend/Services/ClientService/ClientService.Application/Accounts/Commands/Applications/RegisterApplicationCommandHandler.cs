using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.AuthServices;

namespace ClientService.Application.Accounts.Commands.Applications;

/// <summary>
/// Handler for RegisterApplicationCommand
/// </summary>
/// <param name="authService"></param>
public class RegisterApplicationCommandHandler(IAuthService authService) : ICommandHandler<RegisterApplicationCommand, RegisterApplicationCommandResponse>
{
    public async Task<RegisterApplicationCommandResponse> Handle(RegisterApplicationCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterApplicationAsync(request, cancellationToken);
    }
}
