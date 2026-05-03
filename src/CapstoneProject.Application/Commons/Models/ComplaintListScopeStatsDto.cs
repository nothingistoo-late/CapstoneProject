using System.Text.Json.Serialization;

namespace CapstoneProject.Application.Common.Models;

/// <summary>Counts for the same keyword/date/user scope as the list, ignoring status/statusGroup filters (CMS dashboard).</summary>
public class ComplaintListScopeStatsDto
{
    [JsonPropertyName("totalInScope")]
    public int TotalInScope { get; set; }

    [JsonPropertyName("pendingInScope")]
    public int PendingInScope { get; set; }

    [JsonPropertyName("solvedInScope")]
    public int SolvedInScope { get; set; }
}
