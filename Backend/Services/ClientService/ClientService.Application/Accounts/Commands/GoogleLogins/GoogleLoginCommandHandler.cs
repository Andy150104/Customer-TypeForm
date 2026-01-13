using ClientService.Application.Interfaces.AuthServices;
using MediatR;

namespace ClientService.Application.Accounts.Commands.GoogleLogins;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, GoogleLoginCommandResponse>
{
    private readonly IAuthService _authService;

    public GoogleLoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<GoogleLoginCommandResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.GoogleLoginAsync(request, cancellationToken);
    }
}
