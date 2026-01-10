using BaseService.Application.Interfaces.Commons;
using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Infrastructure.Identities;
using BaseService.Infrastructure.Logics;

namespace ClientService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientServiceCommonServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICommonLogic, CommonLogic>();
        return services;
    }
}

