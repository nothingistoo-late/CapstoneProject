using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Infrastructure.Configurations;

namespace CapstoneProject.Infrastructure.Services;

public class PayOSService : IPayOSService
{
    private readonly PayOSClient? _client;
    private readonly ILogger<PayOSService> _logger;

    public PayOSService(IOptions<PayOSSettings> options, ILogger<PayOSService> logger)
    {
        var s = options.Value;
        if (!string.IsNullOrWhiteSpace(s.ClientId) && !string.IsNullOrWhiteSpace(s.ApiKey) && !string.IsNullOrWhiteSpace(s.ChecksumKey))
            _client = new PayOSClient(new PayOSOptions
            {
                ClientId = s.ClientId,
                ApiKey = s.ApiKey,
                ChecksumKey = s.ChecksumKey
            });
        else
            _client = null;
        _logger = logger;
    }

    public async Task<(string? CheckoutUrl, string? Error)> CreatePaymentLinkAsync(
        long orderCode,
        long amountVnd,
        string description,
        string returnUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return (null, "PayOS is not configured (ClientId, ApiKey, ChecksumKey).");
        try
        {
            var request = new CreatePaymentLinkRequest
            {
                OrderCode = (int)(orderCode % int.MaxValue),
                Amount = (int)Math.Min(amountVnd, int.MaxValue),
                Description = description,
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };
            var response = await _client.PaymentRequests.CreateAsync(request);
            return (response?.CheckoutUrl, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<bool?> GetPaymentStatusByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return null;
        try
        {
            var link = await _client.PaymentRequests.GetAsync((int)(orderCode % int.MaxValue));
            if (link == null)
                return false;
            // PaymentLinkStatus: thường 1 = Paid (kiểm tra PayOS docs nếu khác)
            var status = link.Status;
            var isPaid = status == PaymentLinkStatus.Paid;
            return isPaid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS GetPaymentStatus orderCode={OrderCode}: {Message}", orderCode, ex.Message);
            return null;
        }
    }

    public async Task<PayOSWebhookVerifiedData?> VerifyWebhookAsync(string webhookJson, CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return null;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var webhook = JsonSerializer.Deserialize<Webhook>(webhookJson, options);
            if (webhook?.Data == null || !webhook.Success)
                return null;
            var verified = await _client.Webhooks.VerifyAsync(webhook);
            if (verified == null)
                return null;
            return new PayOSWebhookVerifiedData
            {
                OrderCode = verified.OrderCode,
                Amount = verified.Amount,
                Description = verified.Description,
                TransactionDateTime = verified.TransactionDateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS webhook verify failed: {Message}", ex.Message);
            return null;
        }
    }
}
