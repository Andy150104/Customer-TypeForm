using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.NotificationServices;

namespace ClientService.Application.Notifications.Queries.GetNotifications;

/// <summary>
/// Handler for GetNotificationsQuery
/// </summary>
public class GetNotificationsQueryHandler(INotificationService notificationService) : IQueryHandler<GetNotificationsQuery, GetNotificationsQueryResponse>
{
    /// <summary>
    /// Handle GetNotificationsQuery
    /// </summary>
    public async Task<GetNotificationsQueryResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await notificationService.GetNotificationsAsync(request, cancellationToken);
    }
}
