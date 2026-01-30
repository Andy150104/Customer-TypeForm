using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateForm;
using ClientService.Application.Forms.Commands.DeleteForm;
using ClientService.Application.Forms.Commands.SubmitForm;
using ClientService.Application.Forms.Commands.UpdateForm;
using ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetForms;
using ClientService.Application.Forms.Queries.GetNextQuestion;
using ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetDetailSubmissions;
using ClientService.Application.Forms.Queries.GetSubmissionsOverview;
using ClientService.Application.Forms.Queries.GetSubmissionById;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Application.Interfaces.NotificationServices;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of IFormService
/// </summary>
public class FormService : IFormService
{
    private readonly ICommandRepository<Form> _formRepository;
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly ICommandRepository<Logic> _logicRepository;
    private readonly ICommandRepository<FieldOption> _fieldOptionRepository;
    private readonly ICommandRepository<Submission> _submissionRepository;
    private readonly ICommandRepository<Answer> _answerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormService(
        ICommandRepository<Form> formRepository,
        ICommandRepository<Field> fieldRepository,
        ICommandRepository<Logic> logicRepository,
        ICommandRepository<FieldOption> fieldOptionRepository,
        ICommandRepository<Submission> submissionRepository,
        ICommandRepository<Answer> answerRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        INotificationService notificationService)
    {
        _formRepository = formRepository;
        _fieldRepository = fieldRepository;
        _logicRepository = logicRepository;
        _fieldOptionRepository = fieldOptionRepository;
        _submissionRepository = submissionRepository;
        _answerRepository = answerRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _notificationService = notificationService;
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

            // Generate slug automatically from title
            var slug = GenerateSlug(request.Title);

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

            // Get all options for these fields
            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.FieldId)
                .ThenBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            // Group options by FieldId
            var optionsByFieldId = allOptions
                .GroupBy(o => o!.FieldId)
                .ToDictionary(g => g.Key, g => g.Select(o => new FieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList());

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
                    ImageUrl = f.ImageUrl,
                    Type = f.Type.ToString(),
                    Properties = f.Properties,
                    IsRequired = f.IsRequired,
                    Order = f.Order,
                    CreatedAt = f.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = f.UpdatedAt,
                    LogicRules = fieldLogics,
                    DefaultNextFieldId = nextField?.Id,
                    Options = optionsByFieldId.ContainsKey(f.Id) ? optionsByFieldId[f.Id] : null
                };
            }).ToList();

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form with fields and logic retrieved successfully.");
            response.Response = new FormWithFieldsAndLogicResponseEntity
            {
                Id = form.Id,
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
    /// Get published form with fields and logic rules (public endpoint)
    /// </summary>
    public async Task<GetPublishedFormWithFieldsAndLogicQueryResponse> GetPublishedFormWithFieldsAndLogicAsync(GetPublishedFormWithFieldsAndLogicQuery request, CancellationToken cancellationToken)
    {
        var response = new GetPublishedFormWithFieldsAndLogicQueryResponse();

        try
        {
            // Get published form (no authentication required, public endpoint)
            var form = await _formRepository
                .Find(f => f!.Id == request.FormId && f.IsPublished && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or is not published.");
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

            // Get all options for these fields
            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.FieldId)
                .ThenBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            // Group options by FieldId
            var optionsByFieldId = allOptions
                .Where(o => o != null)
                .GroupBy(o => o!.FieldId)
                .ToDictionary(g => g.Key, g => g.Select(o => new FieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList());

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
                    ImageUrl = f.ImageUrl,
                    Type = f.Type.ToString(),
                    Properties = f.Properties,
                    IsRequired = f.IsRequired,
                    Order = f.Order,
                    CreatedAt = f.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = f.UpdatedAt,
                    LogicRules = fieldLogics,
                    DefaultNextFieldId = nextField?.Id,
                    Options = optionsByFieldId.ContainsKey(f.Id) ? optionsByFieldId[f.Id] : null
                };
            }).ToList();

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Published form with fields and logic retrieved successfully.");
            response.Response = new FormWithFieldsAndLogicResponseEntity
            {
                Id = form.Id,
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
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving published form: {ex.Message}");
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

            _formRepository.Update(form, currentUser.Email);
            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

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

            var orderedFields = fields
                .Where(f => f != null)
                .OrderBy(f => f!.Order)
                .Select(f => f!)
                .ToList();

            var fieldsById = orderedFields.ToDictionary(f => f.Id, f => f);
            var fieldIds = fieldsById.Keys.ToHashSet();

            var answersByFieldId = request.Answers
                .GroupBy(a => a.FieldId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Get all field options for fields that have options (Select, MultiSelect, Radio)
            var optionFieldIds = orderedFields
                .Where(f => f!.Type == ConstantEnum.FieldType.Select || f.Type == ConstantEnum.FieldType.MultiSelect || f.Type == ConstantEnum.FieldType.Radio)
                .Select(f => f!.Id)
                .ToList();
            
            var allOptions = new List<FieldOption>();
            if (optionFieldIds.Any())
            {
                var options = await _fieldOptionRepository
                    .Find(o => optionFieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken);
                
                allOptions = options.Where(o => o != null).Select(o => o!).ToList();
            }
            
            var optionsByFieldId = allOptions
                .GroupBy(o => o.FieldId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get all logic rules for fields in this form
            var logicRules = await _logicRepository
                .Find(l => fieldIds.Contains(l!.FieldId) && l.IsActive, cancellationToken: cancellationToken)
                .OrderBy(l => l!.Order)
                .ToListAsync(cancellationToken);

            var logicsByFieldId = logicRules
                .Where(l => l != null)
                .GroupBy(l => l!.FieldId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(l => l != null).OrderBy(l => l!.Order).Select(l => l!).ToList());

            var defaultNextByFieldId = BuildDefaultNextByOrder(orderedFields);
            var logicPathFieldIds = BuildLogicPathFieldIds(
                orderedFields,
                fieldsById,
                defaultNextByFieldId,
                logicsByFieldId,
                optionsByFieldId,
                answersByFieldId,
                out var logicPathError);

            if (logicPathError != null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, logicPathError);
                return response;
            }

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
            var requiredFieldIds = orderedFields
                .Where(f => f!.IsRequired && logicPathFieldIds.Contains(f.Id))
                .Select(f => f!.Id)
                .ToHashSet();

            var answeredFieldIds = answersByFieldId.Keys.ToHashSet();

            var missingRequiredFields = requiredFieldIds
                .Where(fid => !answeredFieldIds.Contains(fid))
                .ToList();

            if (missingRequiredFields.Any())
            {
                var missingFieldTitles = orderedFields
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
                if (!fieldsById.TryGetValue(answerDto.FieldId, out var field))
                {
                    continue;
                }

                Guid? fieldOptionId = null;

                // Validate and set FieldOptionId for Select/MultiSelect/Radio fields
                if (field.Type == ConstantEnum.FieldType.Select || field.Type == ConstantEnum.FieldType.MultiSelect || field.Type == ConstantEnum.FieldType.Radio)
                {
                    if (answerDto.FieldOptionId.HasValue)
                    {
                        // Validate FieldOptionId belongs to this field
                        if (optionsByFieldId.ContainsKey(field.Id))
                        {
                            var validOption = optionsByFieldId[field.Id]
                                .FirstOrDefault(o => o!.Id == answerDto.FieldOptionId.Value);
                            
                            if (validOption != null)
                            {
                                fieldOptionId = answerDto.FieldOptionId.Value;
                            }
                            else
                            {
                                response.Success = false;
                                response.SetMessage(MessageId.E10000, $"Invalid FieldOptionId for field '{field.Title}'.");
                                return response;
                            }
                        }
                    }
                    // If FieldOptionId not provided, try to find by value
                    else if (optionsByFieldId.ContainsKey(field.Id))
                    {
                        // Extract value from JsonDocument
                        var valueStr = answerDto.Value.RootElement.GetRawText().Trim('"');
                        var matchingOption = optionsByFieldId[field.Id]
                            .FirstOrDefault(o => o!.Value == valueStr);
                        
                        if (matchingOption != null)
                        {
                            fieldOptionId = matchingOption.Id;
                        }
                    }
                }

                var answer = new Answer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    FieldId = answerDto.FieldId,
                    Value = answerDto.Value,
                    FieldOptionId = fieldOptionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "Anonymous",
                    UpdatedBy = "Anonymous"
                };

                await _answerRepository.AddAsync(answer);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _notificationService.CreateSubmissionNotificationAsync(
                    form.UserId,
                    form.Id,
                    form.Title,
                    submission.Id,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Notification failures should not affect submission flow
            }

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
    /// Get all submissions for a form (detail)
    /// </summary>
    public async Task<GetDetailSubmissionsQueryResponse> GetDetailSubmissionsAsync(GetDetailSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetDetailSubmissionsQueryResponse();

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

            // Get all submissions for this form
            var submissions = await _submissionRepository
                .Find(s => s!.FormId == request.FormId && s.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(s => s!.CreatedAt)
                .ToListAsync(cancellationToken);

            // Get all answers for these submissions
            var submissionIds = submissions.Select(s => s!.Id).ToList();
            var allAnswers = await _answerRepository
                .Find(a => submissionIds.Contains(a!.SubmissionId) && a.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            // Get all fields for this form
            var fields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var fieldsById = fields.ToDictionary(f => f!.Id, f => f!);

            // Get all field options
            var fieldIds = fields.Select(f => f!.Id).ToList();
            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var optionsById = allOptions.Where(o => o != null).ToDictionary(o => o!.Id, o => o!);

            // Group answers by submission
            var answersBySubmission = allAnswers
                .Where(a => a != null)
                .GroupBy(a => a!.SubmissionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Build response
            response.Success = true;
            response.SetMessage(MessageId.I00001, "Submissions retrieved successfully.");
            response.Response = submissions.Select(s => new SubmissionResponseEntity
            {
                Id = s!.Id,
                FormId = s.FormId,
                MetaData = s.MetaData,
                CreatedAt = s.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = s.UpdatedAt,
                Answers = answersBySubmission.ContainsKey(s.Id)
                    ? answersBySubmission[s.Id].Select(a => 
                    {
                        var field = fieldsById.ContainsKey(a!.FieldId) ? fieldsById[a.FieldId] : null;
                        var option = a.FieldOptionId.HasValue && optionsById.ContainsKey(a.FieldOptionId.Value)
                            ? optionsById[a.FieldOptionId.Value]
                            : null;

                        return new AnswerResponseEntity
                        {
                            Id = a.Id,
                            FieldId = a.FieldId,
                            FieldTitle = field?.Title ?? "Unknown Field",
                            FieldType = field?.Type.ToString() ?? "Unknown",
                            Value = a.Value,
                            FieldOptionId = a.FieldOptionId,
                            OptionLabel = option?.Label,
                            OptionValue = option?.Value,
                            CreatedAt = a.CreatedAt ?? DateTime.UtcNow
                        };
                    }).OrderBy(a => fieldsById.ContainsKey(a.FieldId) ? fieldsById[a.FieldId].Order : int.MaxValue).ToList()
                    : new List<AnswerResponseEntity>()
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving submissions: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get submissions overview for a form
    /// </summary>
    public async Task<GetSubmissionsOverviewQueryResponse> GetSubmissionsOverviewAsync(GetSubmissionsOverviewQuery request, CancellationToken cancellationToken)
    {
        var response = new GetSubmissionsOverviewQueryResponse();

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

            // Get all fields for this form
            var fields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            // Get all submissions for this form
            var submissions = await _submissionRepository
                .Find(s => s!.FormId == request.FormId && s.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var submissionIds = submissions.Select(s => s!.Id).ToList();
            var fieldIds = fields.Select(f => f!.Id).ToList();

            var answers = submissionIds.Any()
                ? await _answerRepository
                    .Find(a => submissionIds.Contains(a!.SubmissionId) && a.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken)
                : new List<Answer>();

            var options = fieldIds.Any()
                ? await _fieldOptionRepository
                    .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken)
                : new List<FieldOption>();

            var optionsByFieldId = options
                .Where(o => o != null)
                .GroupBy(o => o!.FieldId)
                .ToDictionary(g => g.Key, g => g.Where(o => o != null).Select(o => o!).ToList());

            var answersByFieldId = answers
                .Where(a => a != null)
                .GroupBy(a => a!.FieldId)
                .ToDictionary(g => g.Key, g => g.Where(a => a != null).Select(a => a!).ToList());

            var totalSubmissions = submissions.Count;
            var fieldOverviews = new List<FieldOverviewResponseEntity>();

            foreach (var field in fields)
            {
                var fieldAnswers = answersByFieldId.ContainsKey(field.Id) ? answersByFieldId[field.Id] : new List<Answer>();

                var answeredSubmissionIds = new HashSet<Guid>();
                foreach (var answer in fieldAnswers)
                {
                    if (answer == null) continue;
                    if (answer.FieldOptionId.HasValue || !IsEmptyValue(answer.Value))
                    {
                        answeredSubmissionIds.Add(answer.SubmissionId);
                    }
                }

                var answeredCount = answeredSubmissionIds.Count;
                var emptyCount = Math.Max(totalSubmissions - answeredCount, 0);
                var emptyRate = totalSubmissions > 0 ? (double)emptyCount / totalSubmissions * 100 : 0;

                List<OptionTrendResponseEntity>? optionTrends = null;
                List<ValueTrendResponseEntity>? topValues = null;

                if (optionsByFieldId.ContainsKey(field.Id))
                {
                    optionTrends = BuildOptionTrends(fieldAnswers, optionsByFieldId[field.Id]);
                }
                else
                {
                    topValues = BuildTopValues(fieldAnswers, maxItems: 5);
                }

                fieldOverviews.Add(new FieldOverviewResponseEntity
                {
                    FieldId = field.Id,
                    Title = field.Title,
                    Type = field.Type.ToString(),
                    IsRequired = field.IsRequired,
                    AnsweredCount = answeredCount,
                    EmptyCount = emptyCount,
                    EmptyRate = emptyRate,
                    AnswerCount = fieldAnswers.Count,
                    OptionTrends = optionTrends,
                    TopValues = topValues
                });
            }

            FieldQualityResponseEntity? mostEmptyField = null;
            if (fieldOverviews.Any())
            {
                var worst = fieldOverviews
                    .OrderByDescending(f => f.EmptyRate)
                    .ThenByDescending(f => f.EmptyCount)
                    .First();

                mostEmptyField = new FieldQualityResponseEntity
                {
                    FieldId = worst.FieldId,
                    Title = worst.Title,
                    Type = worst.Type,
                    EmptyCount = worst.EmptyCount,
                    EmptyRate = worst.EmptyRate
                };
            }

            var totalFields = fields.Count;
            var totalPossible = totalSubmissions * totalFields;
            var totalAnswered = fieldOverviews.Sum(f => f.AnsweredCount);
            var completionRate = totalPossible > 0 ? (double)totalAnswered / totalPossible * 100 : 0;

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Submissions overview retrieved successfully.");
            response.Response = new FormSubmissionsOverviewResponseEntity
            {
                FormId = form.Id,
                FormTitle = form.Title,
                TotalSubmissions = totalSubmissions,
                TotalFields = totalFields,
                AnsweredCount = totalAnswered,
                CompletionRate = completionRate,
                MostEmptyField = mostEmptyField,
                Fields = fieldOverviews
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving submissions overview: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get submission by ID (public endpoint for submitter to view their submission)
    /// </summary>
    public async Task<GetSubmissionByIdQueryResponse> GetSubmissionByIdAsync(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetSubmissionByIdQueryResponse();

        try
        {
            // Get submission
            var submission = await _submissionRepository
                .Find(s => s!.Id == request.SubmissionId && s.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (submission == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Submission not found.");
                return response;
            }

            // Get form
            var form = await _formRepository
                .Find(f => f!.Id == submission.FormId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found.");
                return response;
            }

            // Get all answers for this submission
            var answers = await _answerRepository
                .Find(a => a!.SubmissionId == request.SubmissionId && a.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            // Get all fields for this form
            var fields = await _fieldRepository
                .Find(f => f!.FormId == submission.FormId && f.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var fieldsById = fields.ToDictionary(f => f!.Id, f => f!);

            // Get all field options
            var fieldIds = fields.Select(f => f!.Id).ToList();
            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var optionsById = allOptions.Where(o => o != null).ToDictionary(o => o!.Id, o => o!);

            // Build response
            response.Success = true;
            response.SetMessage(MessageId.I00001, "Submission retrieved successfully.");
            response.Response = new SubmissionDetailResponseEntity
            {
                Id = submission.Id,
                FormId = submission.FormId,
                FormTitle = form.Title,
                MetaData = submission.MetaData,
                CreatedAt = submission.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = submission.UpdatedAt,
                Answers = answers
                    .Where(a => a != null)
                    .Select(a =>
                    {
                        var field = fieldsById.ContainsKey(a!.FieldId) ? fieldsById[a.FieldId] : null;
                        var option = a.FieldOptionId.HasValue && optionsById.ContainsKey(a.FieldOptionId.Value)
                            ? optionsById[a.FieldOptionId.Value]
                            : null;

                        return new AnswerDetailResponseEntity
                        {
                            Id = a.Id,
                            FieldId = a.FieldId,
                            FieldTitle = field?.Title ?? "Unknown Field",
                            FieldDescription = field?.Description ?? "",
                            FieldType = field?.Type.ToString() ?? "Unknown",
                            Value = a.Value,
                            FieldOptionId = a.FieldOptionId,
                            OptionLabel = option?.Label,
                            OptionValue = option?.Value,
                            CreatedAt = a.CreatedAt ?? DateTime.UtcNow
                        };
                    })
                    .OrderBy(a => fieldsById.ContainsKey(a.FieldId) ? fieldsById[a.FieldId].Order : int.MaxValue)
                    .ToList()
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving submission: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get next question based on logic rules
    /// </summary>
    public async Task<GetNextQuestionQueryResponse> GetNextQuestionAsync(GetNextQuestionQuery request, CancellationToken cancellationToken)
    {
        var response = new GetNextQuestionQueryResponse();

        try
        {
            // Get form (must be published)
            var form = await _formRepository
                .Find(f => f!.Id == request.FormId && f.IsPublished && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or is not published.");
                return response;
            }

            // Get current field
            var currentField = await _fieldRepository
                .Find(f => f!.Id == request.CurrentFieldId && f.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentField == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Current field not found.");
                return response;
            }

            // Get all fields ordered
            var allFields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            // Get logic rules for current field
            var logicRules = await _logicRepository
                .Find(l => l!.FieldId == request.CurrentFieldId && l.IsActive, cancellationToken: cancellationToken)
                .OrderBy(l => l!.Order)
                .ToListAsync(cancellationToken);

            Guid? nextFieldId = null;
            Guid? appliedLogicId = null;

            // Evaluate logic rules
            if (logicRules.Any() && !IsEmptyValue(request.CurrentValue))
            {
                var currentValues = CollectAnswerValues(request.CurrentValue);
                if (currentValues.Count > 0)
                {
                    foreach (var logic in logicRules)
                    {
                        if (logic == null) continue;

                        var matches = EvaluateLogicCondition(currentField.Type, logic.Condition, currentValues, logic.Value);

                        if (matches)
                        {
                            nextFieldId = logic.DestinationFieldId;
                            appliedLogicId = logic.Id;
                            break;
                        }
                    }
                }
            }

            // If no logic matched, use default order
            if (!nextFieldId.HasValue)
            {
                var currentIndex = allFields.FindIndex(f => f!.Id == currentField.Id);
                if (currentIndex >= 0 && currentIndex + 1 < allFields.Count)
                {
                    nextFieldId = allFields[currentIndex + 1]!.Id;
                }
            }

            // Check if end of form
            if (!nextFieldId.HasValue)
            {
                response.Success = true;
                response.SetMessage(MessageId.I00001, "End of form reached.");
                response.Response = new NextQuestionResponseEntity
                {
                    NextFieldId = null,
                    NextField = null,
                    IsEndOfForm = true,
                    AppliedLogicId = appliedLogicId
                };
                return response;
            }

            // Get next field details
            var targetField = allFields.FirstOrDefault(f => f!.Id == nextFieldId);
            if (targetField == null)
            {
                response.Success = true;
                response.SetMessage(MessageId.I00001, "End of form reached (destination field not found).");
                response.Response = new NextQuestionResponseEntity
                {
                    NextFieldId = null,
                    NextField = null,
                    IsEndOfForm = true,
                    AppliedLogicId = appliedLogicId
                };
                return response;
            }

            // Get options for next field
            var options = await _fieldOptionRepository
                .Find(o => o!.FieldId == nextFieldId && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Next question retrieved successfully.");
            response.Response = new NextQuestionResponseEntity
            {
                NextFieldId = targetField.Id,
                NextField = new FieldResponseEntity
                {
                    Id = targetField.Id,
                    FormId = targetField.FormId,
                    Title = targetField.Title,
                    Description = targetField.Description,
                    ImageUrl = targetField.ImageUrl,
                    Type = targetField.Type.ToString(),
                    Properties = targetField.Properties,
                    IsRequired = targetField.IsRequired,
                    Order = targetField.Order,
                    CreatedAt = targetField.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = targetField.UpdatedAt,
                    Options = options.Any() ? options.Select(o => new FieldOptionResponseEntity
                    {
                        Id = o!.Id,
                        Label = o.Label,
                        Value = o.Value,
                        Order = o.Order
                    }).ToList() : null
                },
                IsEndOfForm = false,
                AppliedLogicId = appliedLogicId
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred: {ex.Message}");
            return response;
        }
    }


    /// <summary>
    /// Build default next-field lookup based on order
    /// </summary>
    private static Dictionary<Guid, Guid?> BuildDefaultNextByOrder(List<Field> orderedFields)
    {
        var nextByFieldId = new Dictionary<Guid, Guid?>();
        for (var i = 0; i < orderedFields.Count; i++)
        {
            var nextId = i + 1 < orderedFields.Count ? orderedFields[i + 1].Id : (Guid?)null;
            nextByFieldId[orderedFields[i].Id] = nextId;
        }

        return nextByFieldId;
    }

    /// <summary>
    /// Build the field path based on submitted answers and logic rules
    /// </summary>
    private static HashSet<Guid> BuildLogicPathFieldIds(
        List<Field> orderedFields,
        IReadOnlyDictionary<Guid, Field> fieldsById,
        IReadOnlyDictionary<Guid, Guid?> defaultNextByFieldId,
        IReadOnlyDictionary<Guid, List<Logic>> logicsByFieldId,
        IReadOnlyDictionary<Guid, List<FieldOption>> optionsByFieldId,
        IReadOnlyDictionary<Guid, List<AnswerDto>> answersByFieldId,
        out string? errorMessage)
    {
        errorMessage = null;
        var path = new HashSet<Guid>();

        if (orderedFields.Count == 0)
        {
            return path;
        }

        Guid? currentFieldId = orderedFields[0].Id;
        var guard = 0;

        while (currentFieldId.HasValue)
        {
            if (!path.Add(currentFieldId.Value))
            {
                errorMessage = "Form logic is invalid (loop detected).";
                break;
            }

            if (!fieldsById.TryGetValue(currentFieldId.Value, out var currentField))
            {
                break;
            }

            Guid? nextFieldId = null;

            if (logicsByFieldId.TryGetValue(currentFieldId.Value, out var logics)
                && answersByFieldId.TryGetValue(currentFieldId.Value, out var currentAnswers)
                && HasAnswerValue(currentAnswers))
            {
                var currentValues = CollectAnswerValues(
                    currentAnswers,
                    optionsByFieldId.TryGetValue(currentFieldId.Value, out var options) ? options : null);

                foreach (var logic in logics)
                {
                    if (logic == null) continue;

                    if (EvaluateLogicCondition(currentField.Type, logic.Condition, currentValues, logic.Value))
                    {
                        nextFieldId = logic.DestinationFieldId;
                        break;
                    }
                }
            }

            if (!nextFieldId.HasValue && defaultNextByFieldId.TryGetValue(currentFieldId.Value, out var defaultNext))
            {
                nextFieldId = defaultNext;
            }

            if (!nextFieldId.HasValue)
            {
                break;
            }

            if (!fieldsById.ContainsKey(nextFieldId.Value))
            {
                break;
            }

            currentFieldId = nextFieldId;
            guard++;

            if (guard > orderedFields.Count + 5)
            {
                errorMessage = "Form logic is invalid (path exceeds field count).";
                break;
            }
        }

        return path;
    }

    private static bool HasAnswerValue(IReadOnlyList<AnswerDto> answers)
    {
        return answers.Any(a => a.FieldOptionId.HasValue || !IsEmptyValue(a.Value));
    }

    private static List<string> CollectAnswerValues(JsonDocument? value)
    {
        var values = new List<string>();
        foreach (var item in ExtractValueStrings(value))
        {
            var normalized = NormalizeValue(item);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                values.Add(normalized);
            }
        }

        return values;
    }

    private static List<string> CollectAnswerValues(IEnumerable<AnswerDto> answers, IReadOnlyList<FieldOption>? fieldOptions)
    {
        var values = new List<string>();

        foreach (var answer in answers)
        {
            foreach (var item in ExtractValueStrings(answer.Value))
            {
                var normalized = NormalizeValue(item);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    values.Add(normalized);
                }
            }

            if (answer.FieldOptionId.HasValue && fieldOptions != null)
            {
                var option = fieldOptions.FirstOrDefault(o => o!.Id == answer.FieldOptionId.Value);
                if (option != null)
                {
                    var optionValue = NormalizeValue(option.Value ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(optionValue))
                    {
                        values.Add(optionValue);
                    }

                    var optionLabel = NormalizeValue(option.Label ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(optionLabel))
                    {
                        values.Add(optionLabel);
                    }
                }
            }
        }

        return values;
    }

    private static string NormalizeValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal))
        {
            return trimmed[1..^1].Trim();
        }

        return trimmed;
    }

    private static bool IsLogicConditionAllowed(ConstantEnum.FieldType fieldType, ConstantEnum.LogicCondition condition)
    {
        return fieldType switch
        {
            ConstantEnum.FieldType.Number or ConstantEnum.FieldType.Rating or ConstantEnum.FieldType.Scale =>
                condition is ConstantEnum.LogicCondition.Is
                    or ConstantEnum.LogicCondition.IsNot
                    or ConstantEnum.LogicCondition.GreaterThan
                    or ConstantEnum.LogicCondition.LessThan
                    or ConstantEnum.LogicCondition.GreaterThanOrEqual
                    or ConstantEnum.LogicCondition.LessThanOrEqual
                    or ConstantEnum.LogicCondition.Always,
            ConstantEnum.FieldType.Date or ConstantEnum.FieldType.Time or ConstantEnum.FieldType.DateTime =>
                condition is ConstantEnum.LogicCondition.Is
                    or ConstantEnum.LogicCondition.IsNot
                    or ConstantEnum.LogicCondition.GreaterThan
                    or ConstantEnum.LogicCondition.LessThan
                    or ConstantEnum.LogicCondition.Always,
            ConstantEnum.FieldType.Select or ConstantEnum.FieldType.Radio or ConstantEnum.FieldType.YesNo =>
                condition is ConstantEnum.LogicCondition.Is
                    or ConstantEnum.LogicCondition.IsNot
                    or ConstantEnum.LogicCondition.Always,
            ConstantEnum.FieldType.MultiSelect =>
                condition is ConstantEnum.LogicCondition.Contains
                    or ConstantEnum.LogicCondition.DoesNotContain
                    or ConstantEnum.LogicCondition.Always,
            _ =>
                condition is ConstantEnum.LogicCondition.Is
                    or ConstantEnum.LogicCondition.IsNot
                    or ConstantEnum.LogicCondition.Contains
                    or ConstantEnum.LogicCondition.DoesNotContain
                    or ConstantEnum.LogicCondition.Always
        };
    }

    /// <summary>
    /// Evaluate logic condition based on field type and values
    /// </summary>
    private static bool EvaluateLogicCondition(
        ConstantEnum.FieldType fieldType,
        ConstantEnum.LogicCondition condition,
        IReadOnlyList<string> currentValues,
        string? logicValue)
    {
        if (!IsLogicConditionAllowed(fieldType, condition))
        {
            return false;
        }

        if (condition == ConstantEnum.LogicCondition.Always)
        {
            return true;
        }

        if (currentValues.Count == 0)
        {
            return false;
        }

        var normalizedLogicValue = NormalizeValue(logicValue ?? string.Empty);
        if (string.IsNullOrWhiteSpace(normalizedLogicValue))
        {
            return false;
        }

        switch (fieldType)
        {
            case ConstantEnum.FieldType.Number:
            case ConstantEnum.FieldType.Rating:
            case ConstantEnum.FieldType.Scale:
                if (!TryParseDouble(normalizedLogicValue, out var logicNumber))
                {
                    return false;
                }

                var currentNumber = GetFirstNumber(currentValues);
                if (!currentNumber.HasValue)
                {
                    return false;
                }

                return condition switch
                {
                    ConstantEnum.LogicCondition.Is => currentNumber.Value == logicNumber,
                    ConstantEnum.LogicCondition.IsNot => currentNumber.Value != logicNumber,
                    ConstantEnum.LogicCondition.GreaterThan => currentNumber.Value > logicNumber,
                    ConstantEnum.LogicCondition.LessThan => currentNumber.Value < logicNumber,
                    ConstantEnum.LogicCondition.GreaterThanOrEqual => currentNumber.Value >= logicNumber,
                    ConstantEnum.LogicCondition.LessThanOrEqual => currentNumber.Value <= logicNumber,
                    _ => false
                };
            case ConstantEnum.FieldType.Date:
            case ConstantEnum.FieldType.DateTime:
                if (!TryParseDateTime(normalizedLogicValue, out var logicDateTime))
                {
                    return false;
                }

                var currentDateTime = GetFirstDateTime(currentValues);
                if (!currentDateTime.HasValue)
                {
                    return false;
                }

                if (fieldType == ConstantEnum.FieldType.Date)
                {
                    logicDateTime = logicDateTime.Date;
                    currentDateTime = currentDateTime.Value.Date;
                }

                return condition switch
                {
                    ConstantEnum.LogicCondition.Is => currentDateTime.Value == logicDateTime,
                    ConstantEnum.LogicCondition.IsNot => currentDateTime.Value != logicDateTime,
                    ConstantEnum.LogicCondition.GreaterThan => currentDateTime.Value > logicDateTime,
                    ConstantEnum.LogicCondition.LessThan => currentDateTime.Value < logicDateTime,
                    _ => false
                };
            case ConstantEnum.FieldType.Time:
                if (!TryParseTime(normalizedLogicValue, out var logicTime))
                {
                    return false;
                }

                var currentTime = GetFirstTime(currentValues);
                if (!currentTime.HasValue)
                {
                    return false;
                }

                return condition switch
                {
                    ConstantEnum.LogicCondition.Is => currentTime.Value == logicTime,
                    ConstantEnum.LogicCondition.IsNot => currentTime.Value != logicTime,
                    ConstantEnum.LogicCondition.GreaterThan => currentTime.Value > logicTime,
                    ConstantEnum.LogicCondition.LessThan => currentTime.Value < logicTime,
                    _ => false
                };
            case ConstantEnum.FieldType.MultiSelect:
                return condition switch
                {
                    ConstantEnum.LogicCondition.Contains => currentValues.Any(v => string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    ConstantEnum.LogicCondition.DoesNotContain => currentValues.All(v => !string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    _ => false
                };
            case ConstantEnum.FieldType.Select:
            case ConstantEnum.FieldType.Radio:
            case ConstantEnum.FieldType.YesNo:
                return condition switch
                {
                    ConstantEnum.LogicCondition.Is => currentValues.Any(v => string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    ConstantEnum.LogicCondition.IsNot => currentValues.All(v => !string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    _ => false
                };
            default:
                return condition switch
                {
                    ConstantEnum.LogicCondition.Is => currentValues.Any(v => string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    ConstantEnum.LogicCondition.IsNot => currentValues.All(v => !string.Equals(v, normalizedLogicValue, StringComparison.OrdinalIgnoreCase)),
                    ConstantEnum.LogicCondition.Contains => currentValues.Any(v => v.IndexOf(normalizedLogicValue, StringComparison.OrdinalIgnoreCase) >= 0),
                    ConstantEnum.LogicCondition.DoesNotContain => currentValues.All(v => v.IndexOf(normalizedLogicValue, StringComparison.OrdinalIgnoreCase) < 0),
                    _ => false
                };
        }
    }

    private static double? GetFirstNumber(IReadOnlyList<string> values)
    {
        foreach (var value in values)
        {
            if (TryParseDouble(value, out var number))
            {
                return number;
            }
        }

        return null;
    }

    private static DateTime? GetFirstDateTime(IReadOnlyList<string> values)
    {
        foreach (var value in values)
        {
            if (TryParseDateTime(value, out var dateTime))
            {
                return dateTime;
            }
        }

        return null;
    }

    private static TimeSpan? GetFirstTime(IReadOnlyList<string> values)
    {
        foreach (var value in values)
        {
            if (TryParseTime(value, out var time))
            {
                return time;
            }
        }

        return null;
    }

    private static bool TryParseDouble(string? input, out double value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = 0;
            return false;
        }

        return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    private static bool TryParseDateTime(string? input, out DateTime value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = default;
            return false;
        }

        return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value)
            || DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out value);
    }

    private static bool TryParseTime(string? input, out TimeSpan value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = default;
            return false;
        }

        return TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out value)
            || TimeSpan.TryParse(input, CultureInfo.CurrentCulture, out value);
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

    private static bool IsEmptyValue(JsonDocument? value)
    {
        if (value == null)
        {
            return true;
        }

        try
        {
            var element = value.RootElement;
            return element.ValueKind switch
            {
                JsonValueKind.Null => true,
                JsonValueKind.Undefined => true,
                JsonValueKind.String => string.IsNullOrWhiteSpace(element.GetString()),
                JsonValueKind.Array => element.GetArrayLength() == 0,
                JsonValueKind.Object => !element.EnumerateObject().Any(),
                _ => false
            };
        }
        catch
        {
            return true;
        }
    }

    private static IEnumerable<string> ExtractValueStrings(JsonDocument? value)
    {
        if (value == null)
        {
            yield break;
        }

        var element = value.RootElement;
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                {
                    var str = element.GetString();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        yield return str;
                    }
                    break;
                }
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                yield return element.ToString();
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var str = item.GetString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            yield return str;
                        }
                    }
                    else if (item.ValueKind == JsonValueKind.Number || item.ValueKind == JsonValueKind.True || item.ValueKind == JsonValueKind.False)
                    {
                        yield return item.ToString();
                    }
                }
                break;
        }
    }

    private static List<OptionTrendResponseEntity> BuildOptionTrends(IEnumerable<Answer> answers, List<FieldOption> options)
    {
        var optionCounts = new Dictionary<Guid, int>();
        var optionByValue = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var optionByLabel = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var option in options)
        {
            optionCounts[option.Id] = 0;
            if (!string.IsNullOrWhiteSpace(option.Value) && !optionByValue.ContainsKey(option.Value))
            {
                optionByValue[option.Value] = option.Id;
            }

            if (!string.IsNullOrWhiteSpace(option.Label) && !optionByLabel.ContainsKey(option.Label))
            {
                optionByLabel[option.Label] = option.Id;
            }
        }

        var totalSelections = 0;
        foreach (var answer in answers)
        {
            if (answer == null) continue;

            if (answer.FieldOptionId.HasValue && optionCounts.ContainsKey(answer.FieldOptionId.Value))
            {
                optionCounts[answer.FieldOptionId.Value]++;
                totalSelections++;
                continue;
            }

            foreach (var value in ExtractValueStrings(answer.Value))
            {
                if (optionByValue.TryGetValue(value, out var optionId) || optionByLabel.TryGetValue(value, out optionId))
                {
                    optionCounts[optionId]++;
                    totalSelections++;
                }
            }
        }

        return options.Select(option =>
        {
            var count = optionCounts.TryGetValue(option.Id, out var c) ? c : 0;
            var rate = totalSelections > 0 ? (double)count / totalSelections * 100 : 0;
            return new OptionTrendResponseEntity
            {
                FieldOptionId = option.Id,
                Label = option.Label,
                Value = option.Value,
                Count = count,
                Rate = rate
            };
        }).ToList();
    }

    private static List<ValueTrendResponseEntity>? BuildTopValues(IEnumerable<Answer> answers, int maxItems)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var answer in answers)
        {
            if (answer == null) continue;
            foreach (var value in ExtractValueStrings(answer.Value))
            {
                if (string.IsNullOrWhiteSpace(value)) continue;
                counts[value] = counts.TryGetValue(value, out var current) ? current + 1 : 1;
            }
        }

        if (!counts.Any())
        {
            return null;
        }

        var total = counts.Values.Sum();
        return counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(maxItems)
            .Select(kv => new ValueTrendResponseEntity
            {
                Value = kv.Key,
                Count = kv.Value,
                Rate = total > 0 ? (double)kv.Value / total * 100 : 0
            })
            .ToList();
    }

    /// <summary>
    /// Update form
    /// </summary>
    public async Task<UpdateFormCommandResponse> UpdateFormAsync(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateFormCommandResponse();

        try
        {
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

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                form.Title = request.Title;
            }

            form.UpdatedAt = DateTime.UtcNow;
            form.UpdatedBy = currentUser.Email;

            _formRepository.Update(form, currentUser.Email);
            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form updated successfully.");
            response.Response = new UpdateFormResponseEntity
            {
                Id = form.Id,
                Title = form.Title,
                Slug = form.Slug,
                ThemeConfig = form.ThemeConfig,
                Settings = form.Settings,
                IsPublished = form.IsPublished,
                UpdatedAt = form.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while updating form: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Delete form (soft delete - set IsActive = false)
    /// </summary>
    public async Task<DeleteFormCommandResponse> DeleteFormAsync(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var response = new DeleteFormCommandResponse();

        try
        {
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

            // Soft delete - set IsActive = false
            _formRepository.Update(form, currentUser.Email, needLogicalDelete: true);
            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken, needLogicalDelete: true);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form deleted successfully.");
            response.Response = new DeleteFormResponseEntity
            {
                Id = form.Id,
                UpdatedAt = form.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while deleting form: {ex.Message}");
            return response;
        }
    }
}
