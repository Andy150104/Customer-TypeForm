using System.Text.Json;
using BaseService.Common.Utils.Const;

namespace ClientService.Domain.Entities;

public class FormTemplateField : BaseEntity
{
    public Guid TemplateId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public ConstantEnum.FieldType Type { get; set; }

    public JsonDocument? Properties { get; set; }

    public bool IsRequired { get; set; }

    public int Order { get; set; }

    public FormTemplate? Template { get; set; }

    public ICollection<FormTemplateFieldOption> Options { get; set; } = new List<FormTemplateFieldOption>();
}
