using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateOrUpdateLogic;

/// <summary>
/// Handler for CreateOrUpdateLogicCommand
/// </summary>
/// <param name="logicService"></param>
public class CreateOrUpdateLogicCommandHandler(ILogicService logicService) : ICommandHandler<CreateOrUpdateLogicCommand, CreateOrUpdateLogicCommandResponse>
{
    /// <summary>
    /// Handle CreateOrUpdateLogicCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateOrUpdateLogicCommandResponse> Handle(CreateOrUpdateLogicCommand request, CancellationToken cancellationToken)
    {
        return await logicService.CreateOrUpdateLogicAsync(request, cancellationToken);
    }
}
