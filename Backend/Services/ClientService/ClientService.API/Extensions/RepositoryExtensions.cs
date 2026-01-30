using BaseService.Application.Interfaces.Repositories;
using BaseService.Domain.Entities;
using BaseService.Infrastructure.Repositories;
using ClientService.Application.Interfaces.AuthServices;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Application.Interfaces.NotificationServices;
using ClientService.Application.Interfaces.TokenServices;
using ClientService.Domain.Entities;
using ClientService.Infrastructure.Implements;
using ClientService.Infrastructure.Notifications;

namespace ClientService.API.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddClientServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICommandRepository<User>, CommandRepository<User>>();
        services.AddScoped<ICommandRepository<Role>, CommandRepository<Role>>();
        services.AddScoped<ICommandRepository<Form>, CommandRepository<Form>>();
        services.AddScoped<ICommandRepository<FormTemplate>, CommandRepository<FormTemplate>>();
        services.AddScoped<ICommandRepository<FormTemplateField>, CommandRepository<FormTemplateField>>();
        services.AddScoped<ICommandRepository<FormTemplateFieldOption>, CommandRepository<FormTemplateFieldOption>>();
        services.AddScoped<ICommandRepository<Field>, CommandRepository<Field>>();
        services.AddScoped<ICommandRepository<FieldOption>, CommandRepository<FieldOption>>();
        services.AddScoped<ICommandRepository<Logic>, CommandRepository<Logic>>();
        services.AddScoped<ICommandRepository<Submission>, CommandRepository<Submission>>();
        services.AddScoped<ICommandRepository<Answer>, CommandRepository<Answer>>();
        services.AddScoped<ICommandRepository<Notification>, CommandRepository<Notification>>();
        return services;
    }

    public static IServiceCollection AddClientServiceApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<ILogicService, LogicService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<INotificationHub, NotificationHub>();
        return services;
    }
}

