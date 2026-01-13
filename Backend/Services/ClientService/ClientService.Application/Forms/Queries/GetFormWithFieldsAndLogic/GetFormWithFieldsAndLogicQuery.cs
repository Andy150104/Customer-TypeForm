using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;

public class GetFormWithFieldsAndLogicQuery : IQuery<GetFormWithFieldsAndLogicQueryResponse>
{
    public Guid FormId { get; set; }
}
