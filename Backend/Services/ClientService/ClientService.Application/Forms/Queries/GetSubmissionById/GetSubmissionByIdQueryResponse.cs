using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Queries.GetSubmissionById;

public record GetSubmissionByIdQueryResponse : AbstractApiResponse<SubmissionDetailResponseEntity>
{
    public override SubmissionDetailResponseEntity Response { get; set; } = null!;
}

public class SubmissionDetailResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public string FormTitle { get; set; } = null!;
    public JsonDocument? MetaData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AnswerDetailResponseEntity> Answers { get; set; } = new();
}

public class AnswerDetailResponseEntity
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string FieldTitle { get; set; } = null!;
    public string FieldDescription { get; set; } = null!;
    public string FieldType { get; set; } = null!;
    public JsonDocument Value { get; set; } = null!;
    public Guid? FieldOptionId { get; set; }
    public string? OptionLabel { get; set; }
    public string? OptionValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
