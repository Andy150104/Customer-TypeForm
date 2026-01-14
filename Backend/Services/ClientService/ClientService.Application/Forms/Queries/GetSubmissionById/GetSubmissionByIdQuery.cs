using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetSubmissionById;

public class GetSubmissionByIdQuery : IQuery<GetSubmissionByIdQueryResponse>
{
    public Guid SubmissionId { get; set; }
}
