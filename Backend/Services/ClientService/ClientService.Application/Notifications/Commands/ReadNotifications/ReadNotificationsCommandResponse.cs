using BaseService.Common.ApiEntities;

namespace ClientService.Application.Notifications.Commands.ReadNotifications;

public record ReadNotificationsCommandResponse : AbstractApiResponse<List<Guid>>
{
    public override List<Guid> Response { get; set; } = new();
}
