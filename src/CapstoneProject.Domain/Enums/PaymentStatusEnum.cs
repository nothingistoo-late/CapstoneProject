namespace CapstoneProject.Domain.Enums;

/// <summary>
/// Trạng thái giao dịch thanh toán.
/// </summary>
public enum PaymentStatusEnum
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3,
    Cancelled = 4
}
