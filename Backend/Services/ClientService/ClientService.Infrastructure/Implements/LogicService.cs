using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateOrUpdateLogic;
using ClientService.Application.Interfaces.FormServices;
using ClientService.Domain.Entities;
using ClientService.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClientService.Infrastructure.Implements;

/// <summary>
/// Implementation of ILogicService
/// </summary>
public class LogicService : ILogicService
{
    private readonly ICommandRepository<Logic> _logicRepository;
    private readonly ICommandRepository<Field> _fieldRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public LogicService(
        ICommandRepository<Logic> logicRepository,
        ICommandRepository<Field> fieldRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _logicRepository = logicRepository;
        _fieldRepository = fieldRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    /// <summary>
    /// Create or update logic rule
    /// If logic with same FieldId, Condition, and Value exists, update it
    /// Otherwise, create new logic
    /// </summary>
    public async Task<CreateOrUpdateLogicCommandResponse> CreateOrUpdateLogicAsync(CreateOrUpdateLogicCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateOrUpdateLogicCommandResponse();

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

            // Validate FieldId exists and belongs to current user's form
            var field = await _fieldRepository
                .Find(f => f!.Id == request.FieldId && f.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (field == null)
            {
                response.Success = false;
                response.SetMessage(MessageId.E10000, "Field not found.");
                return response;
            }

            // Validate DestinationFieldId if provided
            if (request.DestinationFieldId.HasValue)
            {
                var destinationField = await _fieldRepository
                    .Find(f => f!.Id == request.DestinationFieldId.Value && f.FormId == field.FormId && f.IsActive, cancellationToken: cancellationToken)
                    .FirstOrDefaultAsync(cancellationToken);

                if (destinationField == null)
                {
                    response.Success = false;
                    response.SetMessage(MessageId.E10000, "Destination field not found or does not belong to the same form.");
                    return response;
                }
            }

            // Auto-determine LogicGroupId and Order
            // Check if there are existing logics for this field
            var existingLogics = await _logicRepository
                .Find(l => l!.FieldId == request.FieldId && l.IsActive, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            Guid? logicGroupId = null;
            int order = 0;

            if (existingLogics.Any())
            {
                // Check if there's a logic group already
                var existingGroupId = existingLogics.FirstOrDefault(l => l!.LogicGroupId.HasValue)?.LogicGroupId;
                logicGroupId = existingGroupId ?? Guid.NewGuid();

                // Calculate order based on existing logics in the same group
                var existingLogicsInGroup = existingLogics
                    .Where(l => l!.LogicGroupId == logicGroupId)
                    .OrderByDescending(l => l!.Order)
                    .ToList();

                if (existingLogicsInGroup.Any())
                {
                    order = existingLogicsInGroup.First()!.Order + 1;
                }
                else
                {
                    order = 0; // First logic in group
                }
            }
            else
            {
                // First logic for this field - no group needed (standalone)
                order = 0;
            }

            // Check if logic with same FieldId, Condition, and Value already exists
            var existingLogic = await _logicRepository
                .Find(l => l!.FieldId == request.FieldId 
                    && l.Condition == request.Condition 
                    && l.Value == request.Value 
                    && l.LogicGroupId == logicGroupId
                    && l.IsActive, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);

            Logic logic;
            if (existingLogic != null)
            {
                // Update existing logic
                existingLogic.DestinationFieldId = request.DestinationFieldId;
                existingLogic.Order = order;
                existingLogic.UpdatedAt = DateTime.UtcNow;
                existingLogic.UpdatedBy = currentUser.Email;
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                logic = existingLogic;
            }
            else
            {
                // Create new logic
                logic = new Logic
                {
                    Id = Guid.NewGuid(),
                    FieldId = request.FieldId,
                    Condition = request.Condition,
                    Value = request.Value,
                    DestinationFieldId = request.DestinationFieldId,
                    Order = order,
                    LogicGroupId = logicGroupId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.Email,
                    UpdatedBy = currentUser.Email
                };

                await _logicRepository.AddAsync(logic);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            response.Success = true;
            response.SetMessage(MessageId.I00001, existingLogic != null ? "Logic updated successfully." : "Logic created successfully.");
            response.Response = new LogicResponseEntity
            {
                Id = logic.Id,
                FieldId = logic.FieldId,
                Condition = logic.Condition.ToString(),
                Value = logic.Value,
                DestinationFieldId = logic.DestinationFieldId,
                Order = logic.Order,
                LogicGroupId = logic.LogicGroupId,
                CreatedAt = logic.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = logic.UpdatedAt
            };

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.SetMessage(MessageId.E00000, $"An error occurred while creating/updating logic: {ex.Message}");
            return response;
        }
    }
}
