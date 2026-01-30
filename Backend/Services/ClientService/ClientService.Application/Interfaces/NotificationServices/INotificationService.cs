namespace ClientService.Application.Interfaces.NotificationServices;

using ClientService.Application.Notifications.Queries.GetNotifications;

public interface INotificationService
{
    Task CreateSubmissionNotificationAsync(Guid userId, Guid formId, string formTitle, Guid submissionId, CancellationToken cancellationToken);

    Task<GetNotificationsQueryResponse> GetNotificationsAsync(GetNotificationsQuery request, CancellationToken cancellationToken);
}
