using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.UpdateLogic;

public record UpdateLogicCommandResponse : AbstractApiResponse<UpdateLogicResponseEntity>
{
    public override UpdateLogicResponseEntity Response { get; set; } = null!;
}

public class UpdateLogicResponseEntity
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string Condition { get; set; } = null!;
    public string? Value { get; set; }
    public Guid? DestinationFieldId { get; set; }
    public int Order { get; set; }
    public Guid? LogicGroupId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
