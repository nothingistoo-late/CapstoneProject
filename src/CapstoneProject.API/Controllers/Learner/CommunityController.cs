using CapstoneProject.Application.Features.Community.Commands.RateMap;
using CapstoneProject.Application.Features.Community.Commands.ReportMap;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API cộng đồng dành cho Learner: đánh giá map, báo cáo nội dung.
/// </summary>
[ApiController]
[Route("api/learner/community")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Community")]
[SwaggerTag("Learner - Rate maps, report content")]
public class LearnerCommunityController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerCommunityController(IMediator mediator) => _mediator = mediator;

    /// <summary>Đánh giá map (1–5 sao) và gửi/nhận comment.</summary>
    [HttpPost("maps/{mapId:guid}/rate")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Đánh giá map (1–5 sao)", Description = "Gửi hoặc cập nhật đánh giá (rating 1–5) và comment tùy chọn cho map. Body: rating (bắt buộc), comment (tùy chọn). Yêu cầu Bearer token.", OperationId = "Learner_RateMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> RateMap(Guid mapId, [FromBody] RateMapRequest request)
    {
        var result = await _mediator.Send(new RateMapCommand(mapId, request.Rating, request.Comment));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Báo cáo map (nội dung không phù hợp).</summary>
    [HttpPost("maps/{mapId:guid}/report")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Báo cáo map", Description = "Gửi báo cáo nội dung không phù hợp cho map. Body: reason (bắt buộc), details (tùy chọn). Trả về reportId. Admin/Moderator xử lý tại CMS - Community. Yêu cầu Bearer token.", OperationId = "Learner_ReportMap", Tags = new[] { "Learner - Community" })]
    public async Task<IActionResult> ReportMap(Guid mapId, [FromBody] ReportMapRequest request)
    {
        var result = await _mediator.Send(new ReportMapCommand(mapId, request.Reason, request.Details));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class RateMapRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReportMapRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}
