using BaseService.Common.ApiEntities;
using System.Text.Json;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;

namespace ClientService.Application.Forms.Queries.GetTemplateWithFields;

public record GetTemplateWithFieldsQueryResponse : AbstractApiResponse<TemplateWithFieldsResponseEntity>
{
    public override TemplateWithFieldsResponseEntity Response { get; set; } = null!;
}

public class TemplateWithFieldsResponseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TemplateFieldResponseEntity> Fields { get; set; } = new();
}

public class TemplateFieldResponseEntity
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = null!;
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FieldOptionResponseEntity>? Options { get; set; }
}
