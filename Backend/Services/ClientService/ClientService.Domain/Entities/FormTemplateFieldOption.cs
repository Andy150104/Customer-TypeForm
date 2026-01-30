namespace ClientService.Domain.Entities;

public class FormTemplateFieldOption : BaseEntity
{
    public Guid TemplateFieldId { get; set; }

    public string Label { get; set; } = null!;

    public string Value { get; set; } = null!;

    public int Order { get; set; }

    public FormTemplateField? TemplateField { get; set; }
}
