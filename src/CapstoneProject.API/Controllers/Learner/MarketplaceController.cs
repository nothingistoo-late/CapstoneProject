using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Features.Marketplace.Commands.PurchaseMap;
using CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackageById;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackages;

namespace CapstoneProject.API.Controllers.Learner;

[ApiController]
[Route("api/learner/marketplace")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("Learner - Marketplace")]
[SwaggerTag("Learner - Browse packages, purchase package or paid map")]
public class LearnerMarketplaceController : ControllerBase
{
    private readonly IMediator _mediator;

    public LearnerMarketplaceController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get list of feature packages
    /// </summary>
    /// <remarks>
    /// Trả về danh sách gói tính năng có phân trang. Query: pageNumber, pageSize, isActive, search.
    ///
    ///     GET /api/learner/marketplace/packages
    ///     Query: pageNumber=1, pageSize=10, isActive, search
    /// </remarks>
    /// <response code="200">Returns message and data (paginated list of packages).</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("packages")]
    [ProducesResponseType(typeof(Result<PaginationResult<PackageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PaginationResult<PackageDto>>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Lấy danh sách gói tính năng", Description = "Returns paginated feature packages. Query: pageNumber, pageSize, isActive, search. Login required to purchase.", OperationId = "Learner_GetPackages", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> GetPackages([FromQuery] PackageFilter? filter = null)
    {
        var result = await _mediator.Send(new GetPackagesQuery(filter));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Get package detail by ID
    /// </summary>
    /// <remarks>
    /// Trả về thông tin chi tiết gói (tên, giá, thời hạn, tính năng). Dùng trước khi gọi Purchase.
    ///
    ///     GET /api/learner/marketplace/packages/{id}
    /// </remarks>
    /// <response code="200">Returns message and data (package detail).</response>
    /// <response code="404">Package not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("packages/{id:guid}")]
    [ProducesResponseType(typeof(Result<PackageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PackageDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<PackageDto>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Xem chi tiết gói theo ID", Description = "Returns package detail (name, price, duration, features). Use before Purchase. Requires Bearer token for purchase.", OperationId = "Learner_GetPackageById", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> GetPackageById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Purchase feature package
    /// </summary>
    /// <remarks>
    /// Mua gói tính năng cho user hiện tại. Yêu cầu Bearer token (Learner).
    ///
    ///     POST /api/learner/marketplace/packages/{id}/purchase
    ///     Query: paymentMethodId (optional)
    /// </remarks>
    /// <response code="200">Purchase successful. Returns message and data (order/purchase id).</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Not a Learner</response>
    /// <response code="404">Package not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("packages/{id:guid}/purchase")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Purchase package", Description = "Purchases a feature package for the current user. Requires Bearer token.", OperationId = "Learner_PurchasePackage", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchasePackage(Guid id, [FromQuery] Guid? paymentMethodId = null)
    {
        var result = await _mediator.Send(new PurchasePackageCommand(id, paymentMethodId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Purchase paid challenge map
    /// </summary>
    /// <remarks>
    /// Mua map thử thách trả phí (map có giá > 0). Yêu cầu Bearer token (Learner).
    ///
    ///     POST /api/learner/marketplace/maps/{mapId}/purchase
    ///     Query: paymentMethodId (optional)
    /// </remarks>
    /// <response code="200">Purchase successful. Returns message and data.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Not a Learner</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("maps/{mapId:guid}/purchase")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Mua map trả phí", Description = "Purchases paid challenge map by mapId. Only for maps with price > 0. Optional paymentMethodId. Requires Bearer token.", OperationId = "Learner_PurchaseMap", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchaseMap(Guid mapId, [FromQuery] Guid? paymentMethodId = null)
    {
        var result = await _mediator.Send(new PurchaseMapCommand(mapId, paymentMethodId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
