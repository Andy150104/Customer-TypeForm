using BuildingBlocks.CQRS;
using System.Text.Json;

namespace ClientService.Application.Forms.Queries.GetNextQuestion;

public class GetNextQuestionQuery : IQuery<GetNextQuestionQueryResponse>
{
    public Guid FormId { get; set; }
    public Guid CurrentFieldId { get; set; }
    /// <summary>
    /// Current field's answer value (used to evaluate logic rules)
    /// </summary>
    public JsonDocument? CurrentValue { get; set; }
}
