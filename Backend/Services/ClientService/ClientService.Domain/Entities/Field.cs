using System.Text.Json;
using BaseService.Common.Utils.Const;

namespace ClientService.Domain.Entities;

public class Field : BaseEntity
{
    public Guid FormId { get; set; }
    
    public string Title { get; set; } = null!;
    
    public string? Description { get; set; }
    
    public ConstantEnum.FieldType Type { get; set; }
    
    public JsonDocument? Properties { get; set; }
    
    public bool IsRequired { get; set; }
    
    public int Order { get; set; }
    
    // Navigation properties
    public Form? Form { get; set; }
    
    public ICollection<Logic> LogicRules { get; set; } = new List<Logic>();
    
    public ICollection<Logic> DestinationLogicRules { get; set; } = new List<Logic>();
    
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    
    public ICollection<FieldOption> Options { get; set; } = new List<FieldOption>();
}
