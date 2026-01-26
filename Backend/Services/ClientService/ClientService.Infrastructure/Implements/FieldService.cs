using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateField;
using ClientService.Application.Forms.Commands.CreateMultipleField;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
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
                .ToDictionary(g => g.Key, g => g.Select(o => new FieldOptionResponseEntity
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
}
