using ClientService.Application.Forms.Commands.CreateOrUpdateLogic;

namespace ClientService.Application.Interfaces.FormServices;

public interface ILogicService
{
    /// <summary>
    /// Create or update logic rule
    /// If logic with same FieldId, Condition, and Value exists, update it
    /// Otherwise, create new logic
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CreateOrUpdateLogicCommandResponse> CreateOrUpdateLogicAsync(CreateOrUpdateLogicCommand request, CancellationToken cancellationToken);
}
