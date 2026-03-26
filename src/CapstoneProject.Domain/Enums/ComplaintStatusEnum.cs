namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Trạng thái xử lý khiếu nại (Complaint Resolution).
/// Workflow: Open -> InProgress -> Resolved.
/// </summary>
public enum ComplaintStatusEnum
{
    Open = 0,
    InProgress = 1,
    Resolved = 2
}

