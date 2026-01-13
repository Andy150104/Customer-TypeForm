using BuildingBlocks.CQRS;
using ClientService.Domain.Entities.Enums;

namespace ClientService.Application.Forms.Commands.CreateOrUpdateLogic;

public class CreateOrUpdateLogicCommand : ICommand<CreateOrUpdateLogicCommandResponse>
{
    public Guid FieldId { get; set; }
    public LogicCondition Condition { get; set; }
    public string? Value { get; set; }
    public Guid? DestinationFieldId { get; set; }
}
