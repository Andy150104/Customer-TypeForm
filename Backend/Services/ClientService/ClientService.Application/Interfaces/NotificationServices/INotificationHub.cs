using ClientService.Application.Notifications;

namespace ClientService.Application.Interfaces.NotificationServices;

public interface INotificationHub
{
    NotificationSubscription Subscribe(Guid userId);
    void Unsubscribe(NotificationSubscription subscription);
    void Publish(Guid userId, NotificationEvent notificationEvent);
}
