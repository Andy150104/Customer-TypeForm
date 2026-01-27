using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.UpdateField;

public record UpdateFieldCommandResponse : AbstractApiResponse<UpdateFieldResponseEntity>
{
    public override UpdateFieldResponseEntity Response { get; set; } = null!;
}

public class UpdateFieldResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public System.Text.Json.JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UpdateFieldOptionResponseEntity>? Options { get; set; }
}

public class UpdateFieldOptionResponseEntity
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
    public int Order { get; set; }
}
