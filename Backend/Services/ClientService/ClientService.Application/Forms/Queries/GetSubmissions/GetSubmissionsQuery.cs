using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetSubmissions;

public class GetSubmissionsQuery : IQuery<GetSubmissionsQueryResponse>
{
    public Guid FormId { get; set; }
}
