using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;
using CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;
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

    /// <summary>Create a complaint.</summary>
    /// <remarks>
    /// Create a new complaint ticket. Requires Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/complaints
    ///
    /// **Body (JSON):**
    /// - subject (string, required)
    /// - category (string, required): e.g. "Technical", "LearningExperience", "LessonContent"
    /// - description (string, required)
    /// </remarks>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Create complaint", OperationId = "Learner_CreateComplaint", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest request)
    {
        var result = await _mediator.Send(new CreateComplaintCommand(request.Subject, request.Category, request.Description));
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
    [HttpPost("{complaintId:guid}/messages")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send complaint message", OperationId = "Learner_SendComplaintMessage", Tags = new[] { "Learner - Complaints" })]
    public async Task<IActionResult> SendMessage(Guid complaintId, [FromBody] SendComplaintMessageRequest request)
    {
        var result = await _mediator.Send(new SendComplaintMessageCommand(complaintId, request.Content));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

