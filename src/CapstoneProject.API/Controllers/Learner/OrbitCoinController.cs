using CapstoneProject.Application.Features.OrbitCoin.Commands.ConfirmDeposit;
using CapstoneProject.Application.Features.OrbitCoin.Commands.CreateDepositOrder;
using CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinBalance;
using CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinTransactionHistory;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/orbitcoin")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - OrbitCoin")]
[SwaggerTag("OrbitCoin: balance, transaction history. Use marketplace APIs to purchase map/package with OrbitCoin.")]
public class LearnerOrbitCoinController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerOrbitCoinController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get current user OrbitCoin balance
    /// </summary>
    /// <remarks>
    /// Returns the authenticated user's OrbitCoin (virtual currency) balance. Requires Bearer token.
    /// </remarks>
    /// <response code="200">Returns balance (decimal).</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("balance")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<OrbitCoinBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<OrbitCoinBalanceDto>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get OrbitCoin balance", Description = "Returns current user's OrbitCoin balance in data.balance.", OperationId = "Learner_GetOrbitCoinBalance", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> GetBalance()
    {
        var result = await _mediator.Send(new GetOrbitCoinBalanceQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get OrbitCoin transaction history
    /// </summary>
    /// <remarks>
    /// Returns paginated transaction history for the current user (credits and debits). Requires Bearer token.
    /// Query: pageNumber (default 1), pageSize (default 20).
    /// </remarks>
    /// <response code="200">Returns items, totalCount, pageNumber, pageSize.</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("transactions")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<OrbitCoinTransactionHistoryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<OrbitCoinTransactionHistoryResult>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Get transaction history", Description = "Returns paginated OrbitCoin transaction history.", OperationId = "Learner_GetOrbitCoinTransactionHistory", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetOrbitCoinTransactionHistoryQuery(pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create deposit order and get PayOS checkout URL (user top-up OrbitCoin)
    /// </summary>
    /// <remarks>
    /// Creates a pending deposit order and returns a PayOS checkout URL. User opens the URL to pay with PayOS; on success, webhook credits OrbitCoin. Body: amountOrbitCoin (decimal, required).
    /// </remarks>
    /// <response code="200">Returns orderId and checkoutUrl. Redirect user to checkoutUrl.</response>
    /// <response code="400">Invalid amount.</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("deposit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(Summary = "Create deposit order (PayOS)", Description = "Creates deposit order and returns PayOS checkout URL. User pays there; webhook credits OrbitCoin.", OperationId = "Learner_CreateDepositOrder", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> CreateDepositOrder([FromBody] CreateDepositOrderRequest request)
    {
        var result = await _mediator.Send(new CreateDepositOrderCommand(request.AmountOrbitCoin));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Confirm deposit after redirect from PayOS (when webhook is not used or delayed).
    /// Call this when user lands on the success/return URL with orderId in query. Backend checks PayOS and credits OrbitCoin if paid.
    /// </summary>
    /// <param name="orderId">Order ID from return URL query (e.g. ?orderId=xxx).</param>
    [HttpPost("deposit/confirm")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Confirm deposit (after PayOS redirect)", Description = "Call when user returns from PayOS success page. Verifies payment with PayOS and credits OrbitCoin.", OperationId = "Learner_ConfirmDeposit", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> ConfirmDeposit([FromQuery] Guid orderId)
    {
        var result = await _mediator.Send(new ConfirmDepositCommand(orderId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}

public class CreateDepositOrderRequest
{
    public decimal AmountOrbitCoin { get; set; }
}
