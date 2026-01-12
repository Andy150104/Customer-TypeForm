using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.CreateForm;

public record CreateFormCommandResponse : AbstractApiResponse<CreateFormResponseEntity>
{
    public override CreateFormResponseEntity Response { get; set; } = null!;
}

public class CreateFormResponseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
