using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Queries.GetSubmissionsOverview;

public record GetSubmissionsOverviewQueryResponse : AbstractApiResponse<FormSubmissionsOverviewResponseEntity>
{
    public override FormSubmissionsOverviewResponseEntity Response { get; set; } = null!;
}

public class FormSubmissionsOverviewResponseEntity
{
    public Guid FormId { get; set; }
    public string FormTitle { get; set; } = null!;
    public int TotalSubmissions { get; set; }
    public int TotalFields { get; set; }
    public int AnsweredCount { get; set; }
    public double CompletionRate { get; set; }
    public FieldQualityResponseEntity? MostEmptyField { get; set; }
    public List<FieldOverviewResponseEntity> Fields { get; set; } = new();
}

public class FieldOverviewResponseEntity
{
    public Guid FieldId { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsRequired { get; set; }
    public int AnsweredCount { get; set; }
    public int EmptyCount { get; set; }
    public double EmptyRate { get; set; }
    public int AnswerCount { get; set; }
    public List<OptionTrendResponseEntity>? OptionTrends { get; set; }
    public List<ValueTrendResponseEntity>? TopValues { get; set; }
}

public class OptionTrendResponseEntity
{
    public Guid? FieldOptionId { get; set; }
    public string? Label { get; set; }
    public string? Value { get; set; }
    public int Count { get; set; }
    public double Rate { get; set; }
}

public class ValueTrendResponseEntity
{
    public string Value { get; set; } = null!;
    public int Count { get; set; }
    public double Rate { get; set; }
}

public class FieldQualityResponseEntity
{
    public Guid FieldId { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int EmptyCount { get; set; }
    public double EmptyRate { get; set; }
}
