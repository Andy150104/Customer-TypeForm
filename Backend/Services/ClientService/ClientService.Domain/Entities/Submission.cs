using System.Text.Json;

namespace ClientService.Domain.Entities;

public class Submission : BaseEntity
{
    public Guid FormId { get; set; }
    
    public JsonDocument? MetaData { get; set; }
    
    // Navigation properties
    public Form? Form { get; set; }
    
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
