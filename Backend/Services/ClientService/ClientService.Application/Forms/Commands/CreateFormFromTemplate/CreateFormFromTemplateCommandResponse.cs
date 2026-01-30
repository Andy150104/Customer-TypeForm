using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.CreateFormFromTemplate;

public record CreateFormFromTemplateCommandResponse : AbstractApiResponse<CreateFormFromTemplateResponseEntity>
{
    public override CreateFormFromTemplateResponseEntity Response { get; set; } = null!;
}

public class CreateFormFromTemplateResponseEntity
{
    public Guid FormId { get; set; }
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public bool IsPublished { get; set; }
    public int FieldCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
