using BaseService.Application.Interfaces.IdentityHepers;
using BaseService.Application.Interfaces.Repositories;
using BaseService.Common.Utils.Const;
using ClientService.Application.Forms.Commands.CreateForm;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormService(
        ICommandRepository<Form> formRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _formRepository = formRepository;
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
