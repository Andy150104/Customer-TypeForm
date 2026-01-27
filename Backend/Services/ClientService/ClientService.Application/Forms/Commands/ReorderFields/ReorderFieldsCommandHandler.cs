using BuildingBlocks.CQRS;
using ClientService.Application.Interfaces.FormServices;

namespace ClientService.Application.Forms.Commands.ReorderFields;

/// <summary>
/// Handler for ReorderFieldsCommand
/// </summary>
/// <param name="fieldService"></param>
public class ReorderFieldsCommandHandler(IFieldService fieldService) : ICommandHandler<ReorderFieldsCommand, ReorderFieldsCommandResponse>
{
    /// <summary>
    /// Handle ReorderFieldsCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ReorderFieldsCommandResponse> Handle(ReorderFieldsCommand request, CancellationToken cancellationToken)
    {
        return await fieldService.ReorderFieldsAsync(request, cancellationToken);
    }
}
