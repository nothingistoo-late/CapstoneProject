using CapstoneProject.Application.Commons.DTOs.Marketplace;
using CapstoneProject.Application.Features.Marketplace.Commands.BatchUpdatePackageStatus;
using CapstoneProject.Application.Features.Marketplace.Commands.CreatePackage;
using CapstoneProject.Application.Features.Marketplace.Commands.DeletePackage;
using CapstoneProject.Application.Features.Marketplace.Commands.UpdatePackage;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackageById;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPackages;
using CapstoneProject.Application.Features.Marketplace.Queries.GetPaymentReport;
using BatchUpdatePackageStatusResultDto = CapstoneProject.Application.Features.Marketplace.Commands.BatchUpdatePackageStatus.BatchUpdatePackageStatusResultDto;

namespace CapstoneProject.API.Controllers.Cms;

[ApiController]
[Route("api/cms/marketplace")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Marketplace")]
[SwaggerTag("CMS - CRUD packages, batch status, payment reports")]
public class CmsMarketplaceController : ControllerBase
{
    private readonly IMediator _mediator;

    public CmsMarketplaceController(IMediator mediator) => _mediator = mediator;

    /// <summary>Danh sách gói tính năng (phân trang, filter).</summary>
    [HttpGet("packages")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<PaginationResult<PackageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get packages", Description = "Returns paginated packages. Filter by isActive, search. Admin only.", OperationId = "Cms_GetPackages", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> GetPackages([FromQuery] PackageFilter? filter = null)
    {
        var result = await _mediator.Send(new GetPackagesQuery(filter));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Chi tiết gói theo ID.</summary>
    [HttpGet("packages/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<PackageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get package by ID", Description = "Returns a single package by Id. Admin only.", OperationId = "Cms_GetPackageById", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> GetPackageById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo gói tính năng mới.</summary>
    [HttpPost("packages")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create package", Description = "Creates a new feature package. Body: name, durationDays, limit?, price, featuresSpec?. Returns package Id. Admin only.", OperationId = "Cms_CreatePackage", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
    {
        var result = await _mediator.Send(new CreatePackageCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Cập nhật gói tính năng.</summary>
    [HttpPut("packages/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update package", Description = "Updates package by Id. Body: name?, durationDays?, limit?, price?, featuresSpec?, isActive?. Admin only.", OperationId = "Cms_UpdatePackage", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> UpdatePackage(Guid id, [FromBody] UpdatePackageRequest request)
    {
        var result = await _mediator.Send(new UpdatePackageCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa gói (soft delete).</summary>
    [HttpDelete("packages/{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete package", Description = "Soft-deletes a package by Id. Admin only.", OperationId = "Cms_DeletePackage", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> DeletePackage(Guid id)
    {
        var result = await _mediator.Send(new DeletePackageCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Bật/tắt trạng thái nhiều gói cùng lúc.</summary>
    [HttpPost("packages/batch/status")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<BatchUpdatePackageStatusResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch update package status", Description = "Sets isActive for multiple packages. Body: packageIds, isActive. Returns successCount, failedCount, notFoundIds. Admin only.", OperationId = "Cms_BatchUpdatePackageStatus", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> BatchUpdatePackageStatus([FromBody] BatchUpdatePackageStatusRequest request)
    {
        var result = await _mediator.Send(new BatchUpdatePackageStatusCommand(request.PackageIds, request.IsActive));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Báo cáo thanh toán (theo khoảng thời gian, groupBy).</summary>
    [HttpGet("reports/payments")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<PaymentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Payment report", Description = "Returns payment report. Query: from, to (date), groupBy (Day|Month|Year). Admin only.", OperationId = "Cms_GetPaymentReport", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> GetPaymentReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? groupBy = "Day")
    {
        var result = await _mediator.Send(new GetPaymentReportQuery(from, to, groupBy));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
