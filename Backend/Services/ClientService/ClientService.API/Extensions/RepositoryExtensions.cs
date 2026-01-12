using BaseService.Application.Interfaces.Repositories;
using BaseService.Domain.Entities;
using BaseService.Infrastructure.Repositories;
using ClientService.Application.Interfaces.AuthServices;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Application.Interfaces.TokenServices;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Implements;

namespace ClientService.API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddClientServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICommandRepository<User>, CommandRepository<User>>();
        services.AddScoped<ICommandRepository<Role>, CommandRepository<Role>>();
        services.AddScoped<ICommandRepository<Form>, CommandRepository<Form>>();
        return services;
    }

    public static IServiceCollection AddClientServiceApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFormService, FormService>();
        return services;
    }
}

