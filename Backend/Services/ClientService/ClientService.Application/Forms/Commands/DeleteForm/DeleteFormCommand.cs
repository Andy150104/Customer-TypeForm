using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.DeleteForm;

public class DeleteFormCommand : ICommand<DeleteFormCommandResponse>
{
    public Guid FormId { get; set; }
}
