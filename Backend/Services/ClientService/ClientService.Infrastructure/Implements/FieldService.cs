using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateField;
using ClientService.Application.Forms.Commands.CreateMultipleField;
using ClientService.Application.Forms.Commands.DeleteField;
using ClientService.Application.Forms.Commands.UpdateField;
using ClientService.Application.Forms.Commands.ReorderFields;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using GetFieldsByFormId = ClientService.Application.Forms.Queries.GetFieldsByFormId;
using UpdateField = ClientService.Application.Forms.Commands.UpdateField;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of IFieldService
/// </summary>
public class FieldService : IFieldService
{
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly ICommandRepository<Form> _formRepository;
    private readonly ICommandRepository<FieldOption> _fieldOptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public FieldService(
        ICommandRepository<Field> fieldRepository,
        ICommandRepository<Form> formRepository,
        ICommandRepository<FieldOption> fieldOptionRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _fieldRepository = fieldRepository;
        _formRepository = formRepository;
        _fieldOptionRepository = fieldOptionRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    /// <summary>
    /// Create a new field for a form
    /// </summary>
    public async Task<CreateFieldCommandResponse> CreateFieldAsync(CreateFieldCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateFieldCommandResponse();

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

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Title is required.");
                return response;
            }

            // Get the maximum Order value for fields in this form, then add 1
            var maxOrder = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(f => f!.Order)
                .Select(f => f!.Order)
                .FirstOrDefaultAsync(cancellationToken);

            var newOrder = maxOrder + 1;

            // Create new field
            var field = new Field
            {
                Id = Guid.NewGuid(),
                FormId = request.FormId,
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                Type = request.Type,
                Properties = request.Properties,
                IsRequired = request.IsRequired,
                Order = newOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email
            };

            await _fieldRepository.AddAsync(field);
            
            // Create options if provided
            if (request.Options != null && request.Options.Any())
            {
                var options = request.Options.Select((opt, index) => new FieldOption
                {
                    Id = Guid.NewGuid(),
                    FieldId = field.Id,
                    Label = opt.Label,
                    Value = opt.Value,
                    Order = index,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Email,
                    UpdatedBy = currentUser.Email
                }).ToList();
                
                await _fieldOptionRepository.AddRangeAsync(options);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Field created successfully.");
            response.Response = new CreateFieldResponseEntity
            {
                Id = field.Id,
                FormId = field.FormId,
                Title = field.Title,
                Description = field.Description,
                ImageUrl = field.ImageUrl,
                Type = field.Type.ToString(),
                Properties = field.Properties,
                IsRequired = field.IsRequired,
                Order = field.Order,
                CreatedAt = field.CreatedAt ?? DateTime.UtcNow,
                Options = null
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating field: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Create multiple fields for a form at once
    /// </summary>
    public async Task<CreateMultipleFieldCommandResponse> CreateMultipleFieldsAsync(CreateMultipleFieldCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateMultipleFieldCommandResponse();

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

            if (request.Fields == null || !request.Fields.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "At least one field is required.");
                return response;
            }

            // Validate all fields have titles
            var fieldsWithoutTitle = request.Fields.Where(f => string.IsNullOrWhiteSpace(f.Title)).ToList();
            if (fieldsWithoutTitle.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "All fields must have a title.");
                return response;
            }

            // Get the maximum Order value for fields in this form
            var maxOrder = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderByDescending(f => f!.Order)
                .Select(f => f!.Order)
                .FirstOrDefaultAsync(cancellationToken);

            var fields = new List<Field>();
            var allOptions = new List<FieldOption>();
            var startOrder = maxOrder + 1;

            // Create all fields
            for (int i = 0; i < request.Fields.Count; i++)
            {
                var fieldData = request.Fields[i];
                var field = new Field
                {
                    Id = Guid.NewGuid(),
                    FormId = request.FormId,
                    Title = fieldData.Title,
                    Description = fieldData.Description,
                    ImageUrl = fieldData.ImageUrl,
                    Type = fieldData.Type,
                    Properties = fieldData.Properties,
                    IsRequired = fieldData.IsRequired,
                    Order = startOrder + i,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Email,
                    UpdatedBy = currentUser.Email
                };

                fields.Add(field);

                // Create options if provided
                if (fieldData.Options != null && fieldData.Options.Any())
                {
                    var options = fieldData.Options.Select((opt, index) => new FieldOption
                    {
                        Id = Guid.NewGuid(),
                        FieldId = field.Id,
                        Label = opt.Label,
                        Value = opt.Value,
                        Order = index,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = currentUser.Email,
                        UpdatedBy = currentUser.Email
                    }).ToList();

                    allOptions.AddRange(options);
                }
            }

            // Add all fields and options
            await _fieldRepository.AddRangeAsync(fields);
            if (allOptions.Any())
            {
                await _fieldOptionRepository.AddRangeAsync(allOptions);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, $"{fields.Count} field(s) created successfully.");
            response.Response = new List<CreateFieldResponseEntity>();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating fields: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Get all fields by form ID, ordered by Order field
    /// </summary>
    public async Task<GetFieldsByFormIdQueryResponse> GetFieldsByFormIdAsync(GetFieldsByFormIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetFieldsByFormIdQueryResponse();

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

            // Get all field IDs
            var fieldIds = fields.Select(f => f!.Id).ToList();

            // Get all options for these fields
            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.FieldId)
                .ThenBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            // Group options by FieldId
            var optionsByFieldId = allOptions
                .GroupBy(o => o!.FieldId)
                .ToDictionary(g => g.Key, g => g.Select(o => new GetFieldsByFormId.FieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList());

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Fields retrieved successfully.");
            response.Response = fields.Select(f => new FieldResponseEntity
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
                Options = optionsByFieldId.ContainsKey(f.Id) ? optionsByFieldId[f.Id] : null
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while retrieving fields: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Update field
    /// </summary>
    public async Task<UpdateFieldCommandResponse> UpdateFieldAsync(UpdateFieldCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateFieldCommandResponse();

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

            // Validate FieldId exists and belongs to a form owned by current user
            var field = await _fieldRepository
                .Find(f => f!.Id == request.FieldId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (field == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Field not found.");
                return response;
            }

            // Validate form belongs to current user
            var form = await _formRepository
                .Find(f => f!.Id == field.FormId && f.UserId == currentUser.UserId && f.IsActive, cancellationToken: cancellationToken)
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

            _fieldRepository.Update(field, currentUser.Email);

            // Handle options update
            if (request.Options != null)
            {
                // Get existing options
                var existingOptions = await _fieldOptionRepository
                    .Find(o => o!.FieldId == request.FieldId && o.IsActive, cancellationToken: cancellationToken)
                    .ToListAsync(cancellationToken);

                var existingOptionIds = existingOptions.Where(o => o != null).Select(o => o!.Id).ToHashSet();
                var providedOptionIds = request.Options
                    .Where(o => o.Id.HasValue)
                    .Select(o => o.Id!.Value)
                    .ToHashSet();

                // Soft delete options that are not in the provided list
                var optionsToDelete = existingOptions
                    .Where(o => o != null && !providedOptionIds.Contains(o!.Id))
                    .ToList();

                foreach (var option in optionsToDelete)
                {
                    if (option != null)
                    {
                        _fieldOptionRepository.Update(option, currentUser.Email, needLogicalDelete: true);
                    }
                }

                // Update or create options
                var optionsToAdd = new List<FieldOption>();
                for (int i = 0; i < request.Options.Count; i++)
                {
                    var optionDto = request.Options[i];
                    if (optionDto.Id.HasValue && existingOptionIds.Contains(optionDto.Id.Value))
                    {
                        // Update existing option
                        var existingOption = existingOptions.FirstOrDefault(o => o!.Id == optionDto.Id.Value);
                        if (existingOption != null)
                        {
                            existingOption.Label = optionDto.Label;
                            existingOption.Value = optionDto.Value;
                            existingOption.Order = i;
                            existingOption.UpdatedAt = DateTime.UtcNow;
                            existingOption.UpdatedBy = currentUser.Email;
                            _fieldOptionRepository.Update(existingOption, currentUser.Email);
                        }
                    }
                    else
                    {
                        // Create new option
                        var newOption = new FieldOption
                        {
                            Id = Guid.NewGuid(),
                            FieldId = request.FieldId,
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
                    await _fieldOptionRepository.AddRangeAsync(optionsToAdd, currentUser.Email);
                }
            }

            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

            // Get updated options for response
            var updatedOptions = await _fieldOptionRepository
                .Find(o => o!.FieldId == request.FieldId && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Field updated successfully.");
            response.Response = new UpdateFieldResponseEntity
            {
                Id = field.Id,
                FormId = field.FormId,
                Title = field.Title,
                Description = field.Description,
                ImageUrl = field.ImageUrl,
                Type = field.Type.ToString(),
                Properties = field.Properties,
                IsRequired = field.IsRequired,
                Order = field.Order,
                UpdatedAt = field.UpdatedAt ?? DateTime.UtcNow,
                Options = updatedOptions.Any() ? updatedOptions.Select(o => new UpdateFieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList() : null
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while updating field: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Delete field (soft delete - set IsActive = false)
    /// </summary>
    public async Task<DeleteFieldCommandResponse> DeleteFieldAsync(DeleteFieldCommand request, CancellationToken cancellationToken)
    {
        var response = new DeleteFieldCommandResponse();

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

            // Validate FieldId exists and belongs to a form owned by current user
            var field = await _fieldRepository
                .Find(f => f!.Id == request.FieldId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (field == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Field not found.");
                return response;
            }

            // Validate form belongs to current user
            var form = await _formRepository
                .Find(f => f!.Id == field.FormId && f.UserId == currentUser.UserId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (form == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Form not found or you don't have permission to access it.");
                return response;
            }

            // Soft delete - set IsActive = false
            _fieldRepository.Update(field, currentUser.Email, needLogicalDelete: true);
            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken, needLogicalDelete: true);

            // Reorder remaining fields to remove gaps after delete
            var remainingFields = await _fieldRepository
                .Find(f => f!.FormId == field.FormId && f.IsActive && f.Id != field.Id, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            if (remainingFields.Count > 0)
            {
                var minOrder = 1;
                var now = DateTime.UtcNow;
                var hasChanges = false;

                for (int i = 0; i < remainingFields.Count; i++)
                {
                    var remainingField = remainingFields[i]!;
                    var newOrder = minOrder + i;
                    if (remainingField.Order != newOrder)
                    {
                        remainingField.Order = newOrder;
                        remainingField.UpdatedAt = now;
                        remainingField.UpdatedBy = currentUser.Email;
                        _fieldRepository.Update(remainingField, currentUser.Email);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);
                }
            }

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Field deleted successfully.");
            response.Response = new DeleteFieldResponseEntity
            {
                Id = field.Id,
                UpdatedAt = field.UpdatedAt ?? DateTime.UtcNow
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while deleting field: {ex.Message}");
            return response;
        }
    }

    /// <summary>
    /// Reorder fields in a form
    /// </summary>
    public async Task<ReorderFieldsCommandResponse> ReorderFieldsAsync(ReorderFieldsCommand request, CancellationToken cancellationToken)
    {
        var response = new ReorderFieldsCommandResponse();

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

            if (request.FieldIdsInOrder == null || !request.FieldIdsInOrder.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "FieldIdsInOrder is required.");
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

            // Get all active fields of this form
            var fields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            if (!fields.Any())
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "No fields found for this form.");
                return response;
            }

            var existingFieldIds = fields.Select(f => f!.Id).ToHashSet();
            var distinctRequestedIds = request.FieldIdsInOrder.Distinct().ToList();

            // Validate: all fields in form must be included exactly once
            if (distinctRequestedIds.Count != existingFieldIds.Count ||
                !distinctRequestedIds.All(id => existingFieldIds.Contains(id)))
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "FieldIdsInOrder is invalid for this form.");
                return response;
            }

            // Keep the current minimum order as base to avoid breaking relative ordering logic
            var minOrder = fields.Min(f => f!.Order);

            // Apply new order
            for (int i = 0; i < distinctRequestedIds.Count; i++)
            {
                var fieldId = distinctRequestedIds[i];
                var field = fields.First(f => f!.Id == fieldId)!;
                field.Order = minOrder + i;
                field.UpdatedAt = DateTime.UtcNow;
                field.UpdatedBy = currentUser.Email;
                _fieldRepository.Update(field, currentUser.Email);
            }

            await _unitOfWork.SaveChangesAsync(currentUser.Email, cancellationToken);

            // Build response similar to GetFieldsByFormId
            var updatedFields = await _fieldRepository
                .Find(f => f!.FormId == request.FormId && f.IsActive, cancellationToken: cancellationToken)
                .OrderBy(f => f!.Order)
                .ToListAsync(cancellationToken);

            var fieldIds = updatedFields.Select(f => f!.Id).ToList();

            var allOptions = await _fieldOptionRepository
                .Find(o => fieldIds.Contains(o!.FieldId) && o.IsActive, cancellationToken: cancellationToken)
                .OrderBy(o => o!.FieldId)
                .ThenBy(o => o!.Order)
                .ToListAsync(cancellationToken);

            var optionsByFieldId = allOptions
                .GroupBy(o => o!.FieldId)
                .ToDictionary(g => g.Key, g => g.Select(o => new GetFieldsByFormId.FieldOptionResponseEntity
                {
                    Id = o!.Id,
                    Label = o.Label,
                    Value = o.Value,
                    Order = o.Order
                }).ToList());

            response.Success = true;
            response.SetMessage(MessageId.I00001, "Fields reordered successfully.");
            response.Response = updatedFields.Select(f => new GetFieldsByFormId.FieldResponseEntity
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
                Options = optionsByFieldId.ContainsKey(f.Id) ? optionsByFieldId[f.Id] : null
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while reordering fields: {ex.Message}");
            return response;
        }
    }
}
