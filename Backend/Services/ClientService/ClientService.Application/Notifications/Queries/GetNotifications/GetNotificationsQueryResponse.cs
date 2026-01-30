using BaseService.Common.ApiEntities;

namespace ClientService.Application.Notifications.Queries.GetNotifications;

public record GetNotificationsQueryResponse : AbstractApiResponse<List<NotificationResponseEntity>>
{
    public override List<NotificationResponseEntity> Response { get; set; } = new();
}

public class NotificationResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public Guid? LatestSubmissionId { get; set; }
    public string Message { get; set; } = null!;
    public int Count { get; set; }
    public DateTime? FirstSubmissionAt { get; set; }
    public DateTime? LastSubmissionAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
