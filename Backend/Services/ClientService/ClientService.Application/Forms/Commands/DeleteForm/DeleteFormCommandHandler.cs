using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.DeleteForm;

/// <summary>
/// Handler for DeleteFormCommand
/// </summary>
/// <param name="formService"></param>
public class DeleteFormCommandHandler(IFormService formService) : ICommandHandler<DeleteFormCommand, DeleteFormCommandResponse>
{
    /// <summary>
    /// Handle DeleteFormCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeleteFormCommandResponse> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        return await formService.DeleteFormAsync(request, cancellationToken);
    }
}
