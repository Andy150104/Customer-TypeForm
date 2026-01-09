using System.Text.Json;
using BaseService.Domain.Entities;

namespace ClientService.Domain.Entities;

public class Form : BaseEntity
{
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = null!;
    
    public string? Slug { get; set; }
    
    public JsonDocument? ThemeConfig { get; set; }
    
    public JsonDocument? Settings { get; set; }
    
    public bool IsPublished { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    
    public ICollection<Field> Fields { get; set; } = new List<Field>();
    
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
