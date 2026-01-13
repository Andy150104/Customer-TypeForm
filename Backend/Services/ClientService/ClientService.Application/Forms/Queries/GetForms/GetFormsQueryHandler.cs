using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetForms;

/// <summary>
/// Handler for GetFormsQuery
/// </summary>
/// <param name="formService"></param>
public class GetFormsQueryHandler(IFormService formService) : IQueryHandler<GetFormsQuery, GetFormsQueryResponse>
{
    /// <summary>
    /// Handle GetFormsQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetFormsQueryResponse> Handle(GetFormsQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetFormsAsync(request, cancellationToken);
    }
}
