using BaseService.Common.ApiEntities;
using System.Text.Json;

namespace ClientService.Application.Forms.Queries.GetSubmissions;

public record GetSubmissionsQueryResponse : AbstractApiResponse<List<SubmissionResponseEntity>>
{
    public override List<SubmissionResponseEntity> Response { get; set; } = new();
}

public class SubmissionResponseEntity
{
    public Guid Id { get; set; }
    public Guid FormId { get; set; }
    public JsonDocument? MetaData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AnswerResponseEntity> Answers { get; set; } = new();
}

public class AnswerResponseEntity
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string FieldTitle { get; set; } = null!;
    public string FieldType { get; set; } = null!;
    public JsonDocument Value { get; set; } = null!;
    public Guid? FieldOptionId { get; set; }
    public string? OptionLabel { get; set; }
    public string? OptionValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
