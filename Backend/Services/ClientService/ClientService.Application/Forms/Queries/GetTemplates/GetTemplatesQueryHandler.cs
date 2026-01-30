using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetTemplates;

/// <summary>
/// Handler for GetTemplatesQuery
/// </summary>
/// <param name="formService"></param>
public class GetTemplatesQueryHandler(ITemplateService templateService) : IQueryHandler<GetTemplatesQuery, GetTemplatesQueryResponse>
{
    /// <summary>
    /// Handle GetTemplatesQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetTemplatesQueryResponse> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await templateService.GetTemplatesAsync(request, cancellationToken);
    }
}
