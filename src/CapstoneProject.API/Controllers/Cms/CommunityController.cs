using CapstoneProject.Application.Commons.DTOs.Community;
using CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;
using CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;
using CapstoneProject.Application.Features.Community.Commands.DismissReport;
using CapstoneProject.Application.Features.Community.Commands.ResolveReport;
using CapstoneProject.Application.Features.Community.Queries.GetReports;
using ReportListItemDto = CapstoneProject.Application.Features.Community.Queries.GetReports.ReportListItemDto;
using ReportStatusFilter = CapstoneProject.Application.Features.Community.Queries.GetReports.ReportStatusFilter;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/community")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Community")]
[SwaggerTag("CMS - Reports list, resolve, dismiss")]
public class CmsCommunityController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsCommunityController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get list of reports (paginated, filter by status, game, user, date).</summary>
    /// <remarks>
    /// Returns paginated reports. Filter by status, gameId, userId, date range. Admin/Moderator only.
    ///
    /// **Query:**
    /// - status (ReportStatusFilter?, optional): All, Pending, Reviewed, Resolved, Dismissed.
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - gameId (Guid?, optional): Filter by game ID.
    /// - userId (Guid?, optional): Filter by reporter user ID.
    /// - dateFrom (DateTime?, optional): From date.
    /// - dateTo (DateTime?, optional): To date.
    ///
    /// **METHOD and path:** GET /api/cms/community/reports
    ///
    /// **Example request:** GET /api/cms/community/reports?status=Pending&amp;pageNumber=1&amp;pageSize=20
    /// </remarks>
    [HttpGet("reports")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<ReportListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Danh sách báo cáo", Description = "Trả về danh sách báo cáo có phân trang. Query: status (Pending/Reviewed/Resolved/Dismissed), gameId, userId, dateFrom, dateTo, pageNumber, pageSize. Admin/Moderator only.", OperationId = "Cms_GetReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> GetReports([FromQuery] ReportStatusFilter? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? gameId = null, [FromQuery] Guid? userId = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new GetReportsQuery(status, pageNumber, pageSize, gameId, userId, dateFrom, dateTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Mark report as resolved.</summary>
    /// <remarks>
    /// Marks a report as Resolved. Optional query: reviewNote. Admin/Moderator only.
    ///
    /// **Route:** reportId (Guid, required): Report ID.
    ///
    /// **Query:** reviewNote (string, optional): Moderator note.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/community/reports/{reportId}/resolve
    ///
    /// **Example request:** POST /api/cms/community/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6/resolve?reviewNote=Resolved
    /// </remarks>
    [HttpPost("reports/{reportId:guid}/resolve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Resolve report", Description = "Marks a report as Resolved. Optional query: reviewNote. Admin/Moderator only.", OperationId = "Cms_ResolveReport", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> ResolveReport(Guid reportId, [FromQuery] string? reviewNote = null)
    {
        var result = await _mediator.Send(new ResolveReportCommand(reportId, reviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Dismiss report (e.g. not valid).</summary>
    /// <remarks>
    /// Marks a report as Dismissed. Optional query: reviewNote. Admin/Moderator only.
    ///
    /// **Route:** reportId (Guid, required): Report ID.
    ///
    /// **Query:** reviewNote (string, optional): Moderator note.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** POST /api/cms/community/reports/{reportId}/dismiss
    ///
    /// **Example request:** POST /api/cms/community/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6/dismiss
    /// </remarks>
    [HttpPost("reports/{reportId:guid}/dismiss")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Dismiss report", Description = "Marks a report as Dismissed (e.g. not valid). Optional query: reviewNote. Admin/Moderator only.", OperationId = "Cms_DismissReport", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> DismissReport(Guid reportId, [FromQuery] string? reviewNote = null)
    {
        var result = await _mediator.Send(new DismissReportCommand(reportId, reviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch resolve reports.</summary>
    /// <remarks>
    /// Marks multiple reports as resolved. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - reportIds (array of Guid, required): Report IDs to resolve.
    /// - reviewNote (string, optional): Common review note.
    ///
    /// **METHOD and path:** POST /api/cms/community/reports/batch/resolve
    ///
    /// **Example request body:** { "reportIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "reviewNote": "Resolved" }
    /// </remarks>
    [HttpPost("reports/batch/resolve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchReportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch resolve reports", Description = "Marks multiple reports as resolved. Body: reportIds, optional reviewNote. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.", OperationId = "Cms_BatchResolveReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> BatchResolveReports([FromBody] BatchReportsRequest request)
    {
        var result = await _mediator.Send(new BatchResolveReportsCommand(request.ReportIds, request.ReviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch dismiss reports.</summary>
    /// <remarks>
    /// Dismisses multiple reports. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.
    ///
    /// **Body (JSON):**
    /// - reportIds (array of Guid, required): Report IDs to dismiss.
    /// - reviewNote (string, optional): Common review note.
    ///
    /// **METHOD and path:** POST /api/cms/community/reports/batch/dismiss
    ///
    /// **Example request body:** { "reportIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "reviewNote": "Not valid" }
    /// </remarks>
    [HttpPost("reports/batch/dismiss")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchReportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch dismiss reports", Description = "Dismisses multiple reports. Body: reportIds, optional reviewNote. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.", OperationId = "Cms_BatchDismissReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> BatchDismissReports([FromBody] BatchReportsRequest request)
    {
        var result = await _mediator.Send(new BatchDismissReportsCommand(request.ReportIds, request.ReviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
