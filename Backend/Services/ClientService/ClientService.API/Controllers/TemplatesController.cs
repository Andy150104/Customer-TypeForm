using BaseService.API.BaseControllers;
using BaseService.Application.Interfaces.IdentityHepers;
using ClientService.Application.Forms.Commands.CreateFormFromTemplate;
using ClientService.Application.Forms.Commands.CreateTemplate;
using ClientService.Application.Forms.Commands.DeleteTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplate;
using ClientService.Application.Forms.Commands.UpdateTemplateField;
using ClientService.Application.Forms.Queries.GetTemplateWithFields;
using ClientService.Application.Forms.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClientService.API.Controllers.Templates;

[ApiController]
[Route("api/v1/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private IdentityEntity _identityEntity = null!;

    /// <summary>
    /// Constructor
    /// </summary>
    public TemplatesController(
        IMediator mediator,
        IIdentityService identityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _identityService = identityService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create a new template
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Template ID and details</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Create new template",
        Description = "Create template with fields"
    )]
    public async Task<CreateTemplateCommandResponse> CreateTemplate([FromBody] CreateTemplateCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateTemplateCommand, CreateTemplateCommandResponse, CreateTemplateResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateTemplateCommandResponse()
        );
    }

    /// <summary>
    /// Create a new form from template
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Form ID and details</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Create form from template",
        Description = "Auto create form and fields from template"
    )]
    public async Task<CreateFormFromTemplateCommandResponse> CreateFormFromTemplate([FromBody] CreateFormFromTemplateCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateFormFromTemplateCommand, CreateFormFromTemplateCommandResponse, CreateFormFromTemplateResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateFormFromTemplateCommandResponse()
        );
    }

    /// <summary>
    /// Get all templates for the current user
    /// </summary>
    /// <returns>List of templates ordered by CreatedAt descending</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Get templates",
        Description = "Get all templates for current user"
    )]
    public async Task<GetTemplatesQueryResponse> GetTemplates()
    {
        var request = new GetTemplatesQuery();
        return await ApiControllerHelper.HandleRequest<GetTemplatesQuery, GetTemplatesQueryResponse, List<TemplateSummaryResponseEntity>>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetTemplatesQueryResponse()
        );
    }

    /// <summary>
    /// Get template with fields
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns>Template with fields</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Get template with fields",
        Description = "Get template and fields by TemplateId"
    )]
    public async Task<GetTemplateWithFieldsQueryResponse> GetTemplateWithFields([FromQuery] Guid templateId)
    {
        var request = new GetTemplateWithFieldsQuery { TemplateId = templateId };
        return await ApiControllerHelper.HandleRequest<GetTemplateWithFieldsQuery, GetTemplateWithFieldsQueryResponse, TemplateWithFieldsResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetTemplateWithFieldsQueryResponse()
        );
    }

    /// <summary>
    /// Update template
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Updated template details</returns>
    [HttpPut("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Update template",
        Description = "Update template information. Only provided fields will be updated."
    )]
    public async Task<UpdateTemplateCommandResponse> UpdateTemplate([FromBody] UpdateTemplateCommand request)
    {
        return await ApiControllerHelper.HandleRequest<UpdateTemplateCommand, UpdateTemplateCommandResponse, UpdateTemplateResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new UpdateTemplateCommandResponse()
        );
    }

    /// <summary>
    /// Delete template (soft delete)
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns>Deleted template ID</returns>
    [HttpDelete("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Delete template (soft delete)",
        Description = "Soft delete template by setting IsActive = false."
    )]
    public async Task<DeleteTemplateCommandResponse> DeleteTemplate([FromQuery] Guid templateId)
    {
        var request = new DeleteTemplateCommand { TemplateId = templateId };
        return await ApiControllerHelper.HandleRequest<DeleteTemplateCommand, DeleteTemplateCommandResponse, DeleteTemplateResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new DeleteTemplateCommandResponse()
        );
    }

    /// <summary>
    /// Update template field
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Updated template field details</returns>
    [HttpPut("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Update template field",
        Description = "Update template field information. Only provided fields will be updated."
    )]
    public async Task<UpdateTemplateFieldCommandResponse> UpdateTemplateField([FromBody] UpdateTemplateFieldCommand request)
    {
        return await ApiControllerHelper.HandleRequest<UpdateTemplateFieldCommand, UpdateTemplateFieldCommandResponse, UpdateTemplateFieldResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new UpdateTemplateFieldCommandResponse()
        );
    }
}
