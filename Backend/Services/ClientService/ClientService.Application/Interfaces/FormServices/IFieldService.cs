using ClientService.Application.Forms.Commands.CreateField;
using ClientService.Application.Forms.Commands.CreateMultipleField;
using ClientService.Application.Forms.Commands.DeleteField;
using ClientService.Application.Forms.Commands.ReorderFields;
using ClientService.Application.Forms.Commands.UpdateField;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;

namespace ClientService.Application.Interfaces.FormServices;

public interface IFieldService
{
    /// <summary>
    /// Create a new field for a form
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CreateFieldCommandResponse> CreateFieldAsync(CreateFieldCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Create multiple fields for a form at once
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CreateMultipleFieldCommandResponse> CreateMultipleFieldsAsync(CreateMultipleFieldCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Get all fields by form ID, ordered by Order field
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetFieldsByFormIdQueryResponse> GetFieldsByFormIdAsync(GetFieldsByFormIdQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Update field
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UpdateFieldCommandResponse> UpdateFieldAsync(UpdateFieldCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete field (soft delete - set IsActive = false)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DeleteFieldCommandResponse> DeleteFieldAsync(DeleteFieldCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Reorder fields in a form
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReorderFieldsCommandResponse> ReorderFieldsAsync(ReorderFieldsCommand request, CancellationToken cancellationToken);
}
