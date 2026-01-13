using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateField;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using ClientService.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of IFieldService
/// </summary>
public class FieldService : IFieldService
{
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly ICommandRepository<Form> _formRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public FieldService(
        ICommandRepository<Field> fieldRepository,
        ICommandRepository<Form> formRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _fieldRepository = fieldRepository;
        _formRepository = formRepository;
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
                CreatedAt = field.CreatedAt ?? DateTime.UtcNow
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
                UpdatedAt = f.UpdatedAt
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
