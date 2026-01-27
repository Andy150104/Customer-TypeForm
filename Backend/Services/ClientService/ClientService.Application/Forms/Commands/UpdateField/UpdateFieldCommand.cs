using BuildingBlocks.CQRS;
using BaseService.Common.Utils.Const;

namespace ClientService.Application.Forms.Commands.UpdateField;

public class UpdateFieldCommand : ICommand<UpdateFieldCommandResponse>
{
    public Guid FieldId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public ConstantEnum.FieldType? Type { get; set; }
    public System.Text.Json.JsonDocument? Properties { get; set; }
    public bool? IsRequired { get; set; }
    public List<UpdateFieldOptionDto>? Options { get; set; }
}

public class UpdateFieldOptionDto
{
    public Guid? Id { get; set; } // If provided, update existing option; otherwise create new
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
}
