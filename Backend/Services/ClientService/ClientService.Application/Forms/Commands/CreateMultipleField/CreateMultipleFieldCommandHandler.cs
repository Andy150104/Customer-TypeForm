using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.CreateMultipleField;

/// <summary>
/// Handler for CreateMultipleFieldCommand
/// </summary>
/// <param name="fieldService"></param>
public class CreateMultipleFieldCommandHandler(IFieldService fieldService) : ICommandHandler<CreateMultipleFieldCommand, CreateMultipleFieldCommandResponse>
{
    /// <summary>
    /// Handle CreateMultipleFieldCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CreateMultipleFieldCommandResponse> Handle(CreateMultipleFieldCommand request, CancellationToken cancellationToken)
    {
        return await fieldService.CreateMultipleFieldsAsync(request, cancellationToken);
    }
}
