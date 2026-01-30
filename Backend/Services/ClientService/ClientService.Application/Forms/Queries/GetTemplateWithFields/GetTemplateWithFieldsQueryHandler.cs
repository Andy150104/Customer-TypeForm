using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetTemplateWithFields;

/// <summary>
/// Handler for GetTemplateWithFieldsQuery
/// </summary>
/// <param name="formService"></param>
public class GetTemplateWithFieldsQueryHandler(ITemplateService templateService) : IQueryHandler<GetTemplateWithFieldsQuery, GetTemplateWithFieldsQueryResponse>
{
    /// <summary>
    /// Handle GetTemplateWithFieldsQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetTemplateWithFieldsQueryResponse> Handle(GetTemplateWithFieldsQuery request, CancellationToken cancellationToken)
    {
        return await templateService.GetTemplateWithFieldsAsync(request, cancellationToken);
    }
}
