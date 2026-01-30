using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Queries.GetTemplateWithFields;

public class GetTemplateWithFieldsQuery : IQuery<GetTemplateWithFieldsQueryResponse>
{
    public Guid TemplateId { get; set; }
}
