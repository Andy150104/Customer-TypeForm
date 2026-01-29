using BuildingBlocks.CQRS;
using BaseService.Common.Utils.Const;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.CreateField;

public class CreateFieldCommand : ICommand<CreateFieldCommandResponse>
{
    public Guid FormId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ConstantEnum.FieldType Type { get; set; }
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; } = false;
    public List<FieldOptionDto>? Options { get; set; }
}

public class FieldOptionDto
{
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
}
