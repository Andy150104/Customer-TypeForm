using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.SubmitForm;

public record SubmitFormCommandResponse : AbstractApiResponse<SubmitFormResponseEntity>
{
    public override SubmitFormResponseEntity Response { get; set; } = null!;
}

public class SubmitFormResponseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid FormId { get; set; }
    public DateTime CreatedAt { get; set; }
}
