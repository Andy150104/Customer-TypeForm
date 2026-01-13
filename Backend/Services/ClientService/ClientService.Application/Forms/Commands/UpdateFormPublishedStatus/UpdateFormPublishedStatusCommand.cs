using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;

public class UpdateFormPublishedStatusCommand : ICommand<UpdateFormPublishedStatusCommandResponse>
{
    public Guid FormId { get; set; }
    public bool IsPublished { get; set; }
}
