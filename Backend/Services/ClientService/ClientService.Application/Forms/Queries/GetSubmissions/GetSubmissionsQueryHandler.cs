using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetDetailSubmissions;

/// <summary>
/// Handler for GetDetailSubmissionsQuery
/// </summary>
/// <param name="formService"></param>
public class GetDetailSubmissionsQueryHandler(IFormService formService) : IQueryHandler<GetDetailSubmissionsQuery, GetDetailSubmissionsQueryResponse>
{
    /// <summary>
    /// Handle GetDetailSubmissionsQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetDetailSubmissionsQueryResponse> Handle(GetDetailSubmissionsQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetDetailSubmissionsAsync(request, cancellationToken);
    }
}
