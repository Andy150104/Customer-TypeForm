using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.DeleteLogic;

public record DeleteLogicCommandResponse : AbstractApiResponse<DeleteLogicResponseEntity>
{
    public override DeleteLogicResponseEntity Response { get; set; } = null!;
}

public class DeleteLogicResponseEntity
{
    public Guid Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
