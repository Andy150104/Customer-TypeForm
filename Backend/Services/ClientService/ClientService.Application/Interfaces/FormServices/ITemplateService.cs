using ClientService.Application.Forms.Commands.CreateFormFromTemplate;
using ClientService.Application.Forms.Commands.CreateTemplate;
using ClientService.Application.Forms.Commands.DeleteTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplateField;
using ClientService.Application.Forms.Queries.GetTemplateWithFields;
using ClientService.Application.Forms.Queries.GetTemplates;

namespace ClientService.Application.Interfaces.FormServices;

public interface ITemplateService
{
    /// <summary>
    /// Create a new template for forms
    /// </summary>
    Task<CreateTemplateCommandResponse> CreateTemplateAsync(CreateTemplateCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new form and fields from template
    /// </summary>
    Task<CreateFormFromTemplateCommandResponse> CreateFormFromTemplateAsync(CreateFormFromTemplateCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Get all templates for the current user
    /// </summary>
    Task<GetTemplatesQueryResponse> GetTemplatesAsync(GetTemplatesQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Get template with fields
    /// </summary>
    Task<GetTemplateWithFieldsQueryResponse> GetTemplateWithFieldsAsync(GetTemplateWithFieldsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Update template
    /// </summary>
    Task<UpdateTemplateCommandResponse> UpdateTemplateAsync(UpdateTemplateCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete template (soft delete)
    /// </summary>
    Task<DeleteTemplateCommandResponse> DeleteTemplateAsync(DeleteTemplateCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Update template field
    /// </summary>
    Task<UpdateTemplateFieldCommandResponse> UpdateTemplateFieldAsync(UpdateTemplateFieldCommand request, CancellationToken cancellationToken);
}
