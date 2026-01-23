using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.CreateForm;

public class CreateFormCommand : ICommand<CreateFormCommandResponse>
{
    public string Title { get; set; } = null!;
}
