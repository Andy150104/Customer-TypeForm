namespace BaseService.Domain.Entities;

public class User : BaseEntity
{
    public Guid? RoleId { get; set; }
    
    public string Email { get; set; } = null!;
    
    public string? Name { get; set; }
    
    public string? Avatar { get; set; }
    
    public string? GoogleId { get; set; }
    
    // Navigation properties
    public Role? Role { get; set; }
}
