using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Features.Marketplace.Commands.PurchasePackage;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackageById;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackages;
using CapstoneProject.Application.Features.OrbitCoin.Commands.PurchaseMapWithOrbitCoin;

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
    /// Returns paginated list of feature packages. Use filters for active/inactive and search. Login required to purchase.
    ///
    /// **Query:**
    /// - pageNumber (int, optional): Page number. Default 1.
    /// - pageSize (int, optional): Items per page. Default 20.
    /// - isActive (bool?, optional): Filter by active status; null = all.
    /// - search (string, optional): Search in package name.
    ///
    /// **METHOD and path:** GET /api/learner/marketplace/packages
    ///
    /// **Example request:** GET /api/learner/marketplace/packages?pageNumber=1&amp;pageSize=10&amp;isActive=true&amp;search=premium
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
    /// Returns package detail (name, price, duration, features). Use before calling Purchase. Requires Bearer token for purchase.
    ///
    /// **Route:** id (Guid, required): Package ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** GET /api/learner/marketplace/packages/{id}
    ///
    /// **Example request:** GET /api/learner/marketplace/packages/3fa85f64-5717-4562-b3fc-2c963f66afa6
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
    /// Purchase feature package with OrbitCoin
    /// </summary>
    /// <remarks>
    /// Purchases a feature package for the current user by deducting OrbitCoin. User must have topped up OrbitCoin first. Requires Bearer token (Learner).
    ///
    /// **Route:** id (Guid, required): Package ID.
    ///
    /// **METHOD and path:** POST /api/learner/marketplace/packages/{id}/purchase
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
    [SwaggerOperation(Summary = "Purchase package (OrbitCoin)", Description = "Purchases a feature package by deducting OrbitCoin. User must top up first.", OperationId = "Learner_PurchasePackage", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchasePackage(Guid id)
    {
        var result = await _mediator.Send(new PurchasePackageCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>
    /// Purchase paid challenge map with OrbitCoin
    /// </summary>
    /// <remarks>
    /// Purchases a paid challenge map (price &gt; 0) by deducting OrbitCoin. User must have topped up OrbitCoin first. Requires Bearer token (Learner).
    ///
    /// **Route:** mapId (Guid, required): Challenge map ID.
    ///
    /// **METHOD and path:** POST /api/learner/marketplace/maps/{mapId}/purchase
    /// </remarks>
    /// <response code="200">Purchase successful.</response>
    /// <response code="401">Not authorized</response>
    /// <response code="403">Not a Learner</response>
    /// <response code="404">Map not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("maps/{mapId:guid}/purchase")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(Summary = "Mua map trả phí (OrbitCoin)", Description = "Purchases paid map by deducting OrbitCoin. User must top up first.", OperationId = "Learner_PurchaseMap", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchaseMap(Guid mapId)
    {
        var result = await _mediator.Send(new PurchaseMapWithOrbitCoinCommand(mapId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
