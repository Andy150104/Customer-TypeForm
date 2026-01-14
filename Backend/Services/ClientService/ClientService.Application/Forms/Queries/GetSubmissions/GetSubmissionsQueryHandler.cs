using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetSubmissions;

/// <summary>
/// Handler for GetSubmissionsQuery
/// </summary>
/// <param name="formService"></param>
public class GetSubmissionsQueryHandler(IFormService formService) : IQueryHandler<GetSubmissionsQuery, GetSubmissionsQueryResponse>
{
    /// <summary>
    /// Handle GetSubmissionsQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetSubmissionsQueryResponse> Handle(GetSubmissionsQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetSubmissionsAsync(request, cancellationToken);
    }
}
