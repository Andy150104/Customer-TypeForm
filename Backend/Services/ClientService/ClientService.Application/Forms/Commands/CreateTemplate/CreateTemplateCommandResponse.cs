using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.CreateTemplate;

public record CreateTemplateCommandResponse : AbstractApiResponse<CreateTemplateResponseEntity>
{
    public override CreateTemplateResponseEntity Response { get; set; } = null!;
}

public class CreateTemplateResponseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int FieldCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
