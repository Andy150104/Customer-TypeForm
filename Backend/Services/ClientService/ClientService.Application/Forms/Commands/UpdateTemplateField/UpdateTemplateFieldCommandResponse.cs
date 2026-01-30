using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.UpdateTemplateField;

public record UpdateTemplateFieldCommandResponse : AbstractApiResponse<UpdateTemplateFieldResponseEntity>
{
    public override UpdateTemplateFieldResponseEntity Response { get; set; } = null!;
}

public class UpdateTemplateFieldResponseEntity
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = null!;
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UpdateTemplateFieldOptionResponseEntity>? Options { get; set; }
}

public class UpdateTemplateFieldOptionResponseEntity
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int Order { get; set; }
}
