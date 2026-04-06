using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;
using CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;
using CapstoneProject.Application.Features.Complaints.Queries.GetAvailableComplaintCategories;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;
using CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaintDetail;
using CapstoneProject.Application.Features.Complaints.Queries.GetMyComplaints;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/complaints")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Complaints")]
[SwaggerTag("Learner - Submit complaints, view history, send messages")]
public class LearnerComplaintController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerComplaintController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get available complaint categories for users.</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/learner/complaints/categories
    ///
    /// Returns only enabled categories that users can select when creating a complaint.
    /// </remarks>
    [HttpGet("categories")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<List<ComplaintCategoryConfigDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get available complaint categories", OperationId = "Learner_GetComplaintCategories", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> GetAvailableCategories()
    {
        var result = await _mediator.Send(new GetAvailableComplaintCategoriesQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Create a complaint.</summary>
    /// <remarks>
    /// Create a new complaint ticket. Requires Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/complaints
    ///
    /// **Body (multipart/form-data):**
    /// - subject (string, required)
    /// - categoryKey (string, required): runtime category key configured by CMS
    /// - description (string, required)
    /// - context.* (fields, required): contextual IDs for policy validation
    /// - attachments (files, optional): PNG/JPG/GIF/WEBP, max 5 files, max 5MB/file
    ///
    /// **Example form fields:**
    /// - subject=Paid but map locked
    /// - categoryKey=AccessIssue
    /// - description=I completed payment but still cannot open the map.
    /// - context.paymentRecordId=3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// - context.mapId=7c9e6679-7425-40de-944b-e07fc1f90ae7
    /// - context.occurredAt=2026-04-06T09:30:00
    /// - attachments=(binary file, optional)
    ///
    /// **Example success response data:**
    /// { "id": "...", "categoryKey": "AccessIssue", "contextType": "PaymentRecord", "contextId": "...", "contextKey": "AccessIssue:...:...", "contextDataJson": "{...}", "occurredAt": "2026-04-06T09:30:00", "contextResolved": { "displayTitle": "Payment record", "displaySubtitle": "50000 VND", "referenceCode": "PAY-123" } }
    /// </remarks>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CreateComplaintResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<CreateComplaintResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CreateComplaintResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<CreateComplaintResponseDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Create complaint", OperationId = "Learner_CreateComplaint", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> Create([FromForm] CreateComplaintRequest request)
    {
        var context = new CapstoneProject.Application.Commons.Models.Complaints.ComplaintCreateContextInput
        {
            PaymentRecordId = request.Context.PaymentRecordId,
            MapId = request.Context.MapId,
            PackageId = request.Context.PackageId,
            SubmissionId = request.Context.SubmissionId,
            PlayHistoryId = request.Context.PlayHistoryId,
            XpTransactionId = request.Context.XpTransactionId,
            OrbitCoinTransactionId = request.Context.OrbitCoinTransactionId,
            OccurredAt = request.Context.OccurredAt
        };

        var result = await _mediator.Send(new CreateComplaintCommand(request.Subject, request.CategoryKey, request.Description, context, request.Attachments));
        if (result.IsSuccess)
            return StatusCode(StatusCodes.Status201Created, result);

        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get my complaints (paginated).</summary>
    /// <remarks>
    /// Returns a paginated list of complaints created by the current user.
    ///
    /// **METHOD and path:** GET /api/learner/complaints
    ///
    /// **Query:**
    /// - status (ComplaintStatusEnum?, optional): Open/InProgress/Resolved
    /// - pageNumber, pageSize (optional)
    /// - dateFrom, dateTo (optional)
    ///
    /// **Example request:** GET /api/learner/complaints?status=Open&amp;pageNumber=1&amp;pageSize=20
    ///
    /// **Example item data:**
    /// { "id": "...", "subject": "Paid but map locked", "category": "Access Issue", "categoryKey": "AccessIssue", "complaintStatus": "Open", "contextType": "PaymentRecord", "contextResolved": { "displayTitle": "Payment record", "displaySubtitle": "50000 VND" } }
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<MyComplaintListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "My complaints list", OperationId = "Learner_GetMyComplaints", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> GetMyComplaints([FromQuery] CapstoneProject.Domain.Enums.ComplaintStatusEnum? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new GetMyComplaintsQuery(status, pageNumber, pageSize, dateFrom, dateTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get complaint detail (messages + status history).</summary>
    /// <remarks>
    /// **METHOD and path:** GET /api/learner/complaints/{complaintId}
    ///
    /// Returns complaint detail with full context fields and resolved context summary for UI.
    ///
    /// **Example response keys:**
    /// - data.contextType / data.contextId / data.contextKey / data.contextDataJson / data.occurredAt
    /// - data.contextResolved.displayTitle / displaySubtitle / referenceCode / eventTime
    /// - data.messages[] (internal messages are hidden for learner)
    /// - data.statusHistories[]
    /// </remarks>
    [HttpGet("{complaintId:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MyComplaintDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "My complaint detail", OperationId = "Learner_GetMyComplaintDetail", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> GetMyComplaintDetail(Guid complaintId)
    {
        var result = await _mediator.Send(new GetMyComplaintDetailQuery(complaintId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Send a message in a complaint thread.</summary>
    /// <remarks>
    /// **METHOD and path:** POST /api/learner/complaints/{complaintId}/messages
    ///
    /// **Body (multipart/form-data):**
    /// - content (string, required)
    /// - attachments (files, optional): PNG/JPG/GIF/WEBP, max 5 files, max 5MB/file
    ///
    /// **Example form fields:**
    /// - content=Please help check this payment again.
    /// - attachments=(binary file, optional)
    /// </remarks>
    [HttpPost("{complaintId:guid}/messages")]
    [Consumes("multipart/form-data")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<ComplaintMessagePostedDto>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send complaint message", OperationId = "Learner_SendComplaintMessage", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> SendMessage(Guid complaintId, [FromForm] SendComplaintMessageRequest request)
    {
        var result = await _mediator.Send(new SendComplaintMessageCommand(complaintId, request.Content, request.Attachments));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

