using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ValidateSolutionRequest
{
    public Guid MapId { get; set; }
    public string Language { get; set; } = "Blockly";
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }

    /// <summary>Chế độ chơi dùng để ghi history.</summary>
    public PlayModeEnum PlayMode { get; set; } = PlayModeEnum.Single;

    /// <summary>Lobby room id (in-memory) nếu gọi từ lobby.</summary>
    public Guid? RoomId { get; set; }

    /// <summary>Match id (nếu competitive có server-side submit).</summary>
    public Guid? MatchId { get; set; }
}
