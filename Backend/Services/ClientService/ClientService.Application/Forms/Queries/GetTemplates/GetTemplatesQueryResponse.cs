using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Queries.GetTemplates;

public record GetTemplatesQueryResponse : AbstractApiResponse<List<TemplateSummaryResponseEntity>>
{
    public override List<TemplateSummaryResponseEntity> Response { get; set; } = new();
}

public class TemplateSummaryResponseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int FieldCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
