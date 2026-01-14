using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetSubmissionById;

/// <summary>
/// Handler for GetSubmissionByIdQuery
/// </summary>
/// <param name="formService"></param>
public class GetSubmissionByIdQueryHandler(IFormService formService) : IQueryHandler<GetSubmissionByIdQuery, GetSubmissionByIdQueryResponse>
{
    /// <summary>
    /// Handle GetSubmissionByIdQuery
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GetSubmissionByIdQueryResponse> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetSubmissionByIdAsync(request, cancellationToken);
    }
}
