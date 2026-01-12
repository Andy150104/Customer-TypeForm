using BaseService.API.BaseControllers;
using BaseService.Application.Interfaces.IdentityHepers;
using ClientService.Application.Forms.Commands.CreateForm;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ClientService.API.Controllers.Forms;

[ApiController]
[Route("api/v1/[controller]")]
public class FormsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private IdentityEntity _identityEntity = null!;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormsController(
        IMediator mediator,
        IIdentityService identityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _identityService = identityService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Create a new form
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Form ID and details</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Tạo form mới",
        Description = "Tạo form mới với is_published = false"
    )]
    public async Task<CreateFormCommandResponse> CreateForm([FromBody] CreateFormCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateFormCommand, CreateFormCommandResponse, CreateFormResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateFormCommandResponse()
        );
    }
}
