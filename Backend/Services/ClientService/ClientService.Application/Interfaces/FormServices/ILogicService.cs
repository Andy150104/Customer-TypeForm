using ClientService.Application.Forms.Commands.CreateOrUpdateLogic;
using ClientService.Application.Forms.Commands.DeleteLogic;
using ClientService.Application.Forms.Commands.UpdateLogic;

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

    /// <summary>
    /// Update logic rule by LogicId and FieldId
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UpdateLogicCommandResponse> UpdateLogicAsync(UpdateLogicCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete logic rule by LogicId and FieldId (soft delete)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DeleteLogicCommandResponse> DeleteLogicAsync(DeleteLogicCommand request, CancellationToken cancellationToken);
}
