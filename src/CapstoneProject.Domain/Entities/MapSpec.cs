using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Phiên bản spec thực thi của map (grid, điều kiện thắng/thua). Một Map có thể có nhiều version.
/// </summary>
public class MapSpec : BaseEntity
{
    public Guid MapId { get; set; }
    public string GridSpec { get; set; } = string.Empty;
    public string InitialStateSpec { get; set; } = string.Empty;
    public string WinConditionSpec { get; set; } = string.Empty;
    public string FailConditionSpec { get; set; } = string.Empty;
    public int Version { get; set; } = 1;

    public virtual Map Map { get; set; } = null!;
}
