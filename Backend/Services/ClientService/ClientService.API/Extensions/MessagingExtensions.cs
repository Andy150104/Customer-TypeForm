using ClientService.Application.Accounts.Commands.Registers;
using MediatR;

namespace ClientService.API.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddClientServiceMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserRegisterCommand).Assembly));
        return services;
    }
}

