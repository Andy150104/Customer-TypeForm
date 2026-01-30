namespace ClientService.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid FormId { get; set; }

    public Guid? LatestSubmissionId { get; set; }

    public string Message { get; set; } = null!;

    public int Count { get; set; }

    public DateTime? FirstSubmissionAt { get; set; }

    public DateTime? LastSubmissionAt { get; set; }

    public bool IsRead { get; set; }

    // Navigation properties
    public BaseService.Domain.Entities.User? User { get; set; }
    public Form? Form { get; set; }
}
