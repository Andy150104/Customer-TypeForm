using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.DeleteTemplate;

public class DeleteTemplateCommand : ICommand<DeleteTemplateCommandResponse>
{
    public Guid TemplateId { get; set; }
}
