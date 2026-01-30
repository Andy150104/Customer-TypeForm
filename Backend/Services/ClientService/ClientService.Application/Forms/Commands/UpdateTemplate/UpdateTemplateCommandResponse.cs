using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.UpdateTemplate;

public record UpdateTemplateCommandResponse : AbstractApiResponse<UpdateTemplateResponseEntity>
{
    public override UpdateTemplateResponseEntity Response { get; set; } = null!;
}

public class UpdateTemplateResponseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
    public DateTime UpdatedAt { get; set; }
}
