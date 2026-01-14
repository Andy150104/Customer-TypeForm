using BaseService.Common.ApiEntities;
using System.Text.Json;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;

namespace ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;

public record GetPublishedFormWithFieldsAndLogicQueryResponse : AbstractApiResponse<FormWithFieldsAndLogicResponseEntity>
{
    public override FormWithFieldsAndLogicResponseEntity Response { get; set; } = null!;
}
