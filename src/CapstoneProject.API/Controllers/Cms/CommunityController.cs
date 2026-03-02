using CapstoneProject.Application.Features.Community.Commands.BatchDismissReports;
using CapstoneProject.Application.Features.Community.Commands.BatchResolveReports;
using CapstoneProject.Application.Features.Community.Commands.DismissReport;
using CapstoneProject.Application.Features.Community.Commands.ResolveReport;
using CapstoneProject.Application.Features.Community.Queries.GetReports;
using BatchReportResultDto = CapstoneProject.Application.Features.Community.Commands.BatchResolveReports.BatchReportResultDto;
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

    /// <summary>Danh sách báo cáo (phân trang, filter theo trạng thái, map, user, ngày).</summary>
    [HttpGet("reports")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<PaginationResult<ReportListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Danh sách báo cáo", Description = "Trả về danh sách báo cáo có phân trang. Query: status (Pending/Reviewed/Resolved/Dismissed), mapId, userId, dateFrom, dateTo, pageNumber, pageSize. Admin/Moderator only.", OperationId = "Cms_GetReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> GetReports([FromQuery] ReportStatusFilter? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? mapId = null, [FromQuery] Guid? userId = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new GetReportsQuery(status, pageNumber, pageSize, mapId, userId, dateFrom, dateTo));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đánh dấu báo cáo đã xử lý.</summary>
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

    /// <summary>Bỏ qua báo cáo (không hợp lệ).</summary>
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

    /// <summary>Xử lý nhiều báo cáo cùng lúc (resolve).</summary>
    [HttpPost("reports/batch/resolve")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchReportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch resolve reports", Description = "Marks multiple reports as resolved. Body: reportIds, optional reviewNote. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.", OperationId = "Cms_BatchResolveReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> BatchResolveReports([FromBody] CmsBatchReportsRequest request)
    {
        var result = await _mediator.Send(new BatchResolveReportsCommand(request.ReportIds, request.ReviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Bỏ qua nhiều báo cáo cùng lúc.</summary>
    [HttpPost("reports/batch/dismiss")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchReportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch dismiss reports", Description = "Dismisses multiple reports. Body: reportIds, optional reviewNote. Returns successCount, failedCount, notFoundIds. Admin/Moderator only.", OperationId = "Cms_BatchDismissReports", Tags = new[] { "CMS - Community" })]
    public async Task<IActionResult> BatchDismissReports([FromBody] CmsBatchReportsRequest request)
    {
        var result = await _mediator.Send(new BatchDismissReportsCommand(request.ReportIds, request.ReviewNote));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class CmsBatchReportsRequest
{
    public List<Guid> ReportIds { get; set; } = new();
    public string? ReviewNote { get; set; }
}
