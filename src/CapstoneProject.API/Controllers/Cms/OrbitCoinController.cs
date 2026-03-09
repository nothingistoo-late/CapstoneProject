using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Features.OrbitCoin.Commands.CreditOrbitCoin;

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
}
