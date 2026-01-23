using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateForm;
using ClientService.Application.Forms.Commands.SubmitForm;
using ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetForms;
using ClientService.Application.Forms.Queries.GetNextQuestion;
using ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetSubmissions;
using ClientService.Application.Forms.Queries.GetSubmissionById;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using ClientService.Domain.Entities.Enums;
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
    private readonly ICommandRepository<FieldOption> _fieldOptionRepository;
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
        ICommandRepository<FieldOption> fieldOptionRepository,
        ICommandRepository<Submission> submissionRepository,
        ICommandRepository<Answer> answerRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _formRepository = formRepository;
        _fieldRepository = fieldRepository;
        _logicRepository = logicRepository;
        _fieldOptionRepository = fieldOptionRepository;
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

            var fieldIds = fields.Select(f => f!.Id).ToHashSet();
            
            // Get all field options for fields that have options (Select, MultiSelect, Radio)
            var optionFieldIds = fields
                .Where(f => f!.Type == FieldType.Select || f.Type == FieldType.MultiSelect || f.Type == FieldType.Radio)
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
                var field = fields.FirstOrDefault(f => f!.Id == answerDto.FieldId);
                if (field == null) continue;

                Guid? fieldOptionId = null;

                // Validate and set FieldOptionId for Select/MultiSelect/Radio fields
                if (field.Type == FieldType.Select || field.Type == FieldType.MultiSelect || field.Type == FieldType.Radio)
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
    /// Get all submissions for a form
    /// </summary>
    public async Task<GetSubmissionsQueryResponse> GetSubmissionsAsync(GetSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetSubmissionsQueryResponse();

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
            if (logicRules.Any() && request.CurrentValue != null)
            {
                var currentValue = request.CurrentValue.RootElement.ToString();

                foreach (var logic in logicRules)
                {
                    if (logic == null) continue;

                    var matches = EvaluateLogicCondition(logic.Condition, currentValue, logic.Value);

                    if (matches)
                    {
                        nextFieldId = logic.DestinationFieldId;
                        appliedLogicId = logic.Id;
                        break;
                    }
                }
            }

            // If no logic matched, use default order
            if (!nextFieldId.HasValue)
            {
                var nextField = allFields.FirstOrDefault(f => f!.Order == currentField.Order + 1);
                nextFieldId = nextField?.Id;
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
    /// Evaluate logic condition
    /// </summary>
    private static bool EvaluateLogicCondition(LogicCondition condition, string? currentValue, string? logicValue)
    {
        if (currentValue == null) return condition == LogicCondition.Always;

        return condition switch
        {
            LogicCondition.Is => string.Equals(currentValue, logicValue, StringComparison.OrdinalIgnoreCase),
            LogicCondition.IsNot => !string.Equals(currentValue, logicValue, StringComparison.OrdinalIgnoreCase),
            LogicCondition.Contains => currentValue.Contains(logicValue ?? "", StringComparison.OrdinalIgnoreCase),
            LogicCondition.DoesNotContain => !currentValue.Contains(logicValue ?? "", StringComparison.OrdinalIgnoreCase),
            LogicCondition.GreaterThan => double.TryParse(currentValue, out var cv1) && double.TryParse(logicValue, out var lv1) && cv1 > lv1,
            LogicCondition.LessThan => double.TryParse(currentValue, out var cv2) && double.TryParse(logicValue, out var lv2) && cv2 < lv2,
            LogicCondition.GreaterThanOrEqual => double.TryParse(currentValue, out var cv3) && double.TryParse(logicValue, out var lv3) && cv3 >= lv3,
            LogicCondition.LessThanOrEqual => double.TryParse(currentValue, out var cv4) && double.TryParse(logicValue, out var lv4) && cv4 <= lv4,
            LogicCondition.Always => true,
            _ => false
        };
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
