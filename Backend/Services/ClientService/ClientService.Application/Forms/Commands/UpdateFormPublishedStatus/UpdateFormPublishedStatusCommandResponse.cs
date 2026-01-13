using BaseService.Common.ApiEntities;

namespace ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;

public record UpdateFormPublishedStatusCommandResponse : AbstractApiResponse<UpdateFormPublishedStatusResponseEntity>
{
    public override UpdateFormPublishedStatusResponseEntity Response { get; set; } = null!;
}

public class UpdateFormPublishedStatusResponseEntity
{
    public Guid Id { get; set; }
    public bool IsPublished { get; set; }
    public DateTime UpdatedAt { get; set; }
}
