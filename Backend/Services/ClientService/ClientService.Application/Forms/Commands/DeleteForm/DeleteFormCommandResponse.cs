using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.DeleteForm;

public record DeleteFormCommandResponse : AbstractApiResponse<DeleteFormResponseEntity>
{
    public override DeleteFormResponseEntity Response { get; set; } = null!;
}

public class DeleteFormResponseEntity
{
    public Guid Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
