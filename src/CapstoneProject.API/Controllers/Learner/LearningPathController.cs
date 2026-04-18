using Microsoft.AspNetCore.Mvc;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Features.LearningPath.Commands.CompleteConcept;
using CapstoneProject.Application.Features.LearningPath.Commands.SelectLearningGoal;
using CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoals;
using CapstoneProject.Application.Features.LearningPath.Queries.GetConceptById;
using CapstoneProject.Application.Features.LearningPath.Queries.GetConceptCompletion;
using CapstoneProject.Application.Features.LearningPath.Queries.GetConcepts;
using CapstoneProject.Application.Features.LearningPath.Queries.GetLearningGoalById;
using CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPath;
using CapstoneProject.Application.Features.LearningPath.Queries.GetMyLearningPathProgress;
using CapstoneProject.Application.Features.LearningPath.Queries.GetPathItemsByGoal;
using CapstoneProject.Application.Features.LearningPath.Queries.GetSelectedGoal;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Controllers.Learner;

/// <summary>
/// API Lộ trình học (Learning Path): chọn mục tiêu, xem lộ trình concept + game, tiến độ, hoàn thành khái niệm.
/// </summary>
[ApiController]
[Route("api/learner/learning-path")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Learning Path")]
[SwaggerTag("Learning Path: goals, my path, progress, complete concept")]
public class LearningPathController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearningPathController(IMediator mediator) => _mediator = mediator;

    /// <summary>Danh sách mục tiêu học tập (Logic cơ bản, Điều kiện, Vòng lặp, Giải quyết vấn đề...). Dùng để hiển thị khi user chọn mục tiêu.</summary>
    [HttpGet("goals")]
    [ProducesResponseType(typeof(Result<List<LearningGoalDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Danh sách mục tiêu học tập", OperationId = "Learner_GetLearningGoals", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetLearningGoals()
    {
        var result = await _mediator.Send(new GetLearningGoalsQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chi tiết một mục tiêu học tập theo Id. Không yêu cầu đăng nhập.</summary>
    [HttpGet("goals/{goalId:guid}")]
    [ProducesResponseType(typeof(Result<LearningGoalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<LearningGoalDto>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Chi tiết mục tiêu học tập", OperationId = "Learner_GetLearningGoalById", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetLearningGoalById(Guid goalId)
    {
        var result = await _mediator.Send(new GetLearningGoalByIdQuery(goalId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Danh sách item trong lộ trình của một goal (chỉ cấu trúc, không có trạng thái user). Để xem trước "Lộ trình này gồm những gì". Không yêu cầu đăng nhập.</summary>
    [HttpGet("goals/{goalId:guid}/path-items")]
    [ProducesResponseType(typeof(Result<List<PathItemPreviewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<List<PathItemPreviewDto>>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Xem trước lộ trình theo goal", OperationId = "Learner_GetPathItemsByGoal", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetPathItemsByGoal(Guid goalId)
    {
        var result = await _mediator.Send(new GetPathItemsByGoalQuery(goalId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chọn mục tiêu học tập. Gọi sau khi đăng nhập hoặc khi user đổi mục tiêu. Yêu cầu đăng nhập.</summary>
    [HttpPost("goals/select")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Chọn mục tiêu học tập", OperationId = "Learner_SelectLearningGoal", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> SelectLearningGoal([FromBody] SelectLearningGoalRequest request)
    {
        var result = await _mediator.Send(new SelectLearningGoalCommand(request.LearningGoalId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Lấy lộ trình học của user: mục tiêu đã chọn + danh sách concept và game theo thứ tự, kèm trạng thái hoàn thành và mở khóa. Yêu cầu đăng nhập.</summary>
    [HttpGet("my-path")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MyLearningPathDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<MyLearningPathDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Lộ trình của tôi", OperationId = "Learner_GetMyLearningPath", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetMyLearningPath()
    {
        var result = await _mediator.Send(new GetMyLearningPathQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Mục tiêu học tập user đang chọn (id, name, description). Để hiển thị header/breadcrumb mà không cần gọi full my-path. Yêu cầu đăng nhập.</summary>
    [HttpGet("my-path/selected-goal")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<SelectedGoalDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SelectedGoalDto?>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Mục tiêu đang chọn", OperationId = "Learner_GetSelectedGoal", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetSelectedGoal()
    {
        var result = await _mediator.Send(new GetSelectedGoalQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tiến độ lộ trình: tổng số item, đã hoàn thành, % hoàn thành, gợi ý ôn tập (game còn yếu). Yêu cầu đăng nhập.</summary>
    [HttpGet("my-path/progress")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<LearningPathProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<LearningPathProgressDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Tiến độ lộ trình", OperationId = "Learner_GetMyLearningPathProgress", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetMyLearningPathProgress()
    {
        var result = await _mediator.Send(new GetMyLearningPathProgressQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Danh sách khái niệm (concept). Có thể lọc theo learningGoalId (query). Không yêu cầu đăng nhập.</summary>
    [HttpGet("concepts")]
    [ProducesResponseType(typeof(Result<List<ConceptDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Danh sách khái niệm", OperationId = "Learner_GetConcepts", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetConcepts([FromQuery] Guid? learningGoalId = null)
    {
        var result = await _mediator.Send(new GetConceptsQuery(learningGoalId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chi tiết một khái niệm theo Id. FE dùng ContentKey để load nội dung (file/bundle).</summary>
    [HttpGet("concepts/{conceptId:guid}")]
    [ProducesResponseType(typeof(Result<ConceptDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ConceptDetailDto>), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Chi tiết khái niệm", OperationId = "Learner_GetConceptById", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetConceptById(Guid conceptId)
    {
        var result = await _mediator.Send(new GetConceptByIdQuery(conceptId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Trạng thái hoàn thành concept của user hiện tại (IsCompleted, CompletedAt). FE dùng để hiển thị "Đã hoàn thành" trên trang chi tiết concept. Yêu cầu đăng nhập.</summary>
    [HttpGet("concepts/{conceptId:guid}/completion")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<ConceptCompletionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ConceptCompletionDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Trạng thái hoàn thành concept", OperationId = "Learner_GetConceptCompletion", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> GetConceptCompletion(Guid conceptId)
    {
        var result = await _mediator.Send(new GetConceptCompletionQuery(conceptId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đánh dấu đã hoàn thành một khái niệm (đọc xong / làm xong). Item tiếp theo trong lộ trình sẽ được mở khóa. Yêu cầu đăng nhập.</summary>
    [HttpPost("concepts/{conceptId:guid}/complete")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Hoàn thành khái niệm", OperationId = "Learner_CompleteConcept", Tags = new[] { "Learner - Learning Path" })]
    public async Task<IActionResult> CompleteConcept(Guid conceptId)
    {
        var result = await _mediator.Send(new CompleteConceptCommand(conceptId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class SelectLearningGoalRequest
{
    public Guid LearningGoalId { get; set; }
}
