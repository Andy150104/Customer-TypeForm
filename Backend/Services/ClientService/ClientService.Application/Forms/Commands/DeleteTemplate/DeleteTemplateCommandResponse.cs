using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.DeleteTemplate;

public record DeleteTemplateCommandResponse : AbstractApiResponse<DeleteTemplateResponseEntity>
{
    public override DeleteTemplateResponseEntity Response { get; set; } = null!;
}

public class DeleteTemplateResponseEntity
{
    public Guid Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
