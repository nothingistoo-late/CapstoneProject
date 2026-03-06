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
    /// Admin only. Use when user has completed a top-up / deposit; credits their OrbitCoin wallet. Optional note and related payment record.
    /// </remarks>
    /// <response code="200">Credited successfully.</response>
    /// <response code="400">Invalid amount.</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Admin only</response>
    [HttpPost("credit")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Credit OrbitCoin (deposit)", Description = "Credit user's wallet when they deposit real money. Admin only.", OperationId = "Cms_CreditOrbitCoin", Tags = new[] { "CMS - OrbitCoin" })]
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

public class CreditOrbitCoinRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}
