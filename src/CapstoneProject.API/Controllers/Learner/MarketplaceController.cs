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

    /// <summary>Lấy danh sách gói tính năng (phân trang, filter isActive, search).</summary>
    [HttpGet("packages")]
    [ProducesResponseType(typeof(Result<PaginationResult<PackageDto>>), StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Lấy danh sách gói tính năng", Description = "Trả về danh sách gói tính năng có phân trang. Query: pageNumber, pageSize, isActive, search. Learner cần đăng nhập để mua gói.", OperationId = "Learner_GetPackages", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> GetPackages([FromQuery] PackageFilter? filter = null)
    {
        var result = await _mediator.Send(new GetPackagesQuery(filter));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xem chi tiết một gói tính năng theo ID.</summary>
    [HttpGet("packages/{id:guid}")]
    [ProducesResponseType(typeof(Result<PackageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Xem chi tiết gói theo ID", Description = "Trả về thông tin chi tiết một gói (tên, giá, thời hạn, tính năng). Dùng trước khi gọi Purchase.", OperationId = "Learner_GetPackageById", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> GetPackageById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    [HttpPost("packages/{id:guid}/purchase")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Purchase package", Description = "Purchases a feature package for the current user.", OperationId = "Learner_PurchasePackage", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchasePackage(Guid id, [FromQuery] Guid? paymentMethodId = null)
    {
        var result = await _mediator.Send(new PurchasePackageCommand(id, paymentMethodId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Mua map thử thách trả phí (chỉ map có price &gt; 0).</summary>
    [HttpPost("maps/{mapId:guid}/purchase")]
    [AuthorizeRoles(nameof(RoleEnum.Learner))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Mua map trả phí", Description = "Mua map thử thách trả phí theo mapId. Chỉ áp dụng map có giá > 0. Query tùy chọn: paymentMethodId. Yêu cầu Bearer token.", OperationId = "Learner_PurchaseMap", Tags = new[] { "Learner - Marketplace" })]
    public async Task<IActionResult> PurchaseMap(Guid mapId, [FromQuery] Guid? paymentMethodId = null)
    {
        var result = await _mediator.Send(new PurchaseMapCommand(mapId, paymentMethodId));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
