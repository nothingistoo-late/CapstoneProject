namespace CapstoneProject.Application.Commons.DTOs.OrbitCoin;

public class CreateDepositOrderResult
{
    /// <summary>
    /// Internal payment order identifier.
    /// </summary>
    public Guid OrderId { get; set; }
    /// <summary>
    /// Converted payment amount in VND for this deposit order.
    /// </summary>
    public long AmountVnd { get; set; }
    /// <summary>
    /// PayOS checkout URL to complete payment.
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;
}
