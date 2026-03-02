using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.API.Hubs;

/// <summary>
/// SignalR Hub cho chế độ thi đấu: phòng 2-8 người, nộp bài, xếp hạng real-time.
/// </summary>
[Authorize]
public class CompetitiveHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompetitiveHub> _logger;

    public CompetitiveHub(ICurrentUserService currentUserService, ILogger<CompetitiveHub> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnConnectedAsync();
    }

    /// <summary>Tham gia phòng thi đấu (client gọi sau khi có RoomCode từ API JoinRoom).</summary>
    public async Task JoinRoom(string roomCode)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId)) return;
        var groupName = $"Room_{roomCode}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined room {RoomCode}", userId, roomCode);
        await Clients.Group(groupName).SendAsync("UserJoinedRoom", new { UserId = userId, ConnectionId = Context.ConnectionId });
    }

    /// <summary>Rời phòng.</summary>
    public async Task LeaveRoom(string roomCode)
    {
        var groupName = $"Room_{roomCode}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var userId = _currentUserService.UserId;
        await Clients.Group(groupName).SendAsync("UserLeftRoom", new { UserId = userId });
    }

    /// <summary>Nộp giải pháp trong phòng (server sẽ đánh giá và broadcast ranking).</summary>
    public async Task SubmitSolution(string roomCode, string astSpec, string? bytecodeSpec)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId)) return;
        var groupName = $"Room_{roomCode}";
        await Clients.Group(groupName).SendAsync("SolutionSubmitted", new
        {
            UserId = userId,
            AstSpec = astSpec,
            BytecodeSpec = bytecodeSpec,
            At = DateTime.UtcNow
        });
    }

    /// <summary>Server gửi bảng xếp hạng (gọi từ API sau khi chấm xong).</summary>
    public async Task BroadcastRanking(string roomCode, object ranking)
    {
        var groupName = $"Room_{roomCode}";
        await Clients.Group(groupName).SendAsync("RankingUpdated", ranking);
    }
}
