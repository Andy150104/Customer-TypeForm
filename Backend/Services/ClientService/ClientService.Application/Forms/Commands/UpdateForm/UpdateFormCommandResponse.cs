using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.UpdateForm;

public record UpdateFormCommandResponse : AbstractApiResponse<UpdateFormResponseEntity>
{
    public override UpdateFormResponseEntity Response { get; set; } = null!;
}

public class UpdateFormResponseEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public System.Text.Json.JsonDocument? ThemeConfig { get; set; }
    public System.Text.Json.JsonDocument? Settings { get; set; }
    public bool IsPublished { get; set; }
    public DateTime UpdatedAt { get; set; }
}
