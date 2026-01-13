using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateForm;
using ClientService.Application.Forms.Commands.SubmitForm;
using ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetForms;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of IFormService
/// </summary>
public class FormService : IFormService
{
    private readonly ICommandRepository<Form> _formRepository;
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly ICommandRepository<Logic> _logicRepository;
    private readonly ICommandRepository<Submission> _submissionRepository;
    private readonly ICommandRepository<Answer> _answerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormService(
        ICommandRepository<Form> formRepository,
        ICommandRepository<Field> fieldRepository,
        ICommandRepository<Logic> logicRepository,
        ICommandRepository<Submission> submissionRepository,
        ICommandRepository<Answer> answerRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _formRepository = formRepository;
        _fieldRepository = fieldRepository;
        _logicRepository = logicRepository;
        _submissionRepository = submissionRepository;
        _answerRepository = answerRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    /// <summary>
    /// Create a new form (unpublished by default)
    /// </summary>
    public async Task<CreateFormCommandResponse> CreateFormAsync(CreateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateFormCommandResponse();

        try
        {
            // Get current user from identity
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Title is required.");
                return response;
            }

            // Generate slug if not provided
            var slug = request.Slug ?? GenerateSlug(request.Title);

            // Check if slug already exists for this user
            var existingForm = await _formRepository
                .Find(f => f!.UserId == currentUser.UserId && f.Slug == slug && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingForm != null)
            {
                // Append timestamp to make slug unique
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // Create new form with IsPublished = false
            var form = new Form
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.UserId,
                Title = request.Title,
                Slug = slug,
                IsPublished = false, // Always false when creating
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email
            };

            await _formRepository.AddAsync(form);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form created successfully.");
            response.Response = new CreateFormResponseEntity
            {
                Id = form.Id,
                UserId = form.UserId,
                Title = form.Title,
                Slug = form.Slug,
                IsPublished = form.IsPublished,
                CreatedAt = form.CreatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating form: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get all forms for the current user
    /// </summary>
    public async Task<GetFormsQueryResponse> GetFormsAsync(GetFormsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetFormsQueryResponse();

        try
        {
            // Get current user from identity
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            // Get all forms for the current user, ordered by CreatedAt descending (newest first)
            var forms = await _formRepository
                .Find(f => f!.UserId == currentUser.UserId && f.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(f => f!.CreatedAt)
                .ToListAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Forms retrieved successfully.");
            response.Response = forms.Select(f => new FormResponseEntity
            {
                Id = f!.Id,
                UserId = f.UserId,
                Title = f.Title,
                Slug = f.Slug,
                ThemeConfig = f.ThemeConfig,
                Settings = f.Settings,
                IsPublished = f.IsPublished,
                CreatedAt = f.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = f.UpdatedAt
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving forms: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get form with fields and logic rules
    /// </summary>
    public async Task<GetFormWithFieldsAndLogicQueryResponse> GetFormWithFieldsAndLogicAsync(GetFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken)
    {
        var response = new GetFormWithFieldsAndLogicQueryResponse();

        try
        {
            // Get current user from identity
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            // Validate FormId exists and belongs to current user
            var form = await _formRepository
                .Find(f => f!.Id == request.FormId && f.UserId == currentUser.UserId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or you don't have permission to access it.");
                return response;
            }

            // Get all fields for this form, ordered by Order
            var fields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            // Get all logic rules for these fields
            var fieldIds = fields.Select(f => f!.Id).ToList();
            var logicRules = await _logicRepository
                .Find(l => fieldIds.Contains(l!.FieldId) && l.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            // Build response with fields and their logic rules
            var fieldsWithLogic = fields.Select(f =>
            {
                var fieldLogics = logicRules
                    .Where(l => l!.FieldId == f!.Id)
                    .OrderBy(l => l!.Order) // Order by Order for if-else chain
                    .Select(l => new LogicRuleResponseEntity
                    {
                        Id = l!.Id,
                        FieldId = l.FieldId,
                        Condition = l.Condition.ToString(),
                        Value = l.Value,
                        DestinationFieldId = l.DestinationFieldId,
                        Order = l.Order,
                        LogicGroupId = l.LogicGroupId,
                        CreatedAt = l.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = l.UpdatedAt
                    })
                    .ToList();

                // Calculate default next field (field with Order = current Order + 1)
                var nextField = fields.FirstOrDefault(f2 => f2!.Order == f!.Order + 1);

                return new FieldWithLogicResponseEntity
                {
                    Id = f!.Id,
                    FormId = f.FormId,
                    Title = f.Title,
                    Description = f.Description,
                    Type = f.Type.ToString(),
                    Properties = f.Properties,
                    IsRequired = f.IsRequired,
                    Order = f.Order,
                    CreatedAt = f.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = f.UpdatedAt,
                    LogicRules = fieldLogics,
                    DefaultNextFieldId = nextField?.Id
                };
            }).ToList();

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form with fields and logic retrieved successfully.");
            response.Response = new FormWithFieldsAndLogicResponseEntity
            {
                Id = form.Id,
                UserId = form.UserId,
                Title = form.Title,
                Slug = form.Slug,
                ThemeConfig = form.ThemeConfig,
                Settings = form.Settings,
                IsPublished = form.IsPublished,
                CreatedAt = form.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = form.UpdatedAt,
                Fields = fieldsWithLogic
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving form with fields and logic: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Update form published status
    /// If IsPublished = true, validates that form has at least one field
    /// </summary>
    public async Task<UpdateFormPublishedStatusCommandResponse> UpdateFormPublishedStatusAsync(UpdateFormPublishedStatusCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateFormPublishedStatusCommandResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var form = await _formRepository
                .Find(f => f!.Id == request.FormId && f.UserId == currentUser.UserId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or you don't have permission to access it.");
                return response;
            }

            // If trying to publish, validate form has required information
            if (request.IsPublished)
            {
                // Check form has title
                if (string.IsNullOrWhiteSpace(form.Title))
                {
                    response.Success = false;
                    response.SetMessage(MessageId.E10000, "Form must have a title before publishing.");
                    return response;
                }

                // Check form has at least one field
                var fieldCount = await _fieldRepository
                    .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                    .CountAsync(cancellationToken);

                if (fieldCount == 0)
                {
                    response.Success = false;
                    response.SetMessage(MessageId.E10000, "Form must have at least one field before publishing.");
                    return response;
                }
            }

            // Update IsPublished status
            form.IsPublished = request.IsPublished;
            form.UpdatedAt = DateTime.UtcNow;
            form.UpdatedBy = currentUser.Email;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, request.IsPublished ? "Form published successfully." : "Form unpublished successfully.");
            response.Response = new UpdateFormPublishedStatusResponseEntity
            {
                Id = form.Id,
                IsPublished = form.IsPublished,
                UpdatedAt = form.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while updating form published status: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Submit a form with answers
    /// </summary>
    public async Task<SubmitFormCommandResponse> SubmitFormAsync(SubmitFormCommand request, CancellationToken cancellationToken)
    {
        var response = new SubmitFormCommandResponse();

        try
        {
            // Validate form exists and is published
            var form = await _formRepository
                .Find(f => f!.Id == request.FormId && f.IsPublished && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or is not published.");
                return response;
            }

            // Get all active fields for this form
            var fields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var fieldIds = fields.Select(f => f!.Id).ToHashSet();

            // Validate all answers belong to this form's fields
            var invalidFieldIds = request.Answers
                .Where(a => !fieldIds.Contains(a.FieldId))
                .Select(a => a.FieldId)
                .ToList();

            if (invalidFieldIds.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, $"Invalid field IDs: {string.Join(", ", invalidFieldIds)}");
                return response;
            }

            // Validate required fields are answered
            var requiredFieldIds = fields
                .Where(f => f!.IsRequired)
                .Select(f => f!.Id)
                .ToHashSet();

            var answeredFieldIds = request.Answers
                .Select(a => a.FieldId)
                .ToHashSet();

            var missingRequiredFields = requiredFieldIds
                .Where(fid => !answeredFieldIds.Contains(fid))
                .ToList();

            if (missingRequiredFields.Any())
            {
                var missingFieldTitles = fields
                    .Where(f => missingRequiredFields.Contains(f!.Id))
                    .Select(f => f!.Title)
                    .ToList();

                response.Success = false;
                response.SetMessage(MessageId.E10000, $"Required fields are missing: {string.Join(", ", missingFieldTitles)}");
                return response;
            }

            // Create submission
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                FormId = request.FormId,
                MetaData = request.MetaData,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "Anonymous", // Public form submission
                UpdatedBy = "Anonymous"
            };

            await _submissionRepository.AddAsync(submission);

            // Create answers
            foreach (var answerDto in request.Answers)
            {
                var answer = new Answer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    FieldId = answerDto.FieldId,
                    Value = answerDto.Value,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "Anonymous",
                    UpdatedBy = "Anonymous"
                };

                await _answerRepository.AddAsync(answer);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form submitted successfully.");
            response.Response = new SubmitFormResponseEntity
            {
                SubmissionId = submission.Id,
                FormId = submission.FormId,
                CreatedAt = submission.CreatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while submitting form: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Generate a URL-friendly slug from title
    /// </summary>
    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
        
        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple consecutive hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        return slug;
    }
}
