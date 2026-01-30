using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.UpdateTemplate;

/// <summary>
/// Handler for UpdateTemplateCommand
/// </summary>
public class UpdateTemplateCommandHandler(ITemplateService templateService) : ICommandHandler<UpdateTemplateCommand, UpdateTemplateCommandResponse>
{
    /// <summary>
    /// Handle UpdateTemplateCommand
    /// </summary>
    public async Task<UpdateTemplateCommandResponse> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        return await templateService.UpdateTemplateAsync(request, cancellationToken);
    }
}
