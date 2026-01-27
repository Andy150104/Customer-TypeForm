using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.UpdateLogic;

/// <summary>
/// Handler for UpdateLogicCommand
/// </summary>
/// <param name="logicService"></param>
public class UpdateLogicCommandHandler(ILogicService logicService) : ICommandHandler<UpdateLogicCommand, UpdateLogicCommandResponse>
{
    /// <summary>
    /// Handle UpdateLogicCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<UpdateLogicCommandResponse> Handle(UpdateLogicCommand request, CancellationToken cancellationToken)
    {
        return await logicService.UpdateLogicAsync(request, cancellationToken);
    }
}
