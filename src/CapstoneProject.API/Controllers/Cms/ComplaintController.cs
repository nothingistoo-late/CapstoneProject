using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;
using CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintCategoryConfig;
using CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintPolicyRuleConfig;
using CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;
using CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintCategoryConfig;
using CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintPolicyRuleConfig;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintPolicyRuleConfigs;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/complaints")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Complaints")]
[SwaggerTag("CMS - Complaints list, status updates, staff responses")]
public class CmsComplaintController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsComplaintController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get complaints list (paginated, filterable).</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/cms/complaints
    ///
    /// **Query:**
    /// - status, userId, dateFrom, dateTo, keyword, pageNumber, pageSize
    ///
    /// **Supported status values (v2 flow):**
    /// - Open
    /// - SellerPending
    /// - FixInProgress
    /// - FixSubmitted
    /// - Verified
    /// - SellerRejected
    /// - SellerNoResponse
    /// - ResolvedRefund
    /// - ResolvedReject
    /// - Closed
    ///
    /// Compatibility values from legacy flow (`InProgress`, `Resolved`) may still appear for old records.
    ///
    /// **Example request:** GET /api/cms/complaints?status=Open&amp;keyword=payment&amp;pageNumber=1&amp;pageSize=20
    ///
    /// **Example item data:**
    /// { "id": "...", "subject": "Paid but game locked", "categoryKey": "AccessIssue", "contextType": "PaymentRecord", "contextResolved": { "displayTitle": "Payment record", "displaySubtitle": "50000 VND" } }
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<ComplaintListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Complaints list", Description = "Paginated list of complaints. Filters: status, userId, dateFrom, dateTo, keyword.", OperationId = "Cms_GetComplaints", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> GetComplaints([FromQuery] CapstoneProject.Domain.Enums.ComplaintStatusEnum? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] string? keyword = null, [FromQuery] string? statusGroup = null)
    {
        var result = await _mediator.Send(new GetComplaintsQuery(status, pageNumber, pageSize, userId, dateFrom, dateTo, keyword, statusGroup));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get complaint detail (messages + status history).</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/cms/complaints/{complaintId}
    ///
    /// Returns full complaint detail including context raw fields and resolved context summary for investigation.
    ///
    /// **Example response keys:**
    /// - data.categoryKey
    /// - data.contextType / contextId / contextKey / contextDataJson / occurredAt
    /// - data.contextResolved.displayTitle / displaySubtitle / referenceCode / eventTime
    /// - data.messages[] (includes internal notes)
    /// - data.statusHistories[]
    /// </remarks>
    [HttpGet("{complaintId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ComplaintDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Complaint detail", OperationId = "Cms_GetComplaintDetail", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> GetComplaintDetail(Guid complaintId)
    {
        var result = await _mediator.Send(new GetComplaintDetailQuery(complaintId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Change complaint status (v2 complaint workflow).</summary>
    /// <remarks>
    /// **METHOD and path:** POST /api/cms/complaints/{complaintId}/status
    ///
    /// **Body (JSON):**
    /// - toStatus (required): target status in v2 flow
    /// - note (optional)
    /// - issueRefund (optional, bool): used when resolving with refund
    ///
    /// **V2 statuses:**
    /// - Open
    /// - SellerPending
    /// - FixInProgress
    /// - FixSubmitted
    /// - Verified
    /// - SellerRejected
    /// - SellerNoResponse
    /// - ResolvedRefund
    /// - ResolvedReject
    /// - Closed
    ///
    /// **Moderator/Admin typical transitions:**
    /// - Open -> SellerPending | SellerNoResponse
    /// - SellerPending -> FixInProgress | SellerRejected | SellerNoResponse
    /// - FixInProgress -> FixSubmitted | SellerNoResponse
    /// - FixSubmitted -> Verified
    /// - Verified -> ResolvedRefund | ResolvedReject
    /// - SellerRejected -> ResolvedRefund | ResolvedReject
    /// - SellerNoResponse -> ResolvedRefund | ResolvedReject
    /// - ResolvedRefund -> Closed
    /// - ResolvedReject -> Closed
    ///
    /// **Compatibility normalization:**
    /// - InProgress is normalized to SellerPending
    /// - Resolved + issueRefund=true => ResolvedRefund
    /// - Resolved + issueRefund=false => ResolvedReject
    ///
    /// **Example request body:**
    /// { "toStatus": "SellerPending", "note": "Requested seller response", "issueRefund": false }
    ///
    /// Returns `Result&lt;ComplaintStatusUpdateDto&gt;` with new status and context summary.
    /// </remarks>
    [HttpPost("{complaintId:guid}/status")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ComplaintStatusUpdateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ComplaintStatusUpdateDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ComplaintStatusUpdateDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ComplaintStatusUpdateDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ComplaintStatusUpdateDto>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Change complaint status (v2 workflow)",
        Description = "Update complaint status using v2 states: Open, SellerPending, FixInProgress, FixSubmitted, Verified, SellerRejected, SellerNoResponse, ResolvedRefund, ResolvedReject, Closed.",
        OperationId = "Cms_ChangeComplaintStatus",
        Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> ChangeStatus(Guid complaintId, [FromBody] ChangeComplaintStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeComplaintStatusCommand(complaintId, request.ToStatus, request.Note, request.IssueRefund));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Send staff message (reply or internal note).</summary>
    /// <remarks>
    /// **METHOD and path:** POST /api/cms/complaints/{complaintId}/messages
    ///
    /// **Body (multipart/form-data):**
    /// - content (required)
    /// - isInternal (true: internal note, false: visible reply)
    /// - attachments (files, optional): PNG/JPG/GIF/WEBP, max 5 files, max 5MB/file
    ///
    /// **Example form fields:**
    /// - content=Please provide transaction screenshot.
    /// - isInternal=false
    /// - attachments=(binary file, optional)
    /// </remarks>
    [HttpPost("{complaintId:guid}/messages")]
    [Consumes("multipart/form-data")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send staff complaint message", OperationId = "Cms_SendComplaintMessage", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> SendMessage(Guid complaintId, [FromForm] SendComplaintMessageAsStaffRequest request)
    {
        var result = await _mediator.Send(new SendComplaintMessageAsStaffCommand(complaintId, request.Content, request.IsInternal, request.Attachments));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>List complaint category configs.</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/cms/complaints/config/categories
    ///
    /// Returns category catalog used by complaint creation policy.
    /// </remarks>
    [HttpGet("config/categories")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<ComplaintCategoryConfigDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get complaint category configs", OperationId = "Cms_GetComplaintCategoryConfigs", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> GetCategoryConfigs()
    {
        var result = await _mediator.Send(new GetComplaintCategoryConfigsQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Create or update complaint category config.</summary>
    /// <remarks>
    /// **METHOD and path:** PUT /api/cms/complaints/config/categories/{categoryKey}
    ///
    /// **Body (JSON):**
    /// - displayName, description, isEnabled, sortOrder
    ///
    /// **Example request body:** { "displayName": "Access Issue", "description": "Purchased item cannot be accessed", "isEnabled": true, "sortOrder": 20 }
    ///
    /// Returns saved normalized row as `Result&lt;UpsertComplaintCategoryConfigDto&gt;`.
    /// </remarks>
    [HttpPut("config/categories/{categoryKey}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<UpsertComplaintCategoryConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UpsertComplaintCategoryConfigDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<UpsertComplaintCategoryConfigDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UpsertComplaintCategoryConfigDto>), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Upsert complaint category config", OperationId = "Cms_UpsertComplaintCategoryConfig", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> UpsertCategoryConfig(string categoryKey, [FromBody] UpdateComplaintCategoryConfigRequest request)
    {
        var result = await _mediator.Send(new UpsertComplaintCategoryConfigCommand(
            categoryKey,
            request.DisplayName,
            request.Description,
            request.IsEnabled,
            request.SortOrder));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Delete complaint category config (soft delete).</summary>
    [HttpDelete("config/categories/{categoryKey}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete complaint category config", OperationId = "Cms_DeleteComplaintCategoryConfig", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> DeleteCategoryConfig(string categoryKey)
    {
        var result = await _mediator.Send(new DeleteComplaintCategoryConfigCommand(categoryKey));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>List complaint policy rule configs.</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/cms/complaints/config/rules?categoryKey=AccessIssue
    ///
    /// Returns policy rules (required_context, time_window, duplicate_window, rate_limit...) used at create time.
    /// </remarks>
    [HttpGet("config/rules")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<ComplaintPolicyRuleConfigDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get complaint policy rule configs", OperationId = "Cms_GetComplaintPolicyRuleConfigs", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> GetRuleConfigs([FromQuery] string? categoryKey = null)
    {
        var result = await _mediator.Send(new GetComplaintPolicyRuleConfigsQuery(categoryKey));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Create or update complaint policy rule config.</summary>
    /// <remarks>
    /// **METHOD and path:** PUT /api/cms/complaints/config/rules/{categoryKey}/{ruleKey}
    ///
    /// **Body (JSON):**
    /// - isEnabled, priority, configJson, activeFrom, activeTo
    ///
    /// **Example request body:** { "isEnabled": true, "priority": 20, "configJson": "{\"hours\":72}", "activeFrom": null, "activeTo": null }
    ///
    /// Returns saved normalized row as `Result&lt;UpsertComplaintPolicyRuleConfigDto&gt;`.
    /// </remarks>
    [HttpPut("config/rules/{categoryKey}/{ruleKey}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<UpsertComplaintPolicyRuleConfigDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UpsertComplaintPolicyRuleConfigDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<UpsertComplaintPolicyRuleConfigDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UpsertComplaintPolicyRuleConfigDto>), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Upsert complaint policy rule config", OperationId = "Cms_UpsertComplaintPolicyRuleConfig", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> UpsertRuleConfig(string categoryKey, string ruleKey, [FromBody] UpdateComplaintPolicyRuleConfigRequest request)
    {
        var result = await _mediator.Send(new UpsertComplaintPolicyRuleConfigCommand(
            categoryKey,
            ruleKey,
            request.IsEnabled,
            request.Priority,
            request.ConfigJson,
            request.ActiveFrom,
            request.ActiveTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Delete complaint policy rule config (soft delete).</summary>
    [HttpDelete("config/rules/{categoryKey}/{ruleKey}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete complaint policy rule config", OperationId = "Cms_DeleteComplaintPolicyRuleConfig", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> DeleteRuleConfig(string categoryKey, string ruleKey)
    {
        var result = await _mediator.Send(new DeleteComplaintPolicyRuleConfigCommand(categoryKey, ruleKey));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

