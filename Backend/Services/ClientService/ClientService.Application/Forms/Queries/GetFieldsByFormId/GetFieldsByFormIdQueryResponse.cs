using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Queries.GetFieldsByFormId;

public record GetFieldsByFormIdQueryResponse : AbstractApiResponse<List<FieldResponseEntity>>
{
    public override List<FieldResponseEntity> Response { get; set; } = new();
}

public class FieldResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FieldOptionResponseEntity>? Options { get; set; }
}

public class FieldOptionResponseEntity
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int Order { get; set; }
}
