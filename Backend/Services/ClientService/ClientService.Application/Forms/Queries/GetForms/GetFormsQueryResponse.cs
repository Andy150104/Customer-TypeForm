using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Queries.GetForms;

public record GetFormsQueryResponse : AbstractApiResponse<List<FormResponseEntity>>
{
    public override List<FormResponseEntity> Response { get; set; } = new();
}

public class FormResponseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
