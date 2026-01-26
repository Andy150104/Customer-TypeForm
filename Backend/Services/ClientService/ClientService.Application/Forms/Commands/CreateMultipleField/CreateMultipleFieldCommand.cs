using BuildingBlocks.CQRS;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateField;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.CreateMultipleField;

public class CreateMultipleFieldCommand : ICommand<CreateMultipleFieldCommandResponse>
{
    public Guid FormId { get; set; }
    public List<FieldDataDto> Fields { get; set; } = new();
}

public class FieldDataDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public ConstantEnum.FieldType Type { get; set; }
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; } = false;
    public List<FieldOptionDto>? Options { get; set; }
}
