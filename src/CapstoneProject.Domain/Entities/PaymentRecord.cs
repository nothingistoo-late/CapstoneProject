using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Domain.Entities;

/// <summary>
/// Giao dịch thanh toán: mua gói hoặc mua game trả phí.
/// </summary>
public class PaymentRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? GameId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    /// <summary>Số tiền thật (VND) cho đơn nạp qua cổng thanh toán, null với giao dịch không dùng VND.</summary>
    public long? AmountVnd { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ExternalId { get; set; }

    public virtual Payment? Payment { get; set; }
    public virtual Package? Package { get; set; }
}
