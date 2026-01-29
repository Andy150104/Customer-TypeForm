using BaseService.Common.ApiEntities;
using System.Text.Json;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;

namespace ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;

public record GetFormWithFieldsAndLogicQueryResponse : AbstractApiResponse<FormWithFieldsAndLogicResponseEntity>
{
    public override FormWithFieldsAndLogicResponseEntity Response { get; set; } = null!;
}

public class FormWithFieldsAndLogicResponseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FieldWithLogicResponseEntity> Fields { get; set; } = new();
}

public class FieldWithLogicResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = null!;
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<LogicRuleResponseEntity> LogicRules { get; set; } = new();
    public Guid? DefaultNextFieldId { get; set; } // Field tiếp theo theo Order (mặc định)
    public List<FieldOptionResponseEntity>? Options { get; set; }
}

public class LogicRuleResponseEntity
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string Condition { get; set; } = null!;
    public string? Value { get; set; }
    public Guid? DestinationFieldId { get; set; }
    public int Order { get; set; }
    public Guid? LogicGroupId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
