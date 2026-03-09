namespace CapstoneProject.Application.Commons.DTOs.Lobby;

/// <summary>Chỉ cần gửi một trong hai: roomId hoặc roomCode (ví dụ join bằng code: chỉ gửi roomCode).</summary>
public class JoinLobbyRoomRequest
{
    /// <summary>Room ID (Guid). Optional if roomCode is provided.</summary>
    public Guid? RoomId { get; set; }
    /// <summary>Room code (e.g. AB12CD). Optional if roomId is provided.</summary>
    public string? RoomCode { get; set; }
}
