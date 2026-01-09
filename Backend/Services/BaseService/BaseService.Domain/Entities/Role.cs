namespace BaseService.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    
    public string NormalizedName { get; set; } = null!;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
