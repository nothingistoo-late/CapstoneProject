namespace CapstoneProject.Application.Commons.DTOs.Games;

public class BatchApproveMapsRequest
{
    public List<Guid> GameIds { get; set; } = new();
    public string? ReviewNote { get; set; }
}

public class BatchRejectMapsRequest
{
    public List<Guid> GameIds { get; set; } = new();
    public string? RejectReason { get; set; }
}

public class BatchPublishMapsRequest
{
    public List<Guid> GameIds { get; set; } = new();
}
