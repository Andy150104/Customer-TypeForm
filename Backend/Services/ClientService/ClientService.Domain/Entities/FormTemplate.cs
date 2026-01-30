using System.Text.Json;

namespace ClientService.Domain.Entities;

public class FormTemplate : BaseEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public JsonDocument? ThemeConfig { get; set; }

    public JsonDocument? Settings { get; set; }

    public BaseService.Domain.Entities.User? User { get; set; }

    public ICollection<FormTemplateField> Fields { get; set; } = new List<FormTemplateField>();
}
