using CapstoneProject.Application.Features.Competitive.Commands.CreateMatch;
using CapstoneProject.Application.Features.Competitive.Commands.CreateRoom;
using CapstoneProject.Application.Features.Competitive.Commands.JoinRoom;
using CreateRoomResultDto = CapstoneProject.Application.Features.Competitive.Commands.CreateRoom.CreateRoomResultDto;
using JoinRoomResultDto = CapstoneProject.Application.Features.Competitive.Commands.JoinRoom.JoinRoomResultDto;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/competitive")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Competitive")]
[SwaggerTag("Learner - Match, Room, Join. Real-time via SignalR /hubs/competitive")]
public class LearnerCompetitiveController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerCompetitiveController(IMediator mediator) => _mediator = mediator;

    /// <summary>Tạo trận đấu (chọn map).</summary>
    [HttpPost("matches")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Create match", Description = "Creates a competitive match for a map. Body: mapId, optional rulesSpec. Returns matchId. Then create room and join via SignalR.", OperationId = "Learner_CreateMatch", Tags = new[] { "Learner - Competitive" })]
    public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var result = await _mediator.Send(new CreateMatchCommand(request.MapId, request.RulesSpec));
        if (result.IsSuccess)
            return Created(string.Empty, result);
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo phòng trong trận đấu (trả về roomCode để share).</summary>
    [HttpPost("matches/{matchId:guid}/rooms")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CreateRoomResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Create room", OperationId = "Learner_CreateRoom", Description = "Creates a room for the match. Query: maxPlayers (default 8). Returns roomCode for players to join. Use with SignalR hub /hubs/competitive.", Tags = new[] { "Learner - Competitive" })]
    public async Task<IActionResult> CreateRoom(Guid matchId, [FromQuery] int maxPlayers = 8)
    {
        var result = await _mediator.Send(new CreateRoomCommand(matchId, maxPlayers));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Vào phòng bằng roomCode (sau đó kết nối SignalR JoinRoom).</summary>
    [HttpPost("rooms/join")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<JoinRoomResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Join room by code", Description = "Joins a room by roomCode. Body: { roomCode }. Returns room info. Then connect to SignalR hub and call JoinRoom(roomCode), SubmitSolution when done.", OperationId = "Learner_JoinRoom", Tags = new[] { "Learner - Competitive" })]
    public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
    {
        var result = await _mediator.Send(new JoinRoomCommand(request.RoomCode));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class CreateMatchRequest
{
    public Guid MapId { get; set; }
    public string? RulesSpec { get; set; }
}

public class JoinRoomRequest
{
    public string RoomCode { get; set; } = string.Empty;
}
