using BaseService.Common.Settings;
using ClientService.API.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
EnvLoader.Load();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to use string enum converter
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic model state validation
        // Validation errors will be handled manually in ApiControllerHelper.HandleRequest
        // This ensures consistent error response format across all endpoints
        options.SuppressModelStateInvalidFilter = true;
    });
builder.Services
    .AddClientServiceSwagger()
    .AddClientServiceCors()
    .AddClientServiceDatabase(builder.Configuration)
    .AddClientServiceOpenIddict()
    .AddClientServiceMediatR()
    .AddClientServiceRepositories()
    .AddClientServiceCommonServices()
    .AddClientServiceApplicationServices()
    .AddClientServiceAuthentication(builder.Configuration);

var app = builder.Build();

app
    .UseClientServiceSwagger()
    .UseClientServiceCors()
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.UseClientServiceDatabaseMigrations();

app.MapControllers();

app.Run();
