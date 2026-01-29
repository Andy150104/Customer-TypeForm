using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetSubmissionsOverview;

/// <summary>
/// Handler for GetSubmissionsOverviewQuery
/// </summary>
/// <param name="formService"></param>
public class GetSubmissionsOverviewQueryHandler(IFormService formService) : IQueryHandler<GetSubmissionsOverviewQuery, GetSubmissionsOverviewQueryResponse>
{
    /// <summary>
    /// Handle GetSubmissionsOverviewQuery
    /// </summary>
    public async Task<GetSubmissionsOverviewQueryResponse> Handle(GetSubmissionsOverviewQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetSubmissionsOverviewAsync(request, cancellationToken);
    }
}
