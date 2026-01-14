using System.Text.Json;

namespace ClientService.Domain.Entities;

public class Answer : BaseEntity
{
    public Guid SubmissionId { get; set; }
    
    public Guid FieldId { get; set; }
    
    public JsonDocument Value { get; set; } = null!;
    
    /// <summary>
    /// FieldOptionId for Select/MultiSelect/Radio fields
    /// Null for text-based fields (Text, Number, Email, etc.)
    /// </summary>
    public Guid? FieldOptionId { get; set; }
    
    // Navigation properties
    public Submission? Submission { get; set; }
    
    public Field? Field { get; set; }
    
    public FieldOption? FieldOption { get; set; }
}
