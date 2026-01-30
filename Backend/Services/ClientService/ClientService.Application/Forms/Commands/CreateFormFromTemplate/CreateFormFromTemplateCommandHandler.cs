using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateFormFromTemplate;

/// <summary>
/// Handler for CreateFormFromTemplateCommand
/// </summary>
/// <param name="formService"></param>
public class CreateFormFromTemplateCommandHandler(ITemplateService templateService) : ICommandHandler<CreateFormFromTemplateCommand, CreateFormFromTemplateCommandResponse>
{
    /// <summary>
    /// Handle CreateFormFromTemplateCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateFormFromTemplateCommandResponse> Handle(CreateFormFromTemplateCommand request, CancellationToken cancellationToken)
    {
        return await templateService.CreateFormFromTemplateAsync(request, cancellationToken);
    }
}
