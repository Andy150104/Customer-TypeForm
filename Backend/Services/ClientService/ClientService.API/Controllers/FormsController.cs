using BaseService.API.BaseControllers;
using BaseService.Application.Interfaces.IdentityHepers;
using ClientService.Application.Forms.Commands.CreateField;
using ClientService.Application.Forms.Commands.CreateForm;
using ClientService.Application.Forms.Commands.CreateMultipleField;
using ClientService.Application.Forms.Commands.CreateOrUpdateLogic;
using ClientService.Application.Forms.Commands.SubmitForm;
using ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;
using ClientService.Application.Forms.Queries.GetFieldsByFormId;
using ClientService.Application.Forms.Queries.GetFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetForms;
using ClientService.Application.Forms.Queries.GetPublishedFormWithFieldsAndLogic;
using ClientService.Application.Forms.Queries.GetSubmissions;
using ClientService.Application.Forms.Queries.GetSubmissionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using OpenIddict.Validation.AspNetCore;
using Swashbuckle.AspNetCore.Annotations;
using LogicResponseEntity = ClientService.Application.Forms.Commands.CreateOrUpdateLogic.LogicResponseEntity;
using UpdateFormPublishedStatusResponseEntity = ClientService.Application.Forms.Commands.UpdateFormPublishedStatus.UpdateFormPublishedStatusResponseEntity;
using SubmitFormResponseEntity = ClientService.Application.Forms.Commands.SubmitForm.SubmitFormResponseEntity;

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

    /// <summary>
    /// Create a new field for a form
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Field ID and details</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Tạo field mới",
        Description = "Tạo field mới cho form. Order sẽ tự động tăng dựa trên field tạo trước đó."
    )]
    public async Task<CreateFieldCommandResponse> CreateField([FromBody] CreateFieldCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateFieldCommand, CreateFieldCommandResponse, CreateFieldResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateFieldCommandResponse()
        );
    }

    /// <summary>
    /// Create multiple fields for a form at once
    /// </summary>
    /// <param name="request"></param>
    /// <returns>List of created fields</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Tạo nhiều field cùng lúc",
        Description = "Tạo nhiều field cho form trong một request. Order sẽ tự động tăng dựa trên field tạo trước đó."
    )]
    public async Task<CreateMultipleFieldCommandResponse> CreateMultipleField([FromBody] CreateMultipleFieldCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateMultipleFieldCommand, CreateMultipleFieldCommandResponse, List<CreateFieldResponseEntity>>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateMultipleFieldCommandResponse()
        );
    }

    /// <summary>
    /// Get all fields by form ID
    /// </summary>
    /// <param name="formId"></param>
    /// <returns>List of fields ordered by Order</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Lấy danh sách fields theo FormId",
        Description = "Lấy tất cả fields của form, sắp xếp theo thứ tự Order (câu nào tạo trước sẽ có Order nhỏ hơn)"
    )]
    public async Task<GetFieldsByFormIdQueryResponse> GetFieldsByFormId([FromQuery] Guid formId)
    {
        var request = new GetFieldsByFormIdQuery { FormId = formId };
        return await ApiControllerHelper.HandleRequest<GetFieldsByFormIdQuery, GetFieldsByFormIdQueryResponse, List<FieldResponseEntity>>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetFieldsByFormIdQueryResponse()
        );
    }

    /// <summary>
    /// Get all forms for the current user
    /// </summary>
    /// <returns>List of forms ordered by CreatedAt descending</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Lấy danh sách tất cả forms của user hiện tại",
        Description = "Lấy tất cả forms của user đang đăng nhập, sắp xếp theo thời gian tạo (mới nhất trước)"
    )]
    public async Task<GetFormsQueryResponse> GetForms()
    {
        var request = new GetFormsQuery();
        return await ApiControllerHelper.HandleRequest<GetFormsQuery, GetFormsQueryResponse, List<FormResponseEntity>>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetFormsQueryResponse()
        );
    }

    /// <summary>
    /// Get form with fields and logic rules
    /// </summary>
    /// <param name="formId"></param>
    /// <returns>Form with fields and logic rules, including default next field (based on Order)</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Lấy form kèm fields và logic rules",
        Description = "Lấy form với tất cả fields và logic rules. Mỗi field có DefaultNextFieldId (field tiếp theo theo Order mặc định)"
    )]
    public async Task<GetFormWithFieldsAndLogicQueryResponse> GetFormWithFieldsAndLogic([FromQuery] Guid formId)
    {
        var request = new GetFormWithFieldsAndLogicQuery { FormId = formId };
        return await ApiControllerHelper.HandleRequest<GetFormWithFieldsAndLogicQuery, GetFormWithFieldsAndLogicQueryResponse, FormWithFieldsAndLogicResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetFormWithFieldsAndLogicQueryResponse()
        );
    }

    /// <summary>
    /// Get published form with fields and logic rules (public endpoint)
    /// </summary>
    /// <param name="formId"></param>
    /// <returns>Published form with fields and logic rules, including default next field (based on Order)</returns>
    [HttpGet("[action]")]
    [SwaggerOperation(
        Summary = "Lấy form đã published kèm fields và logic rules",
        Description = "Lấy form đã published (IsPublished = true) với tất cả fields và logic rules. Endpoint này không cần authentication, ai cũng có thể xem form đã published."
    )]
    public async Task<GetPublishedFormWithFieldsAndLogicQueryResponse> GetPublishedFormWithFieldsAndLogic([FromQuery] Guid formId)
    {
        var request = new GetPublishedFormWithFieldsAndLogicQuery { FormId = formId };
        return await ApiControllerHelper.HandleRequest<GetPublishedFormWithFieldsAndLogicQuery, GetPublishedFormWithFieldsAndLogicQueryResponse, FormWithFieldsAndLogicResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            new GetPublishedFormWithFieldsAndLogicQueryResponse()
        );
    }

    /// <summary>
    /// Create or update logic rule
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Logic rule details</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Tạo hoặc cập nhật logic rule",
        Description = "Nếu logic với cùng FieldId, Condition, và Value đã tồn tại thì update, không thì tạo mới"
    )]
    public async Task<CreateOrUpdateLogicCommandResponse> CreateOrUpdateLogic([FromBody] CreateOrUpdateLogicCommand request)
    {
        return await ApiControllerHelper.HandleRequest<CreateOrUpdateLogicCommand, CreateOrUpdateLogicCommandResponse, LogicResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new CreateOrUpdateLogicCommandResponse()
        );
    }

    /// <summary>
    /// Update form published status
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Updated form status</returns>
    [HttpPost("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Cập nhật trạng thái publish của form",
        Description = "Nếu IsPublished = true, sẽ kiểm tra form có title và ít nhất 1 field. Nếu false thì update bình thường."
    )]
    public async Task<UpdateFormPublishedStatusCommandResponse> UpdateFormPublishedStatus([FromBody] UpdateFormPublishedStatusCommand request)
    {
        return await ApiControllerHelper.HandleRequest<UpdateFormPublishedStatusCommand, UpdateFormPublishedStatusCommandResponse, UpdateFormPublishedStatusResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new UpdateFormPublishedStatusCommandResponse()
        );
    }

    /// <summary>
    /// Submit a form (public endpoint, no authentication required)
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Submission ID</returns>
    [HttpPost("[action]")]
    [SwaggerOperation(
        Summary = "Submit form",
        Description = "Submit form với answers. Form phải được published. Endpoint này không cần authentication."
    )]
    public async Task<SubmitFormCommandResponse> SubmitForm([FromBody] SubmitFormCommand request)
    {
        return await ApiControllerHelper.HandleRequest<SubmitFormCommand, SubmitFormCommandResponse, SubmitFormResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            new SubmitFormCommandResponse()
        );
    }

    /// <summary>
    /// Get all submissions for a form
    /// </summary>
    /// <param name="formId"></param>
    /// <returns>List of submissions with answers</returns>
    [HttpGet("[action]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [SwaggerOperation(
        Summary = "Lấy danh sách submissions của form",
        Description = "Lấy tất cả submissions (câu trả lời) của form, bao gồm answers. Chỉ owner của form mới xem được."
    )]
    public async Task<GetSubmissionsQueryResponse> GetSubmissions([FromQuery] Guid formId)
    {
        var request = new GetSubmissionsQuery { FormId = formId };
        return await ApiControllerHelper.HandleRequest<GetSubmissionsQuery, GetSubmissionsQueryResponse, List<SubmissionResponseEntity>>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            _identityService,
            _identityEntity,
            _httpContextAccessor,
            new GetSubmissionsQueryResponse()
        );
    }

    /// <summary>
    /// Get submission by ID (public endpoint for submitter to view their submission)
    /// </summary>
    /// <param name="submissionId"></param>
    /// <returns>Submission details with answers</returns>
    [HttpGet("[action]")]
    [SwaggerOperation(
        Summary = "Xem lại submission đã submit",
        Description = "Người submit form có thể xem lại submission của họ bằng submissionId. Endpoint này không cần authentication."
    )]
    public async Task<GetSubmissionByIdQueryResponse> GetSubmissionById([FromQuery] Guid submissionId)
    {
        var request = new GetSubmissionByIdQuery { SubmissionId = submissionId };
        return await ApiControllerHelper.HandleRequest<GetSubmissionByIdQuery, GetSubmissionByIdQueryResponse, SubmissionDetailResponseEntity>(
            request,
            _logger,
            ModelState,
            async () => await _mediator.Send(request),
            new GetSubmissionByIdQueryResponse()
        );
    }
}
