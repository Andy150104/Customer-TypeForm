using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;

public class GetPublishedFormWithFieldsAndLogicQuery : IQuery<GetPublishedFormWithFieldsAndLogicQueryResponse>
{
    public Guid FormId { get; set; }
}
