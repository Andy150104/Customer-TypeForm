using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateField;

/// <summary>
/// Handler for CreateFieldCommand
/// </summary>
/// <param name="fieldService"></param>
public class CreateFieldCommandHandler(IFieldService fieldService) : ICommandHandler<CreateFieldCommand, CreateFieldCommandResponse>
{
    /// <summary>
    /// Handle CreateFieldCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateFieldCommandResponse> Handle(CreateFieldCommand request, CancellationToken cancellationToken)
    {
        return await fieldService.CreateFieldAsync(request, cancellationToken);
    }
}
