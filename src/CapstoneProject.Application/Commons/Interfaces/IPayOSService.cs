namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// PayOS payment gateway: create payment link for user top-up, verify webhook.
/// </summary>
public interface IPayOSService
{
    /// <summary>
    /// Creates a PayOS payment link. OrderCode must be unique per request.
    /// </summary>
    /// <param name="orderCode">Unique order code (e.g. generated from DB or timestamp).</param>
    /// <param name="amountVnd">Amount in VND.</param>
    /// <param name="description">Description shown on payment.</param>
    /// <param name="returnUrl">URL to redirect after success.</param>
    /// <param name="cancelUrl">URL to redirect if user cancels.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checkout URL to redirect user, or (null, errorMessage) on failure.</returns>
    Task<(string? CheckoutUrl, string? Error)> CreatePaymentLinkAsync(
        long orderCode,
        long amountVnd,
        string description,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment status by order code from PayOS (for confirm-after-redirect flow).
    /// </summary>
    /// <param name="orderCode">Order code we sent when creating the payment link.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if payment is completed/paid, false if pending or not found, null if error.</returns>
    Task<bool?> GetPaymentStatusByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies PayOS webhook signature and returns payload data.
    /// </summary>
    /// <param name="webhookJson">Raw JSON body received from PayOS webhook.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verified data (orderCode, amount) if valid; null if invalid.</returns>
    Task<PayOSWebhookVerifiedData?> VerifyWebhookAsync(
        string webhookJson,
        CancellationToken cancellationToken = default);
}

public class PayOSWebhookVerifiedData
{
    public long OrderCode { get; set; }
    public long Amount { get; set; }
    public string? Description { get; set; }
    public string? TransactionDateTime { get; set; }
}
