using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateForm;

/// <summary>
/// Handler for CreateFormCommand
/// </summary>
/// <param name="formService"></param>
public class CreateFormCommandHandler(IFormService formService) : ICommandHandler<CreateFormCommand, CreateFormCommandResponse>
{
    /// <summary>
    /// Handle CreateFormCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateFormCommandResponse> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        return await formService.CreateFormAsync(request, cancellationToken);
    }
}
