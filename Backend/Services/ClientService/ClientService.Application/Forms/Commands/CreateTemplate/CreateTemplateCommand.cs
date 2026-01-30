using BuildingBlocks.CQRS;
using BaseService.Common.Utils.Const;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.CreateTemplate;

public class CreateTemplateCommand : ICommand<CreateTemplateCommandResponse>
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public JsonDocument? ThemeConfig { get; set; }
    public JsonDocument? Settings { get; set; }
    public List<TemplateFieldDataDto> Fields { get; set; } = new();
}

public class TemplateFieldDataDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public ConstantEnum.FieldType Type { get; set; }
    public JsonDocument? Properties { get; set; }
    public bool IsRequired { get; set; } = false;
    public List<TemplateFieldOptionDto>? Options { get; set; }
}

public class TemplateFieldOptionDto
{
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
}
