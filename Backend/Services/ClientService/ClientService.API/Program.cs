using BaseService.Common.Settings;
using ClientService.API.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
EnvLoader.Load();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
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
