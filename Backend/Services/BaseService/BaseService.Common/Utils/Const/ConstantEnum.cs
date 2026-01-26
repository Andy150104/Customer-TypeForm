using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BaseService.Common.Utils.Const;

public static class ConstantEnum
{

    public enum SubjectImprovementStatus
    {
        [Description("Đã đạt và đang cải thiện")]
        PassedAndImproving = 1,
        
        [Description("Chưa đạt và đang cải thiện")]
        NotPassedAndImproving = 2,
        
        [Description("Chưa học trên FAP và đang học khoá học")]
        NotStartedAndImproving = 4,
        
        [Description("Đã đạt và chỉ đánh giá")]
        PassedAndEvaluation = 5,
        
        [Description("Đã đạt điểm tốt")]
        PassedWithGoodGrade = 6,
         
        [Description("Đã hoàn thành")]
        Completed = 7,
        
        [Description("Bỏ qua")]
        Skipped = 8,
        [Description("Đang học")]
        Studying = 9
    }

    /// <summary>
    /// Form field types
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FieldType
    {
        Text,
        Number,
        Email,
        Phone,
        Date,
        Time,
        DateTime,
        Textarea,
        Select,
        MultiSelect,
        Checkbox,
        Radio,
        File,
        Rating,
        Scale,
        YesNo
    }

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
}