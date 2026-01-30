using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.DeleteTemplate;

/// <summary>
/// Handler for DeleteTemplateCommand
/// </summary>
public class DeleteTemplateCommandHandler(ITemplateService templateService) : ICommandHandler<DeleteTemplateCommand, DeleteTemplateCommandResponse>
{
    /// <summary>
    /// Handle DeleteTemplateCommand
    /// </summary>
    public async Task<DeleteTemplateCommandResponse> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        return await templateService.DeleteTemplateAsync(request, cancellationToken);
    }
}
