using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Queries.GetNextQuestion;

public class GetNextQuestionQueryHandler(IFormService formService) : IQueryHandler<GetNextQuestionQuery, GetNextQuestionQueryResponse>
{
    public async Task<GetNextQuestionQueryResponse> Handle(GetNextQuestionQuery request, CancellationToken cancellationToken)
    {
        return await formService.GetNextQuestionAsync(request, cancellationToken);
    }
}
