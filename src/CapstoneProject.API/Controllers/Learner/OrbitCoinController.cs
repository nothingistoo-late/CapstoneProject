using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Features.OrbitCoin.Commands.ConfirmDeposit;
using CapstoneProject.Application.Features.OrbitCoin.Commands.CreateDepositOrder;
using CapstoneProject.Application.Features.OrbitCoin.Queries.GetDepositOrder;
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
    /// Trả về số dư OrbitCoin (tiền ảo) của user đang đăng nhập. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/orbitcoin/balance
    ///
    /// **Body:** None. Headers: Authorization Bearer &lt;token&gt;.
    /// </remarks>
    /// <response code="200">Returns message and data (balance).</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("balance")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<OrbitCoinBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<OrbitCoinBalanceDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<OrbitCoinBalanceDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get OrbitCoin balance", Description = "Returns current user's OrbitCoin balance in data.balance. Requires Bearer token.", OperationId = "Learner_GetOrbitCoinBalance", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> GetBalance()
    {
        var result = await _mediator.Send(new GetOrbitCoinBalanceQuery());
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get OrbitCoin transaction history
    /// </summary>
    /// <remarks>
    /// Trả về lịch sử giao dịch OrbitCoin (nạp/trừ) có phân trang. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** GET /api/learner/orbitcoin/transactions
    ///
    /// **Query:** pageNumber (int, optional, default 1), pageSize (int, optional, default 20).
    ///
    /// **Example request:** GET /api/learner/orbitcoin/transactions?pageNumber=1&amp;pageSize=20
    /// </remarks>
    /// <response code="200">Returns message and data (items, totalCount, pageNumber, pageSize). Each item includes amount (OrbitCoin) and amountVnd (real money if available).</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("transactions")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<OrbitCoinTransactionHistoryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<OrbitCoinTransactionHistoryResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<OrbitCoinTransactionHistoryResult>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get transaction history", Description = "Returns paginated OrbitCoin transaction history. Query: pageNumber, pageSize. Requires Bearer token.", OperationId = "Learner_GetOrbitCoinTransactionHistory", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetOrbitCoinTransactionHistoryQuery(pageNumber, pageSize));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Create deposit order and get PayOS checkout URL (user top-up OrbitCoin)
    /// </summary>
    /// <remarks>
    /// Tạo lệnh nạp tiền và trả về URL thanh toán PayOS. User mở URL để thanh toán; khi thành công, webhook sẽ cộng OrbitCoin. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/orbitcoin/deposit
    ///
    /// **Body (JSON):**
    /// - amountOrbitCoin (decimal, required): Số OrbitCoin muốn nạp (số dương).
    ///
    /// **Example request body:** { "amountOrbitCoin": 100 }
    /// </remarks>
    /// <response code="200">Returns message and data (orderId, amountVnd, checkoutUrl). Redirect user to checkoutUrl.</response>
    /// <response code="400">Invalid amount</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("deposit")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<CreateDepositOrderResult>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Create deposit order (PayOS)", Description = "Creates deposit order and returns PayOS checkout URL. Response data includes orderId, amountVnd, checkoutUrl. Body: amountOrbitCoin. User pays at URL; webhook credits OrbitCoin.", OperationId = "Learner_CreateDepositOrder", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> CreateDepositOrder([FromBody] CreateDepositOrderRequest request)
    {
        var result = await _mediator.Send(new CreateDepositOrderCommand(request.AmountOrbitCoin));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get deposit order detail (success page: DB times, amounts, PayOS code)
    /// </summary>
    /// <remarks>
    /// Trả về CreatedAt, PaidAt, số tiền, phương thức, mã PayOS. Chỉ đơn nạp OrbitCoin của user hiện tại.
    ///
    /// **METHOD and path:** GET /api/learner/orbitcoin/deposit/order?orderId={guid}
    /// </remarks>
    [HttpGet("deposit/order")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<DepositOrderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<DepositOrderDetailDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<DepositOrderDetailDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<DepositOrderDetailDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Get deposit order detail", Description = "Returns CreatedAt, PaidAt, amounts, payment method, PayOS external code. Query: orderId. Only OrbitCoin deposit orders for the current user.", OperationId = "Learner_GetDepositOrder", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> GetDepositOrder([FromQuery] Guid orderId)
    {
        var result = await _mediator.Send(new GetDepositOrderQuery(orderId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Confirm deposit after redirect from PayOS
    /// </summary>
    /// <remarks>
    /// Gọi khi user quay lại từ trang thành công PayOS (khi webhook chưa xử lý hoặc trễ). Backend kiểm tra PayOS và cộng OrbitCoin nếu đã thanh toán. Yêu cầu Bearer token.
    ///
    /// **METHOD and path:** POST /api/learner/orbitcoin/deposit/confirm
    ///
    /// **Query:** orderId (Guid, required): Order ID từ return URL (vd ?orderId=xxx).
    ///
    /// **Example request:** POST /api/learner/orbitcoin/deposit/confirm?orderId=3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
    /// <response code="200">Deposit confirmed and OrbitCoin credited.</response>
    /// <response code="400">Order invalid or already processed</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Order not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("deposit/confirm")]
    [AuthorizeRoles(nameof(RoleEnum.Learner), nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Confirm deposit (after PayOS redirect)", Description = "Call when user returns from PayOS success page. Query: orderId. Verifies payment and credits OrbitCoin.", OperationId = "Learner_ConfirmDeposit", Tags = new[] { "Learner - OrbitCoin" })]
    public async Task<IActionResult> ConfirmDeposit([FromQuery] Guid orderId)
    {
        var result = await _mediator.Send(new ConfirmDepositCommand(orderId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
