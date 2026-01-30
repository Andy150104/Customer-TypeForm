using BuildingBlocks.CQRS;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.UpdateTemplate;

public class UpdateTemplateCommand : ICommand<UpdateTemplateCommandResponse>
{
    public Guid TemplateId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
}
