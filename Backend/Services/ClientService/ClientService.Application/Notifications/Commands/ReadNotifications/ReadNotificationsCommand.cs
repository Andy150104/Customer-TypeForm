using BuildingBlocks.CQRS;

namespace ClientService.Application.Notifications.Commands.ReadNotifications;

public class ReadNotificationsCommand : ICommand<ReadNotificationsCommandResponse>
{
    public List<Guid> NotificationIds { get; set; } = new();
}
