using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.UpdateTemplate;

public class UpdateTemplateCommand : ICommand<UpdateTemplateCommandResponse>
{
    public Guid TemplateId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
