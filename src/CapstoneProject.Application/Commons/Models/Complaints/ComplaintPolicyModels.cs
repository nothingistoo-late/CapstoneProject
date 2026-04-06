namespace CapstoneProject.Application.Commons.Models.Complaints;

public class ComplaintCreateContextInput
{
    public Guid? PaymentRecordId { get; set; }
    public Guid? MapId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? SubmissionId { get; set; }
    public Guid? PlayHistoryId { get; set; }
    public Guid? XpTransactionId { get; set; }
    public Guid? OrbitCoinTransactionId { get; set; }
    public DateTime? OccurredAt { get; set; }
}

public class ComplaintCreatePolicyInput
{
    public Guid UserId { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplaintCreateContextInput? Context { get; set; }
}

public class ComplaintCreatePolicyResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string CategoryDisplayName { get; set; } = string.Empty;
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextKey { get; set; }
    public DateTime? OccurredAt { get; set; }
    public string? NormalizedContextJson { get; set; }
}
