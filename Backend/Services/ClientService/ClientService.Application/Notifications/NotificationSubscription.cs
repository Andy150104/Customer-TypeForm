using System.Threading.Channels;

namespace ClientService.Application.Notifications;

public class NotificationSubscription
{
    public NotificationSubscription(Guid userId, Guid subscriptionId, ChannelReader<NotificationEvent> reader)
    {
        UserId = userId;
        SubscriptionId = subscriptionId;
        Reader = reader;
    }

    public Guid UserId { get; }
    public Guid SubscriptionId { get; }
    public ChannelReader<NotificationEvent> Reader { get; }
}
