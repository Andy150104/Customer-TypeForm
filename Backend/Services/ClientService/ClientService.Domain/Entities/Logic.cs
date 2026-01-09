namespace ClientService.Domain.Entities;

public class Logic : BaseEntity
{
    public Guid FieldId { get; set; }
    
    public string Condition { get; set; } = null!;
    
    public string? Value { get; set; }
    
    public Guid? DestinationFieldId { get; set; }
    
    // Navigation properties
    public Field? Field { get; set; }
    
    public Field? DestinationField { get; set; }
}
