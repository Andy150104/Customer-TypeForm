using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateFormFromTemplate;
using ClientService.Application.Forms.Commands.CreateTemplate;
using ClientService.Application.Forms.Commands.DeleteTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplateField;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Forms.Queries.GetTemplateWithFields;
using ClientService.Application.Forms.Queries.GetTemplates;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of ITemplateService
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly ICommandRepository<FormTemplate> _templateRepository;
    private readonly ICommandRepository<FormTemplateField> _templateFieldRepository;
    private readonly ICommandRepository<FormTemplateFieldOption> _templateFieldOptionRepository;
    private readonly ICommandRepository<Form> _formRepository;
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly ICommandRepository<FieldOption> _fieldOptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public TemplateService(
        ICommandRepository<FormTemplate> templateRepository,
        ICommandRepository<FormTemplateField> templateFieldRepository,
        ICommandRepository<FormTemplateFieldOption> templateFieldOptionRepository,
        ICommandRepository<Form> formRepository,
        ICommandRepository<Field> fieldRepository,
        ICommandRepository<FieldOption> fieldOptionRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _templateRepository = templateRepository;
        _templateFieldRepository = templateFieldRepository;
        _templateFieldOptionRepository = templateFieldOptionRepository;
        _formRepository = formRepository;
        _fieldRepository = fieldRepository;
        _fieldOptionRepository = fieldOptionRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    /// <summary>
    /// Create a new template for forms
    /// </summary>
    public async Task<CreateTemplateCommandResponse> CreateTemplateAsync(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateTemplateCommandResponse();

        try
        {
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

            if (request.Fields == null || !request.Fields.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "At least one field is required.");
                return response;
            }

            var fieldsWithoutTitle = request.Fields.Where(f => string.IsNullOrWhiteSpace(f.Title)).ToList();
            if (fieldsWithoutTitle.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "All fields must have a title.");
                return response;
            }

            var now = DateTime.UtcNow;
            var template = new FormTemplate
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.UserId,
                Title = request.Title,
                Description = request.Description,
                ThemeConfig = request.ThemeConfig,
                Settings = request.Settings,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email
            };

            var templateFields = new List<FormTemplateField>();
            var templateOptions = new List<FormTemplateFieldOption>();

            for (int i = 0; i < request.Fields.Count; i++)
            {
                var fieldData = request.Fields[i];
                var field = new FormTemplateField
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Title = fieldData.Title,
                    Description = fieldData.Description,
                    ImageUrl = fieldData.ImageUrl,
                    Type = fieldData.Type,
                    Properties = fieldData.Properties,
                    IsRequired = fieldData.IsRequired,
                    Order = i + 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = currentUser.Email,
                    UpdatedBy = currentUser.Email
                };

                templateFields.Add(field);

                if (fieldData.Options != null && fieldData.Options.Any())
                {
                    for (int optionIndex = 0; optionIndex < fieldData.Options.Count; optionIndex++)
                    {
                        var optionData = fieldData.Options[optionIndex];
                        templateOptions.Add(new FormTemplateFieldOption
                        {
                            Id = Guid.NewGuid(),
                            TemplateFieldId = field.Id,
                            Label = optionData.Label,
                            Value = optionData.Value,
                            Order = optionIndex,
                            IsActive = true,
                            CreatedAt = now,
                            UpdatedAt = now,
                            CreatedBy = currentUser.Email,
                            UpdatedBy = currentUser.Email
                        });
                    }
                }
            }

            await _templateRepository.AddAsync(template);
            await _templateFieldRepository.AddRangeAsync(templateFields);
            if (templateOptions.Any())
            {
                await _templateFieldOptionRepository.AddRangeAsync(templateOptions);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Template created successfully.");
            response.Response = new CreateTemplateResponseEntity
            {
                Id = template.Id,
                UserId = currentUser.UserId,
                Title = template.Title,
                Description = template.Description,
                FieldCount = templateFields.Count,
                CreatedAt = template.CreatedAt ?? now
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating template: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Create a new form and fields from template
    /// </summary>
    public async Task<CreateFormFromTemplateCommandResponse> CreateFormFromTemplateAsync(CreateFormFromTemplateCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateFormFromTemplateCommandResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var template = await _templateRepository
                .Find(t => t!.Id == request.TemplateId && t.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template not found or you don't have permission to access it.");
                return response;
            }

            var title = string.IsNullOrWhiteSpace(request.Title) ? template.Title : request.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Title is required.");
                return response;
            }

            var slug = GenerateSlug(title);
            var existingForm = await _formRepository
                .Find(f => f!.UserId == currentUser.UserId && f.Slug == slug && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingForm != null)
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var now = DateTime.UtcNow;
            var form = new Form
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.UserId,
                Title = title,
                Slug = slug,
                ThemeConfig = template.ThemeConfig,
                Settings = template.Settings,
                IsPublished = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email
            };

            var templateFields = await _templateFieldRepository
                .Find(f => f!.TemplateId == template.Id && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            var templateFieldIds = templateFields.Select(f => f!.Id).ToList();
            var templateOptions = new List<FormTemplateFieldOption>();
            if (templateFieldIds.Any())
            {
                templateOptions = await _templateFieldOptionRepository
                    .Find(o => templateFieldIds.Contains(o!.TemplateFieldId) && o.IsActive, cancellationToken: cancellationToken)
                    .OrderBy(o => o!.TemplateFieldId)
                    .ThenBy(o => o!.Order)
                    .ToListAsync(cancellationToken);
            }

            var optionsByTemplateFieldId = templateOptions
                .Where(o => o != null)
                .GroupBy(o => o!.TemplateFieldId)
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o!.Order).ToList());

            var fields = new List<Field>();
            var options = new List<FieldOption>();

            foreach (var templateField in templateFields)
            {
                var newFieldId = Guid.NewGuid();
                var field = new Field
                {
                    Id = newFieldId,
                    FormId = form.Id,
                    Title = templateField.Title,
                    Description = templateField.Description,
                    ImageUrl = templateField.ImageUrl,
                    Type = templateField.Type,
                    Properties = templateField.Properties,
                    IsRequired = templateField.IsRequired,
                    Order = templateField.Order,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = currentUser.Email,
                    UpdatedBy = currentUser.Email
                };

                fields.Add(field);

                if (optionsByTemplateFieldId.TryGetValue(templateField.Id, out var templateFieldOptions))
                {
                    foreach (var templateOption in templateFieldOptions)
                    {
                        options.Add(new FieldOption
                        {
                            Id = Guid.NewGuid(),
                            FieldId = newFieldId,
                            Label = templateOption.Label,
                            Value = templateOption.Value,
                            Order = templateOption.Order,
                            IsActive = true,
                            CreatedAt = now,
                            UpdatedAt = now,
                            CreatedBy = currentUser.Email,
                            UpdatedBy = currentUser.Email
                        });
                    }
                }
            }

            await _formRepository.AddAsync(form);
            if (fields.Any())
            {
                await _fieldRepository.AddRangeAsync(fields);
            }

            if (options.Any())
            {
                await _fieldOptionRepository.AddRangeAsync(options);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Form created from template successfully.");
            response.Response = new CreateFormFromTemplateResponseEntity
            {
                FormId = form.Id,
                TemplateId = template.Id,
                Title = form.Title,
                Slug = form.Slug,
                IsPublished = form.IsPublished,
                FieldCount = fields.Count,
                CreatedAt = form.CreatedAt ?? now
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating form from template: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get all templates for the current user
    /// </summary>
    public async Task<GetTemplatesQueryResponse> GetTemplatesAsync(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var response = new GetTemplatesQueryResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var templates = await _templateRepository
                .Find(t => t!.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(t => t!.CreatedAt)
                .ToListAsync(cancellationToken);

            var templateIds = templates.Select(t => t!.Id).ToList();
            var fieldCountByTemplateId = new Dictionary<Guid, int>();

            if (templateIds.Any())
            {
                var fieldCounts = await _templateFieldRepository
                    .Find(f => templateIds.Contains(f!.TemplateId) && f.IsActive, cancellationToken: cancellationToken)
                    .GroupBy(f => f!.TemplateId)
                    .Select(g => new { TemplateId = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                fieldCountByTemplateId = fieldCounts.ToDictionary(f => f.TemplateId, f => f.Count);
            }

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Templates retrieved successfully.");
            response.Response = templates.Select(t => new TemplateSummaryResponseEntity
            {
                Id = t!.Id,
                Title = t.Title,
                Description = t.Description,
                FieldCount = fieldCountByTemplateId.TryGetValue(t.Id, out var count) ? count : 0,
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving templates: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get template with fields
    /// </summary>
    public async Task<GetTemplateWithFieldsQueryResponse> GetTemplateWithFieldsAsync(GetTemplateWithFieldsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetTemplateWithFieldsQueryResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var template = await _templateRepository
                .Find(t => t!.Id == request.TemplateId && t.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template not found or you don't have permission to access it.");
                return response;
            }

            var fields = await _templateFieldRepository
                .Find(f => f!.TemplateId == request.TemplateId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            var fieldIds = fields.Select(f => f!.Id).ToList();
            var allOptions = new List<FormTemplateFieldOption>();
            if (fieldIds.Any())
            {
                allOptions = await _templateFieldOptionRepository
                    .Find(o => fieldIds.Contains(o!.TemplateFieldId) && o.IsActive, cancellationToken: cancellationToken)
                    .OrderBy(o => o!.TemplateFieldId)
                    .ThenBy(o => o!.Order)
                    .ToListAsync(cancellationToken);
            }

            var optionsByFieldId = allOptions
                .Where(o => o != null)
                .GroupBy(o => o!.TemplateFieldId)
                .ToDictionary(g => g.Key, g => g.Select(o => new FieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList());

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Template retrieved successfully.");
            response.Response = new TemplateWithFieldsResponseEntity
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                ThemeConfig = template.ThemeConfig,
                Settings = template.Settings,
                CreatedAt = template.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = template.UpdatedAt,
                Fields = fields.Select(f => new TemplateFieldResponseEntity
                {
                    Id = f!.Id,
                    TemplateId = f.TemplateId,
                    Title = f.Title,
                    Description = f.Description,
                    ImageUrl = f.ImageUrl,
                    Type = f.Type.ToString(),
                    Properties = f.Properties,
                    IsRequired = f.IsRequired,
                    Order = f.Order,
                    CreatedAt = f.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = f.UpdatedAt,
                    Options = optionsByFieldId.ContainsKey(f.Id) ? optionsByFieldId[f.Id] : null
                }).ToList()
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving template: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Update template
    /// </summary>
    public async Task<UpdateTemplateCommandResponse> UpdateTemplateAsync(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateTemplateCommandResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var template = await _templateRepository
                .Find(t => t!.Id == request.TemplateId && t.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template not found or you don't have permission to access it.");
                return response;
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                template.Title = request.Title;
            }

            if (request.Description != null)
            {
                template.Description = request.Description;
            }

            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedBy = currentUser.Email;

            _templateRepository.Update(template, currentUser.Email);
            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Template updated successfully.");
            response.Response = new UpdateTemplateResponseEntity
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                ThemeConfig = template.ThemeConfig,
                Settings = template.Settings,
                UpdatedAt = template.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while updating template: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Delete template (soft delete)
    /// </summary>
    public async Task<DeleteTemplateCommandResponse> DeleteTemplateAsync(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var response = new DeleteTemplateCommandResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var template = await _templateRepository
                .Find(t => t!.Id == request.TemplateId && t.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template not found or you don't have permission to access it.");
                return response;
            }

            var templateFields = await _templateFieldRepository
                .Find(f => f!.TemplateId == template.Id && f.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            var templateFieldIds = templateFields.Select(f => f!.Id).ToList();
            var templateOptions = new List<FormTemplateFieldOption>();
            if (templateFieldIds.Any())
            {
                templateOptions = await _templateFieldOptionRepository
                    .Find(o => templateFieldIds.Contains(o!.TemplateFieldId) && o.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken);
            }

            _templateRepository.Update(template, currentUser.Email, needLogicalDelete: true);

            if (templateFields.Any())
            {
                _templateFieldRepository.UpdateRange(templateFields, currentUser.Email, needLogicalDelete: true);
            }

            if (templateOptions.Any())
            {
                _templateFieldOptionRepository.UpdateRange(templateOptions, currentUser.Email, needLogicalDelete: true);
            }

            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken, needLogicalDelete: true);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Template deleted successfully.");
            response.Response = new DeleteTemplateResponseEntity
            {
                Id = template.Id,
                UpdatedAt = template.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while deleting template: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Update template field
    /// </summary>
    public async Task<UpdateTemplateFieldCommandResponse> UpdateTemplateFieldAsync(UpdateTemplateFieldCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateTemplateFieldCommandResponse();

        try
        {
            var currentUser = _identityService.GetCurrentUser();
            if (currentUser == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E11006);
                return response;
            }

            var field = await _templateFieldRepository
                .Find(f => f!.Id == request.TemplateFieldId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (field == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template field not found.");
                return response;
            }

            var template = await _templateRepository
                .Find(t => t!.Id == field.TemplateId && t.UserId == currentUser.UserId && t.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Template not found or you don't have permission to access it.");
                return response;
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                field.Title = request.Title;
            }

            if (request.Description != null)
            {
                field.Description = request.Description;
            }

            if (request.ImageUrl != null)
            {
                field.ImageUrl = request.ImageUrl;
            }

            if (request.Type.HasValue)
            {
                field.Type = request.Type.Value;
            }

            if (request.Properties != null)
            {
                field.Properties = request.Properties;
            }

            if (request.IsRequired.HasValue)
            {
                field.IsRequired = request.IsRequired.Value;
            }

            field.UpdatedAt = DateTime.UtcNow;
            field.UpdatedBy = currentUser.Email;

            _templateFieldRepository.Update(field, currentUser.Email);

            if (request.Options != null)
            {
                var existingOptions = await _templateFieldOptionRepository
                    .Find(o => o!.TemplateFieldId == request.TemplateFieldId && o.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken);

                var existingOptionIds = existingOptions.Where(o => o != null).Select(o => o!.Id).ToHashSet();
                var providedOptionIds = request.Options
                    .Where(o => o.Id.HasValue)
                    .Select(o => o.Id!.Value)
                    .ToHashSet();

                var optionsToDelete = existingOptions
                    .Where(o => o != null && !providedOptionIds.Contains(o!.Id))
                    .ToList();

                foreach (var option in optionsToDelete)
                {
                    if (option != null)
                    {
                        _templateFieldOptionRepository.Update(option, currentUser.Email, needLogicalDelete: true);
                    }
                }

                var optionsToAdd = new List<FormTemplateFieldOption>();
                for (int i = 0; i < request.Options.Count; i++)
                {
                    var optionDto = request.Options[i];
                    if (optionDto.Id.HasValue && existingOptionIds.Contains(optionDto.Id.Value))
                    {
                        var existingOption = existingOptions.FirstOrDefault(o => o!.Id == optionDto.Id.Value);
                        if (existingOption != null)
                        {
                            existingOption.Label = optionDto.Label;
                            existingOption.Value = optionDto.Value;
                            existingOption.Order = i;
                            existingOption.UpdatedAt = DateTime.UtcNow;
                            existingOption.UpdatedBy = currentUser.Email;
                            _templateFieldOptionRepository.Update(existingOption, currentUser.Email);
                        }
                    }
                    else
                    {
                        var newOption = new FormTemplateFieldOption
                        {
                            Id = Guid.NewGuid(),
                            TemplateFieldId = request.TemplateFieldId,
                            Label = optionDto.Label,
                            Value = optionDto.Value,
                            Order = i,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = currentUser.Email,
                            UpdatedBy = currentUser.Email
                        };
                        optionsToAdd.Add(newOption);
                    }
                }

                if (optionsToAdd.Any())
                {
                    await _templateFieldOptionRepository.AddRangeAsync(optionsToAdd, currentUser.Email);
                }
            }

            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

            var updatedOptions = await _templateFieldOptionRepository
                .Find(o => o!.TemplateFieldId == request.TemplateFieldId && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Template field updated successfully.");
            response.Response = new UpdateTemplateFieldResponseEntity
            {
                Id = field.Id,
                TemplateId = field.TemplateId,
                Title = field.Title,
                Description = field.Description,
                ImageUrl = field.ImageUrl,
                Type = field.Type.ToString(),
                Properties = field.Properties,
                IsRequired = field.IsRequired,
                Order = field.Order,
                UpdatedAt = field.UpdatedAt ?? DateTime.UtcNow,
                Options = updatedOptions.Any()
                    ? updatedOptions.Select(o => new UpdateTemplateFieldOptionResponseEntity
                    {
                        Id = o!.Id,
                        Label = o.Label,
                        Value = o.Value,
                        Order = o.Order
                    }).ToList()
                    : null
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while updating template field: {ex.Message}");
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
