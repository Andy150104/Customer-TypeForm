using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.DeleteLogic;

/// <summary>
/// Handler for DeleteLogicCommand
/// </summary>
/// <param name="logicService"></param>
public class DeleteLogicCommandHandler(ILogicService logicService) : ICommandHandler<DeleteLogicCommand, DeleteLogicCommandResponse>
{
    /// <summary>
    /// Handle DeleteLogicCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeleteLogicCommandResponse> Handle(DeleteLogicCommand request, CancellationToken cancellationToken)
    {
        return await logicService.DeleteLogicAsync(request, cancellationToken);
    }
}
