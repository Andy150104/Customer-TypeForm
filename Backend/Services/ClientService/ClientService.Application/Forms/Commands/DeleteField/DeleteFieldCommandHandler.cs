using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.DeleteField;

/// <summary>
/// Handler for DeleteFieldCommand
/// </summary>
/// <param name="fieldService"></param>
public class DeleteFieldCommandHandler(IFieldService fieldService) : ICommandHandler<DeleteFieldCommand, DeleteFieldCommandResponse>
{
    /// <summary>
    /// Handle DeleteFieldCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeleteFieldCommandResponse> Handle(DeleteFieldCommand request, CancellationToken cancellationToken)
    {
        return await fieldService.DeleteFieldAsync(request, cancellationToken);
    }
}
