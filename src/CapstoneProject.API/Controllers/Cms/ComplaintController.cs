using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;
using CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;
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
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<ComplaintListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Complaints list", Description = "Paginated list of complaints. Filters: status, userId, dateFrom, dateTo, keyword.", OperationId = "Cms_GetComplaints", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> GetComplaints([FromQuery] CapstoneProject.Domain.Enums.ComplaintStatusEnum? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? userId = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] string? keyword = null)
    {
        var result = await _mediator.Send(new GetComplaintsQuery(status, pageNumber, pageSize, userId, dateFrom, dateTo, keyword));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Get complaint detail (messages + status history).</summary>
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

    /// <summary>Change complaint status (Open -> InProgress -> Resolved).</summary>
    [HttpPost("{complaintId:guid}/status")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Change complaint status", OperationId = "Cms_ChangeComplaintStatus", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> ChangeStatus(Guid complaintId, [FromBody] ChangeComplaintStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeComplaintStatusCommand(complaintId, request.ToStatus, request.Note));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Send staff message (reply or internal note).</summary>
    [HttpPost("{complaintId:guid}/messages")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Send staff complaint message", OperationId = "Cms_SendComplaintMessage", Tags = new[] { "CMS - Complaints" })]
    public async Task<IActionResult> SendMessage(Guid complaintId, [FromBody] SendComplaintMessageAsStaffRequest request)
    {
        var result = await _mediator.Send(new SendComplaintMessageAsStaffCommand(complaintId, request.Content, request.IsInternal));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

