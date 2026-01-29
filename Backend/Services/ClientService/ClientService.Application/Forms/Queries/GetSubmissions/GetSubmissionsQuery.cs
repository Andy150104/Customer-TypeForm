using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetDetailSubmissions;

public class GetDetailSubmissionsQuery : IQuery<GetDetailSubmissionsQueryResponse>
{
    public Guid FormId { get; set; }
}
