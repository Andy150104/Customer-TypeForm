namespace ClientService.Application.Notifications;

public class NotificationEvent
{
    public Guid NotificationId { get; set; }
    public Guid FormId { get; set; }
    public Guid? LatestSubmissionId { get; set; }
    public string Message { get; set; } = null!;
    public int Count { get; set; }
    public DateTime OccurredAt { get; set; }
}
