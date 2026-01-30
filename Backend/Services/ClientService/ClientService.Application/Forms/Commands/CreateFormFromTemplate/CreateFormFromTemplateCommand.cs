using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.CreateFormFromTemplate;

public class CreateFormFromTemplateCommand : ICommand<CreateFormFromTemplateCommandResponse>
{
    public Guid TemplateId { get; set; }
    public string? Title { get; set; }
}
