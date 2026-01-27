using BuildingBlocks.CQRS;
using BaseService.Common.Utils.Const;

namespace ClientService.Application.Forms.Commands.UpdateLogic;

public class UpdateLogicCommand : ICommand<UpdateLogicCommandResponse>
{
    public Guid LogicId { get; set; }
    public Guid FieldId { get; set; }
    public ConstantEnum.LogicCondition Condition { get; set; }
    public string? Value { get; set; }
    public Guid? DestinationFieldId { get; set; }
}
