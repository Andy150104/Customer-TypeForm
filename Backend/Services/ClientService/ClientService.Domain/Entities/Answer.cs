using System.Text.Json;

namespace ClientService.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid SubmissionId { get; set; }
    
    public Guid FieldId { get; set; }
    
    public JsonDocument Value { get; set; } = null!;
    
    // Navigation properties
    public Submission? Submission { get; set; }
    
    public Field? Field { get; set; }
}
