using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Features.OrbitCoin.Commands.CreditOrbitCoin;
using CapstoneProject.Application.Features.OrbitCoin.Commands.UpdateExchangeRate;
using CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRate;
using CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRateHistory;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/orbitcoin")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - OrbitCoin")]
[SwaggerTag("OrbitCoin: credit user when they deposit real money (admin only)")]
public class CmsOrbitCoinController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsOrbitCoinController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Credit OrbitCoin to user (e.g. after user deposits real money)
    /// </summary>
    /// <remarks>
    /// Chỉ Admin. Cộng OrbitCoin vào ví user (vd sau khi user nạp tiền thật). Có thể ghi chú và liên kết bản ghi thanh toán.
    ///
    /// **METHOD and path:** POST /api/cms/orbitcoin/credit
    ///
    /// **Body (JSON):**
    /// - userId (Guid, required): ID user cần cộng.
    /// - amount (decimal, required): Số OrbitCoin cộng (số dương).
    /// - note (string, optional): Ghi chú.
    /// - relatedEntityType (string, optional): Loại entity liên quan (vd "Deposit").
    /// - relatedEntityId (string?, optional): ID entity liên quan.
    ///
    /// **Example request body:** { "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "amount": 100, "note": "Manual top-up" }
    /// </remarks>
    /// <response code="200">Credited successfully. Returns message only.</response>
    /// <response code="400">Invalid amount or userId</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("credit")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Credit OrbitCoin (deposit)", Description = "Credit user's wallet when they deposit real money. Admin only. Body: userId, amount, note?, relatedEntityType?, relatedEntityId?.", OperationId = "Cms_CreditOrbitCoin", Tags = new[] { "CMS - OrbitCoin" })]
    public async Task<IActionResult> Credit([FromBody] CreditOrbitCoinRequest request)
    {
        var result = await _mediator.Send(new CreditOrbitCoinCommand(
            request.UserId,
            request.Amount,
            request.Note,
            request.RelatedEntityType,
            request.RelatedEntityId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get current exchange rate for OrbitCoin to VND
    /// </summary>
    /// <remarks>
    /// Get the active exchange rate (1 OrbitCoin = ? VND). Used by system to convert coin amounts to real currency for payment processing.
    ///
    /// **METHOD and path:** GET /api/cms/orbitcoin/exchange-rate
    ///
    /// **Query parameters:**
    /// - fromCurrency (string, optional): Source currency. Default: "OrbitCoin"
    /// - toCurrency (string, optional): Target currency. Default: "VND"
    ///
    /// **Example request:** GET /api/cms/orbitcoin/exchange-rate
    /// </remarks>
    /// <response code="200">Returns exchange rate</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Exchange rate not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("exchange-rate")]
    [ProducesResponseType(typeof(Result<ExchangeRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get exchange rate", Description = "Get current OrbitCoin/VND exchange rate. Used by payment system.", OperationId = "Cms_GetExchangeRate", Tags = new[] { "CMS - OrbitCoin" })]
    public async Task<IActionResult> GetExchangeRate(
        [FromQuery] string fromCurrency = "OrbitCoin",
        [FromQuery] string toCurrency = "VND")
    {
        var result = await _mediator.Send(new GetExchangeRateQuery(fromCurrency, toCurrency));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get exchange rate history for OrbitCoin to VND
    /// </summary>
    [HttpGet("exchange-rate/history")]
    [ProducesResponseType(typeof(Result<List<ExchangeRateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get exchange rate history", Description = "Get recent exchange rate changes for OrbitCoin/VND.", OperationId = "Cms_GetExchangeRateHistory", Tags = new[] { "CMS - OrbitCoin" })]
    public async Task<IActionResult> GetExchangeRateHistory(
        [FromQuery] string fromCurrency = "OrbitCoin",
        [FromQuery] string toCurrency = "VND",
        [FromQuery] int take = 20)
    {
        var result = await _mediator.Send(new GetExchangeRateHistoryQuery(fromCurrency, toCurrency, take));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Update exchange rate between OrbitCoin and VND (admin only)
    /// </summary>
    /// <remarks>
    /// Update the exchange rate for converting OrbitCoin to VND. Changes affect all subsequent payment calculations. Admin only.
    ///
    /// **METHOD and path:** PUT /api/cms/orbitcoin/exchange-rate
    ///
    /// **Body (JSON):**
    /// - rate (decimal, required): New exchange rate (1 OrbitCoin = rate VND). Must be positive.
    /// - reason (string, optional): Reason for the change (audit trail).
    /// - effectiveFrom (DateTime, optional): When this rate becomes effective (UTC). Default: now.
    /// - effectiveTo (DateTime, optional): When this rate expires (UTC). Null = no expiration.
    /// - fromCurrency (string, optional): Source currency. Default: "OrbitCoin"
    /// - toCurrency (string, optional): Target currency. Default: "VND"
    ///
    /// **Example request body:** { "rate": 1100, "reason": "Market adjustment based on demand" }
    /// </remarks>
    /// <response code="200">Exchange rate updated successfully</response>
    /// <response code="400">Invalid rate or request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin only</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("exchange-rate")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<ExchangeRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Update exchange rate", Description = "Update OrbitCoin/VND exchange rate. Admin only. Body: rate, reason?, effectiveFrom?, effectiveTo?.", OperationId = "Cms_UpdateExchangeRate", Tags = new[] { "CMS - OrbitCoin" })]
    public async Task<IActionResult> UpdateExchangeRate([FromBody] UpdateExchangeRateRequest request)
    {
        var command = new UpdateExchangeRateCommand(
            request.Rate,
            request.Reason,
            request.EffectiveFrom,
            request.EffectiveTo);
        var result = await _mediator.Send(command);
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
