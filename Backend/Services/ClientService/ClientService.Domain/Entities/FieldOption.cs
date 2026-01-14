namespace ClientService.Domain.Entities;

public class FieldOption : BaseEntity
{
    public Guid FieldId { get; set; }
    
    public string Label { get; set; } = null!;
    
    public string Value { get; set; } = null!;
    
    public int Order { get; set; }
    
    // Navigation properties
    public Field? Field { get; set; }
}
