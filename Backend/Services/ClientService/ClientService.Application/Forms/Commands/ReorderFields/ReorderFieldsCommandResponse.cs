using BaseService.Common.ApiEntities;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;

namespace ClientService.Application.Forms.Commands.ReorderFields;

public record ReorderFieldsCommandResponse : AbstractApiResponse<List<FieldResponseEntity>>
{
    public override List<FieldResponseEntity> Response { get; set; } = new();
}
