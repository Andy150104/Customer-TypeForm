using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.DeleteField;

public class DeleteFieldCommand : ICommand<DeleteFieldCommandResponse>
{
    public Guid FieldId { get; set; }
}
