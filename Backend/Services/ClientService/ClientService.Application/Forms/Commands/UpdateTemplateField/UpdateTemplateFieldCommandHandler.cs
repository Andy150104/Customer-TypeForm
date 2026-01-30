using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.UpdateTemplateField;

/// <summary>
/// Handler for UpdateTemplateFieldCommand
/// </summary>
public class UpdateTemplateFieldCommandHandler(ITemplateService templateService) : ICommandHandler<UpdateTemplateFieldCommand, UpdateTemplateFieldCommandResponse>
{
    /// <summary>
    /// Handle UpdateTemplateFieldCommand
    /// </summary>
    public async Task<UpdateTemplateFieldCommandResponse> Handle(UpdateTemplateFieldCommand request, CancellationToken cancellationToken)
    {
        return await templateService.UpdateTemplateFieldAsync(request, cancellationToken);
    }
}
