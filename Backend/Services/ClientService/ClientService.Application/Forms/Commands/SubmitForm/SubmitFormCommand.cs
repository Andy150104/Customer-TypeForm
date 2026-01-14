using BuildingBlocks.CQRS;
using System.Text.Json;

namespace ClientService.Application.Forms.Commands.SubmitForm;

public class SubmitFormCommand : ICommand<SubmitFormCommandResponse>
{
    public Guid FormId { get; set; }
    public JsonDocument? MetaData { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public class AnswerDto
{
    public Guid FieldId { get; set; }
    public JsonDocument Value { get; set; } = null!;
    /// <summary>
    /// FieldOptionId for Select/MultiSelect/Radio fields (optional but recommended)
    /// </summary>
    public Guid? FieldOptionId { get; set; }
}
