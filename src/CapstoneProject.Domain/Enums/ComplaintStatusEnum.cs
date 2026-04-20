namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Trạng thái xử lý khiếu nại (Complaint Resolution).
/// Workflow: Open -> InProgress -> Resolved.
/// </summary>
public enum ComplaintStatusEnum
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    SellerPending = 3,
    FixInProgress = 4,
    FixSubmitted = 5,
    Verified = 6,
    SellerRejected = 7,
    SellerNoResponse = 8,
    ResolvedRefund = 9,
    ResolvedReject = 10,
    Closed = 11
}

