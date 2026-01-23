using BaseService.Common.ApiEntities;
using System.Text.Json;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;

namespace ClientService.Application.Forms.Queries.GetNextQuestion;

public record GetNextQuestionQueryResponse : AbstractApiResponse<NextQuestionResponseEntity>
{
    public override NextQuestionResponseEntity Response { get; set; } = null!;
}

public class NextQuestionResponseEntity
{
    public Guid? NextFieldId { get; set; }
    public FieldResponseEntity? NextField { get; set; }
    public bool IsEndOfForm { get; set; }
    /// <summary>
    /// Which logic rule was applied (null if default order was used)
    /// </summary>
    public Guid? AppliedLogicId { get; set; }
}
