using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ValidateSolutionRequest
{
    public Guid MapId { get; set; }
    /// <summary>Level đang nộp (MapDetails.Id). Null = map chỉ có 1 level (tự resolve).</summary>
    public Guid? MapDetailId { get; set; }
    public string Language { get; set; } = "Blockly";
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }

    /// <summary>Chế độ chơi dùng để ghi history.</summary>
    public PlayModeEnum PlayMode { get; set; } = PlayModeEnum.Single;

    /// <summary>Lobby room id (in-memory) nếu gọi từ lobby.</summary>
    public Guid? RoomId { get; set; }

    /// <summary>Match id (nếu có) để lưu trace phiên chơi.</summary>
    public Guid? MatchId { get; set; }

    /// <summary>Kết quả chạy engine (client): thắng/thua — dùng để chấm điểm lobby thay vì đo độ dài AST.</summary>
    public bool? IsWin { get; set; }

    /// <summary>Số bước thực tế từ engine (client).</summary>
    public int? ClientStepsUsed { get; set; }

    /// <summary>Số block đã dùng (client).</summary>
    public int? ClientBlocksUsed { get; set; }

    /// <summary>Thời gian chơi (giây), client.</summary>
    public double? ClientElapsedSeconds { get; set; }
}
