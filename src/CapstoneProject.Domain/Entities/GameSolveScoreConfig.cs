using CapstoneProject.Domain.Common;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Cấu hình điểm khi thắng game (engine metrics): BaseScore + Time/Steps/Blocks khi đạt tiêu chí; tổng 4 phần = 100.
/// </summary>
public class GameSolveScoreConfig : BaseEntity
{
    public const string DefaultConfigKey = "Default";

    /// <summary>Khóa cấu hình (unique), ví dụ <see cref="DefaultConfigKey"/>.</summary>
    public string ConfigKey { get; set; } = DefaultConfigKey;

    public int BaseScore { get; set; }
    public int TimeScore { get; set; }
    public int StepsScore { get; set; }
    public int BlocksScore { get; set; }
}
