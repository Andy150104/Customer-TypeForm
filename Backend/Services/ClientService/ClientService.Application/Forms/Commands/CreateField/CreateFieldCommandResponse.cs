using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.CreateField;

public record CreateFieldCommandResponse : AbstractApiResponse<CreateFieldResponseEntity>
{
    public override CreateFieldResponseEntity Response { get; set; } = null!;
}

public class CreateFieldResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public System.Text.Json.JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
}
