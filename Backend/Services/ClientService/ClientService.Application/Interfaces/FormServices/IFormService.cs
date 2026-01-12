using ClientService.Application.Forms.Commands.CreateForm;

namespace ClientService.Application.Interfaces.FormServices;

public interface IFormService
{
    /// <summary>
    /// Create a new form (unpublished by default)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CreateFormCommandResponse> CreateFormAsync(CreateFormCommand request, CancellationToken cancellationToken);
}
