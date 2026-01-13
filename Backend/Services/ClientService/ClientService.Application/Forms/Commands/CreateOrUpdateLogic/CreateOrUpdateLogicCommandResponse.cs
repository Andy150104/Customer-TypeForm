using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.CreateOrUpdateLogic;

public record CreateOrUpdateLogicCommandResponse : AbstractApiResponse<LogicResponseEntity>
{
    public override LogicResponseEntity Response { get; set; } = null!;
}

public class LogicResponseEntity
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string Condition { get; set; } = null!;
    public string? Value { get; set; }
    public Guid? DestinationFieldId { get; set; }
    public int Order { get; set; }
    public Guid? LogicGroupId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
