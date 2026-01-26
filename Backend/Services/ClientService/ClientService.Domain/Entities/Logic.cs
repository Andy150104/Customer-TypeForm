using BaseService.Common.Utils.Const;

namespace ClientService.Domain.Entities;

public class Logic : BaseEntity
{
    public Guid FieldId { get; set; }
    
    public ConstantEnum.LogicCondition Condition { get; set; }
    
    public string? Value { get; set; }
    
    public Guid? DestinationFieldId { get; set; }
    
    /// <summary>
    /// Order within the logic group (for if-else chain)
    /// Lower order = evaluated first (if condition)
    /// Higher order = evaluated later (else if / else)
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Logic group ID to group related if-else conditions together
    /// If null, it's a standalone logic rule
    /// </summary>
    public Guid? LogicGroupId { get; set; }
    
    // Navigation properties
    public Field? Field { get; set; }
    
    public Field? DestinationField { get; set; }
}
