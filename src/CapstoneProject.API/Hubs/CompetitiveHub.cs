using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CapstoneProject.Application.Common.Interfaces;

namespace CapstoneProject.API.Hubs;

/// <summary>
/// SignalR Hub cho cháº¿ Ä‘á»™ thi Ä‘áº¥u: phÃ²ng 2-8 ngÆ°á»i, ná»™p bÃ i, xáº¿p háº¡ng real-time.
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

    /// <summary>Tham gia phÃ²ng thi Ä‘áº¥u (client gá»i sau khi cÃ³ RoomCode tá»« API JoinRoom).</summary>
    public async Task JoinRoom(string roomCode)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId)) return;
        var groupName = $"Room_{roomCode}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined room {RoomCode}", userId, roomCode);
        await Clients.Group(groupName).SendAsync("UserJoinedRoom", new { UserId = userId, ConnectionId = Context.ConnectionId });
    }

    /// <summary>Rá»i phÃ²ng.</summary>
    public async Task LeaveRoom(string roomCode)
    {
        var groupName = $"Room_{roomCode}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        var userId = _currentUserService.UserId;
        await Clients.Group(groupName).SendAsync("UserLeftRoom", new { UserId = userId });
    }

    /// <summary>Ná»™p giáº£i phÃ¡p trong phÃ²ng (server sáº½ Ä‘Ã¡nh giÃ¡ vÃ  broadcast ranking).</summary>
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
            At = CapstoneProject.Domain.Common.VietnamDateTime.Now
        });
    }

    /// <summary>Server gá»­i báº£ng xáº¿p háº¡ng (gá»i tá»« API sau khi cháº¥m xong).</summary>
    public async Task BroadcastRanking(string roomCode, object ranking)
    {
        var groupName = $"Room_{roomCode}";
        await Clients.Group(groupName).SendAsync("RankingUpdated", ranking);
    }
}

