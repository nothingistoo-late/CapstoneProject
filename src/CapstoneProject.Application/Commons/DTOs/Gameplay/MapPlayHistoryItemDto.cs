using System.Text.Json.Serialization;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

/// <summary>
/// Một dòng lịch sử chơi map (từ UserMapPlayHistories).
/// </summary>
public class MapPlayHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid MapId { get; set; }
    /// <summary>Tiêu đề map tại thời điểm truy vấn (null nếu map đã xóa).</summary>
    public string? MapTitle { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayModeEnum PlayMode { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public int? Score { get; set; }
    public int? Stars { get; set; }
    public Guid? SubmissionId { get; set; }
    public Guid? ExecutionsResultId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? MatchId { get; set; }
    public string? Language { get; set; }
}
