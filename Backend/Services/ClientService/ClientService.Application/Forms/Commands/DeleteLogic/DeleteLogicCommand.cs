using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.DeleteLogic;

public class DeleteLogicCommand : ICommand<DeleteLogicCommandResponse>
{
    public Guid LogicId { get; set; }
    public Guid FieldId { get; set; }
}
