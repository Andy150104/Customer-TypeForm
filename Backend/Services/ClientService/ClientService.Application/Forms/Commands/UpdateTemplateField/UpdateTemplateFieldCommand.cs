using BaseService.Common.Utils.Const;
using BuildingBlocks.CQRS;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.UpdateTemplateField;

public class UpdateTemplateFieldCommand : ICommand<UpdateTemplateFieldCommandResponse>
{
    public Guid TemplateFieldId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ConstantEnum.FieldType? Type { get; set; }
    public JsonDocument? Properties { get; set; }
    public bool? IsRequired { get; set; }
    public List<UpdateTemplateFieldOptionDto>? Options { get; set; }
}

public class UpdateTemplateFieldOptionDto
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
}
