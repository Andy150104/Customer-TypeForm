using BuildingBlocks.CQRS;
using ClientService.Domain.Entities.Enums;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.CreateField;

public class CreateFieldCommand : ICommand<CreateFieldCommandResponse>
{
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public FieldType Type { get; set; }
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; } = false;
}
