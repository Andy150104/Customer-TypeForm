using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;

/// <summary>
/// Handler for GetFormWithFieldsAndLogicQuery
/// </summary>
/// <param name="formService"></param>
public class GetFormWithFieldsAndLogicQueryHandler(IFormService formService) : IQueryHandler<GetFormWithFieldsAndLogicQuery, GetFormWithFieldsAndLogicQueryResponse>
{
    /// <summary>
    /// Handle GetFormWithFieldsAndLogicQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetFormWithFieldsAndLogicQueryResponse> Handle(GetFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetFormWithFieldsAndLogicAsync(request, cancellationToken);
    }
}
