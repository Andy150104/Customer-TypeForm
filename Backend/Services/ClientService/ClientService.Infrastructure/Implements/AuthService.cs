using BaseService.Application.Interfaces.Repositories;
using BaseService.Application.Interfaces.Commons;
using BaseService.Common.Utils.Const;
using BaseService.Domain.Entities;
using ClientService.Application.Accounts.Commands.Applications;
using ClientService.Application.Accounts.Commands.Logins;
using ClientService.Application.Accounts.Commands.Registers;
using ClientService.Application.Accounts.Commands.GoogleLogins;
using ClientService.Application.Interfaces.AuthServices;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of IAuthService
/// </summary>
public class AuthService : IAuthService
{
    private readonly ICommandRepository<User> _userRepository;
    private readonly ICommandRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly ICommonLogic _commonLogic;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userRepository"></param>
    /// <param name="roleRepository"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="applicationManager"></param>
    public AuthService(
        ICommandRepository<User> userRepository,
        ICommandRepository<Role> roleRepository,
        IUnitOfWork unitOfWork,
        IOpenIddictApplicationManager applicationManager,
        ICommonLogic commonLogic)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _applicationManager = applicationManager;
        _commonLogic = commonLogic;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<UserRegisterCommandResponse> RegisterUserAsync(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var response = new UserRegisterCommandResponse();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Password is required.");
                return response;
            }

            // Check if user already exists
            var existingUser = await _userRepository
                .Find(u => u!.Email == request.Email && u.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingUser != null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E00000, "User with this email already exists.");
                return response;
            }

            // Get role if provided
            Role? role = null;
            if (request.RoleId.HasValue)
            {
                role = await _roleRepository
                    .Find(r => r!.Id == request.RoleId.Value && r.IsActive, cancellationToken: cancellationToken)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                Name = request.Name,
                Avatar = request.Avatar,
                GoogleId = request.GoogleId,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "User registered successfully.");
            response.Response = new UserRegisterResponseEntity
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Avatar = user.Avatar,
                RoleName = role?.Name,
                CreatedAt = user.CreatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while registering user: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Login with username/password (username = email)
    /// </summary>
    public async Task<UserLoginCommandResponse> LoginAsync(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var response = new UserLoginCommandResponse();

        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Username/Password is required.");
                return response;
            }

            var user = await _userRepository.FirstOrDefaultAsync(
                u => u!.Email == request.UserName && u.IsActive,
                cancellationToken,
                u => u.Role!);

            if (user == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11001, "User not found.");
                return response;
            }

            if (string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                response.Success = false;
                response.SetMessage(MessageId.E11002, "Password is incorrect.");
                return response;
            }

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Login");
            response.Response = new UserLoginResponseEntity
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.Name ?? user.Email,
                RoleName = user.Role?.Name ?? string.Empty,
                AvatarUrl = user.Avatar
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"Login failed: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Login or register with Google
    /// If GoogleId exists, login. Otherwise, create new user.
    /// </summary>
    public async Task<GoogleLoginCommandResponse> GoogleLoginAsync(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var response = new GoogleLoginCommandResponse();

        try
        {
            if (string.IsNullOrWhiteSpace(request.GoogleId))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "GoogleId is required.");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Name is required.");
                return response;
            }

            // Check if user with this GoogleId already exists
            var existingUser = await _userRepository.FirstOrDefaultAsync(
                u => u!.GoogleId == request.GoogleId && u.IsActive,
                cancellationToken,
                u => u.Role!);

            User user;
            bool isNewUser = false;

            if (existingUser != null)
            {
                // User exists, update name and avatar if provided
                user = existingUser;
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    user.Name = request.Name;
                }
                if (!string.IsNullOrWhiteSpace(request.Avatar))
                {
                    user.Avatar = request.Avatar;
                }
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = "Google Login";
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // User doesn't exist, create new user
                isNewUser = true;
                
                // Get default role (Student role)
                var defaultRole = await _roleRepository
                    .Find(r => r!.NormalizedName == "USER" && r.IsActive, cancellationToken: cancellationToken)
                    .FirstOrDefaultAsync(cancellationToken);

                // Generate email if not provided (use GoogleId as fallback)
                var email = request.Email ?? $"{request.GoogleId}@google.local";

                // Check if email already exists
                var emailExists = await _userRepository
                    .Find(u => u!.Email == email && u.IsActive, cancellationToken: cancellationToken)
                    .AnyAsync(cancellationToken);

                if (emailExists)
                {
                    // If email exists, append GoogleId to make it unique
                    email = $"{request.GoogleId}_{email}";
                }

                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = request.Name,
                    Avatar = request.Avatar,
                    GoogleId = request.GoogleId,
                    RoleId = defaultRole?.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "Google Login",
                    UpdatedBy = "Google Login"
                };

                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload with role
                var reloadedUser = await _userRepository.FirstOrDefaultAsync(
                    u => u!.Id == user.Id && u.IsActive,
                    cancellationToken,
                    u => u.Role!);
                
                if (reloadedUser != null)
                {
                    user = reloadedUser;
                }
            }

            response.Success = true;
            response.SetMessage(MessageId.I00001, isNewUser ? "User created and logged in successfully." : "Login successful.");
            response.Response = new GoogleLoginResponseEntity
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.Name ?? user.Email,
                RoleName = user.Role?.Name ?? string.Empty,
                AvatarUrl = user.Avatar,
                IsNewUser = isNewUser
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"Google login failed: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Register a new OpenIddict Application (Client)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<RegisterApplicationCommandResponse> RegisterApplicationAsync(RegisterApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new RegisterApplicationCommandResponse();

        try
        {
            // Check if application already exists
            var existingApp = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken);
            if (existingApp != null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E00000, "Application with this ClientId already exists.");
                return response;
            }

            // Default permissions if not provided
            var permissions = request.Permissions ?? new List<string>
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Introspection,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + OpenIddictConstants.Scopes.OfflineAccess
            };

            // Create application descriptor
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                DisplayName = request.DisplayName,
                ClientType = request.ClientType ?? OpenIddictConstants.ClientTypes.Confidential,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit
            };

            // Add permissions
            foreach (var permission in permissions)
            {
                descriptor.Permissions.Add(permission);
            }

            // Add redirect URIs if provided
            if (request.RedirectUris != null)
            {
                foreach (var uri in request.RedirectUris)
                {
                    descriptor.RedirectUris.Add(new Uri(uri));
                }
            }

            // Add post logout redirect URIs if provided
            if (request.PostLogoutRedirectUris != null)
            {
                foreach (var uri in request.PostLogoutRedirectUris)
                {
                    descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
                }
            }

            // Create the application
            var application = await _applicationManager.CreateAsync(descriptor, cancellationToken);

            // Get the application ID
            var applicationId = await _applicationManager.GetIdAsync(application, cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Application registered successfully.");
            response.Response = new RegisterApplicationResponseEntity
            {
                ApplicationId = Guid.Parse(applicationId!),
                ClientId = request.ClientId,
                DisplayName = request.DisplayName,
                ClientType = request.ClientType ?? OpenIddictConstants.ClientTypes.Confidential,
                CreatedAt = DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while registering application: {ex.Message}");
            return response;
        }
    }
}
