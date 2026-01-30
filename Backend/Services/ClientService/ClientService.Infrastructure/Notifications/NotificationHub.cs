using System.Collections.Concurrent;
using System.Threading.Channels;
using ClientService.Application.Interfaces.NotificationServices;
using ClientService.Application.Notifications;

namespace ClientService.Infrastructure.Notifications;

public class NotificationHub : INotificationHub
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, Channel<NotificationEvent>>> _subscriptions = new();

    public NotificationSubscription Subscribe(Guid userId)
    {
        var subscriptionId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<NotificationEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var userSubscriptions = _subscriptions.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, Channel<NotificationEvent>>());
        userSubscriptions[subscriptionId] = channel;

        return new NotificationSubscription(userId, subscriptionId, channel.Reader);
    }

    public void Unsubscribe(NotificationSubscription subscription)
    {
        if (_subscriptions.TryGetValue(subscription.UserId, out var userSubscriptions))
        {
            if (userSubscriptions.TryRemove(subscription.SubscriptionId, out var channel))
            {
                channel.Writer.TryComplete();
            }

            if (userSubscriptions.IsEmpty)
            {
                _subscriptions.TryRemove(subscription.UserId, out _);
            }
        }
    }

    public void Publish(Guid userId, NotificationEvent notificationEvent)
    {
        if (!_subscriptions.TryGetValue(userId, out var userSubscriptions))
        {
            return;
        }

        foreach (var channel in userSubscriptions.Values)
        {
            channel.Writer.TryWrite(notificationEvent);
        }
    }
}
