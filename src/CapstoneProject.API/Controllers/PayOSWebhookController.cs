using System.Text;
using CapstoneProject.Application.Features.OrbitCoin.Commands.HandlePayOSWebhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CapstoneProject.API.Controllers;

/// <summary>
/// PayOS webhook endpoint. Called by PayOS when user completes payment. No auth.
/// For local dev: PayOS cannot call localhost — use ngrok and register https://your-ngrok-url/api/webhooks/payos in PayOS dashboard.
/// </summary>
[ApiController]
[Route("api/webhooks/payos")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
[SwaggerTag("Webhook: PayOS gọi khi thanh toán thành công. Không dùng từ client.")]
public class PayOSWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PayOSWebhookController> _logger;

    public PayOSWebhookController(IMediator mediator, ILogger<PayOSWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// PayOS payment webhook
    /// </summary>
    /// <remarks>
    /// PayOS gọi endpoint này khi thanh toán thành công (POST body JSON từ PayOS). Server xác thực chữ ký, cập nhật trạng thái order và cộng OrbitCoin cho user. Không gọi từ client; không cần Authorization.
    ///
    /// **METHOD and path:** POST /api/webhooks/payos
    ///
    /// **Body:** application/json — payload từ PayOS (data, signature...).
    ///
    /// **Response:** always 200 OK (ACK). Invalid payloads are ignored and logged.
    /// </remarks>
    /// <response code="200">Webhook acknowledged. Valid payloads are processed, invalid payloads are ignored.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "PayOS webhook", Description = "Called by PayOS when payment succeeds. Verifies signature and credits OrbitCoin. Do not call from client.", OperationId = "PayOS_Webhook", Tags = new[] { "Webhooks" })]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var webhookJson = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(webhookJson))
        {
            _logger.LogWarning("PayOS webhook: empty body.");
            return Ok(); // ACK to avoid PayOS endpoint health-check failure.
        }

        _logger.LogInformation("PayOS webhook: received body length={Length}", webhookJson.Length);
        var success = await _mediator.Send(new HandlePayOSWebhookCommand(webhookJson), cancellationToken);
        if (!success)
            _logger.LogWarning("PayOS webhook: processing returned false (verify failed or credit failed).");
        return Ok(); // Always ACK webhook transport. Business handling is logged internally.
    }
}
