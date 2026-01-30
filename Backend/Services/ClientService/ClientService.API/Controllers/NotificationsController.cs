using System.Text.Json;
using BaseService.Application.Interfaces.IdentityHepers;
using ClientService.Application.Interfaces.NotificationServices;
using ClientService.Application.Notifications.Queries.GetNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace ClientService.API.Controllers.Notifications;

[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationHub _notificationHub;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor
    /// </summary>
    public NotificationsController(
        INotificationHub notificationHub,
        IIdentityService identityService,
        INotificationService notificationService)
    {
        _notificationHub = notificationHub;
        _identityService = identityService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Stream notifications via SSE
    /// </summary>
    [HttpGet("stream")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task Stream(CancellationToken cancellationToken)
    {
        var currentUser = _identityService.GetCurrentUser();
        if (currentUser == null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var subscription = _notificationHub.Subscribe(currentUser.UserId);

        try
        {
            await foreach (var notificationEvent in subscription.Reader.ReadAllAsync(cancellationToken))
            {
                var payload = JsonSerializer.Serialize(notificationEvent);
                await Response.WriteAsync("event: notification\n", cancellationToken);
                await Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
        finally
        {
            _notificationHub.Unsubscribe(subscription);
        }
    }

    /// <summary>
    /// Get notifications
    /// </summary>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<GetNotificationsQueryResponse> GetNotifications(CancellationToken cancellationToken)
    {
        var request = new GetNotificationsQuery();
        return await _notificationService.GetNotificationsAsync(request, cancellationToken);
    }
}
