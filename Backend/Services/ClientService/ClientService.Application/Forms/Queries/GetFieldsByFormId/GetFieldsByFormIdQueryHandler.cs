using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetFieldsByFormId;

/// <summary>
/// Handler for GetFieldsByFormIdQuery
/// </summary>
/// <param name="fieldService"></param>
public class GetFieldsByFormIdQueryHandler(IFieldService fieldService) : IQueryHandler<GetFieldsByFormIdQuery, GetFieldsByFormIdQueryResponse>
{
    /// <summary>
    /// Handle GetFieldsByFormIdQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetFieldsByFormIdQueryResponse> Handle(GetFieldsByFormIdQuery request, CancellationToken cancellationToken)
    {
        return await fieldService.GetFieldsByFormIdAsync(request, cancellationToken);
    }
}
