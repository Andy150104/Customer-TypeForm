using ClientService.Application.Accounts.Commands.Registers;
using ClientService.Application.Accounts.Commands.Applications;
using ClientService.Application.Accounts.Commands.Logins;
using ClientService.Application.Accounts.Commands.GoogleLogins;

namespace ClientService.Application.Interfaces.AuthServices;

public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserRegisterCommandResponse> RegisterUserAsync(UserRegisterCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Login with username/password
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UserLoginCommandResponse> LoginAsync(UserLoginCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Login or register with Google
    /// If GoogleId exists, login. Otherwise, create new user.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GoogleLoginCommandResponse> GoogleLoginAsync(GoogleLoginCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Register a new OpenIddict Application (Client)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<RegisterApplicationCommandResponse> RegisterApplicationAsync(RegisterApplicationCommand request, CancellationToken cancellationToken);
}
