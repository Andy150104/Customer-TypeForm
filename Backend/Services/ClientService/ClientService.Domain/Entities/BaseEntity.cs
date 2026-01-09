namespace ClientService.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public string? UpdatedBy { get; set; }
    
    public bool IsActive { get; set; } = true;
}
