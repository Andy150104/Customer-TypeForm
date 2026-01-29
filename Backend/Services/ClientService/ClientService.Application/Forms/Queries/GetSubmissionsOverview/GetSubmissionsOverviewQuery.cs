using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetSubmissionsOverview;

public class GetSubmissionsOverviewQuery : IQuery<GetSubmissionsOverviewQueryResponse>
{
    public Guid FormId { get; set; }
}
