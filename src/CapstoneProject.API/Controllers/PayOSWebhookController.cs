using System.Text;
using CapstoneProject.Application.Features.OrbitCoin.Commands.HandlePayOSWebhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneProject.API.Controllers;

/// <summary>
/// PayOS webhook endpoint. Called by PayOS when user completes payment. No auth.
/// For local dev: PayOS cannot call localhost — use ngrok and register https://your-ngrok-url/api/webhooks/payos in PayOS dashboard.
/// </summary>
[ApiController]
[Route("api/webhooks/payos")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
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
    /// PayOS payment webhook. PayOS calls this when payment succeeds. Do not call from client.
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var webhookJson = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(webhookJson))
        {
            _logger.LogWarning("PayOS webhook: empty body.");
            return BadRequest();
        }

        _logger.LogInformation("PayOS webhook: received body length={Length}", webhookJson.Length);
        var success = await _mediator.Send(new HandlePayOSWebhookCommand(webhookJson), cancellationToken);
        if (!success)
            _logger.LogWarning("PayOS webhook: processing returned false (verify failed or credit failed).");
        return success ? Ok() : BadRequest();
    }
}
