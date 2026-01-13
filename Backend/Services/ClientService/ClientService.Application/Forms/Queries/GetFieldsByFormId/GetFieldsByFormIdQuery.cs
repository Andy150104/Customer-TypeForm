using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetFieldsByFormId;

public class GetFieldsByFormIdQuery : IQuery<GetFieldsByFormIdQueryResponse>
{
    public Guid FormId { get; set; }
}
