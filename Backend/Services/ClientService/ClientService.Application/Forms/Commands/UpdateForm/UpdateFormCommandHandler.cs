using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.UpdateForm;

/// <summary>
/// Handler for UpdateFormCommand
/// </summary>
/// <param name="formService"></param>
public class UpdateFormCommandHandler(IFormService formService) : ICommandHandler<UpdateFormCommand, UpdateFormCommandResponse>
{
    /// <summary>
    /// Handle UpdateFormCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<UpdateFormCommandResponse> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        return await formService.UpdateFormAsync(request, cancellationToken);
    }
}
