using BaseService.Common.ApiEntities;
using ClientService.Application.Forms.Commands.CreateField;

namespace ClientService.Application.Forms.Commands.CreateMultipleField;

public record CreateMultipleFieldCommandResponse : AbstractApiResponse<List<CreateFieldResponseEntity>>
{
    public override List<CreateFieldResponseEntity> Response { get; set; } = new();
}
