using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;

/// <summary>
/// Handler for GetPublishedFormWithFieldsAndLogicQuery
/// </summary>
/// <param name="formService"></param>
public class GetPublishedFormWithFieldsAndLogicQueryHandler(IFormService formService) : IQueryHandler<GetPublishedFormWithFieldsAndLogicQuery, GetPublishedFormWithFieldsAndLogicQueryResponse>
{
    /// <summary>
    /// Handle GetPublishedFormWithFieldsAndLogicQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetPublishedFormWithFieldsAndLogicQueryResponse> Handle(GetPublishedFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetPublishedFormWithFieldsAndLogicAsync(request, cancellationToken);
    }
}
