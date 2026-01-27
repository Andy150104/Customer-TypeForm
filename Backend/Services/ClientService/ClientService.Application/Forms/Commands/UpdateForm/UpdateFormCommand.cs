using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.UpdateForm;

public class UpdateFormCommand : ICommand<UpdateFormCommandResponse>
{
    public Guid FormId { get; set; }
    public string? Title { get; set; }
}
