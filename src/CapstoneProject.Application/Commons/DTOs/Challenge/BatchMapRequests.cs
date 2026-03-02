namespace CapstoneProject.Application.Commons.DTOs.Challenge;

public class BatchApproveMapsRequest
{
    public List<Guid> MapIds { get; set; } = new();
    public string? ReviewNote { get; set; }
}

public class BatchRejectMapsRequest
{
    public List<Guid> MapIds { get; set; } = new();
    public string? RejectReason { get; set; }
}

public class BatchPublishMapsRequest
{
    public List<Guid> MapIds { get; set; } = new();
}
