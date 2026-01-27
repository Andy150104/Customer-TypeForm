using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.UpdateField;

/// <summary>
/// Handler for UpdateFieldCommand
/// </summary>
/// <param name="fieldService"></param>
public class UpdateFieldCommandHandler(IFieldService fieldService) : ICommandHandler<UpdateFieldCommand, UpdateFieldCommandResponse>
{
    /// <summary>
    /// Handle UpdateFieldCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<UpdateFieldCommandResponse> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
    {
        return await fieldService.UpdateFieldAsync(request, cancellationToken);
    }
}
