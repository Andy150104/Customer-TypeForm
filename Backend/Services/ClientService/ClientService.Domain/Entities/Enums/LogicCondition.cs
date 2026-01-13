using System.Text.Json.Serialization;

namespace ClientService.Domain.Entities.Enums;

/// <summary>
/// Logic condition types for form field logic rules
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogicCondition
{
    /// <summary>
    /// Field value equals the specified value
    /// </summary>
    Is,
    
    /// <summary>
    /// Field value does not equal the specified value
    /// </summary>
    IsNot,
    
    /// <summary>
    /// Field value contains the specified value (for text fields)
    /// </summary>
    Contains,
    
    /// <summary>
    /// Field value does not contain the specified value
    /// </summary>
    DoesNotContain,
    
    /// <summary>
    /// Field value is greater than the specified value (for numbers/dates)
    /// </summary>
    GreaterThan,
    
    /// <summary>
    /// Field value is less than the specified value (for numbers/dates)
    /// </summary>
    LessThan,
    
    /// <summary>
    /// Field value is greater than or equal to the specified value
    /// </summary>
    GreaterThanOrEqual,
    
    /// <summary>
    /// Field value is less than or equal to the specified value
    /// </summary>
    LessThanOrEqual,
    
    /// <summary>
    /// Always true - used for else/default case
    /// </summary>
    Always
}
