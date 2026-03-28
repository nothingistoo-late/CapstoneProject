namespace CapstoneProject.Application.Commons.DTOs.OrbitCoin;

/// <summary>
/// Chi tiết đơn nạp OrbitCoin (PayOS) cho trang thành công / ví.
/// </summary>
public class DepositOrderDetailDto
{
    public Guid OrderId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal AmountOrbitCoin { get; set; }
    public long? AmountVnd { get; set; }
    /// <summary>Mã đơn PayOS (số int dạng string).</summary>
    public string? ExternalOrderCode { get; set; }
    public string PaymentMethodCode { get; set; } = "PayOS";
    public string PaymentMethodName { get; set; } = "PayOS";
}
