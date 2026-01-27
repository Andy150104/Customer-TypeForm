using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.DeleteField;

public record DeleteFieldCommandResponse : AbstractApiResponse<DeleteFieldResponseEntity>
{
    public override DeleteFieldResponseEntity Response { get; set; } = null!;
}

public class DeleteFieldResponseEntity
{
    public Guid Id { get; set; }
    public DateTime UpdatedAt { get; set; }
}
