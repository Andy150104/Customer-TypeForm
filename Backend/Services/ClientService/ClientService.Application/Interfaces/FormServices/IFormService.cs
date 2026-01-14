using ClientService.Application.Forms.Commands.CreateForm;
using ClientService.Application.Forms.Commands.SubmitForm;
using ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetForms;
using ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetSubmissions;
using ClientService.Application.Forms.Queries.GetSubmissionById;

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

    /// <summary>
    /// Get all forms for the current user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetFormsQueryResponse> GetFormsAsync(GetFormsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Get form with fields and logic rules
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetFormWithFieldsAndLogicQueryResponse> GetFormWithFieldsAndLogicAsync(GetFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Get published form with fields and logic rules (public endpoint)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetPublishedFormWithFieldsAndLogicQueryResponse> GetPublishedFormWithFieldsAndLogicAsync(GetPublishedFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Update form published status
    /// If IsPublished = true, validates that form has at least one field
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<UpdateFormPublishedStatusCommandResponse> UpdateFormPublishedStatusAsync(UpdateFormPublishedStatusCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Submit a form with answers
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SubmitFormCommandResponse> SubmitFormAsync(SubmitFormCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Get all submissions for a form
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetSubmissionsQueryResponse> GetSubmissionsAsync(GetSubmissionsQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Get submission by ID (public endpoint for submitter to view their submission)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetSubmissionByIdQueryResponse> GetSubmissionByIdAsync(GetSubmissionByIdQuery request, CancellationToken cancellationToken);
}
