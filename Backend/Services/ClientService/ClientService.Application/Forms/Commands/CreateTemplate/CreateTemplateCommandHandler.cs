using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateTemplate;

/// <summary>
/// Handler for CreateTemplateCommand
/// </summary>
/// <param name="formService"></param>
public class CreateTemplateCommandHandler(ITemplateService templateService) : ICommandHandler<CreateTemplateCommand, CreateTemplateCommandResponse>
{
    /// <summary>
    /// Handle CreateTemplateCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateTemplateCommandResponse> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        return await templateService.CreateTemplateAsync(request, cancellationToken);
    }
}
