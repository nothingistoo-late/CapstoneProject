using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Lịch sử chơi map của user (mỗi lần validate/submit có thể tạo 1 record).
/// Mục tiêu: FE/BE có thể phân tích hành vi chơi, tiến độ theo phiên chơi.
/// </summary>
public class UserMapPlayHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MapId { get; set; }
    /// <summary>Level đang chơi (MapDetails.Id), optional.</summary>
    public Guid? MapDetailId { get; set; }

    /// <summary>Chế độ chơi: Single/Lobby/Competitive.</summary>
    public PlayModeEnum PlayMode { get; set; }

    /// <summary>Thời gian bắt đầu validate / chạy mô phỏng.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Thời gian kết thúc validate / chạy mô phỏng.</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// true: user nộp accepted (có kết quả win) và map được coi là "hoàn thành" theo logic hiện tại.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>Score của attempt.</summary>
    public int? Score { get; set; }

    /// <summary>Số sao của attempt.</summary>
    public int? Stars { get; set; }

    /// <summary>FK tham chiếu tới Submission (nếu có).</summary>
    public Guid? SubmissionId { get; set; }

    /// <summary>FK tham chiếu tới ExecutionsResult (nếu có).</summary>
    public Guid? ExecutionsResultId { get; set; }

    /// <summary>
    /// MatchId/RoomId (tùy mode) để trace nguồn game.
    /// Lobby dùng RoomId (in-memory) truyền xuống; competitive dùng MatchId (nếu triển khai submit server-side).
    /// </summary>
    public Guid? RoomId { get; set; }
    public Guid? MatchId { get; set; }

    public string? Language { get; set; }
}

