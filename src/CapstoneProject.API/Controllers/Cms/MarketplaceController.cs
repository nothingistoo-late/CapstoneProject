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

    /// <summary>Get packages (paginated, filter).</summary>
    /// <remarks>
    /// Returns paginated packages. Filter by isActive, search. Admin only.
    ///
    /// **Query:** pageNumber (int, optional), pageSize (int, optional), isActive (bool?, optional), search (string, optional).
    ///
    /// **METHOD and path:** GET /api/cms/marketplace/packages
    ///
    /// **Example request:** GET /api/cms/marketplace/packages?pageNumber=1&amp;pageSize=20&amp;isActive=true
    /// </remarks>
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

    /// <summary>Get package by ID.</summary>
    /// <remarks>
    /// Returns a single package by Id. Admin only.
    ///
    /// **Route:** id (Guid, required): Package ID.
    ///
    /// **METHOD and path:** GET /api/cms/marketplace/packages/{id}
    ///
    /// **Example request:** GET /api/cms/marketplace/packages/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Create package.</summary>
    /// <remarks>
    /// Creates a new feature package. Returns package Id. Admin only.
    ///
    /// **Body (JSON):**
    /// - name (string, required): Package name.
    /// - durationDays (int, required): Duration in days.
    /// - limit (int?, optional): Usage limit if applicable.
    /// - price (decimal, required): Price.
    /// - featuresSpec (string, optional): JSON spec of features.
    ///
    /// **METHOD and path:** POST /api/cms/marketplace/packages
    ///
    /// **Example request body:** { "name": "Premium", "durationDays": 30, "limit": null, "price": 9.99, "featuresSpec": "{}" }
    /// </remarks>
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

    /// <summary>Update package.</summary>
    /// <remarks>
    /// Updates package by Id. All body fields optional. Admin only.
    ///
    /// **Route:** id (Guid, required): Package ID.
    ///
    /// **Body (JSON):** name (string?, optional), durationDays (int?, optional), limit (int?, optional), price (decimal?, optional), featuresSpec (string?, optional), isActive (bool?, optional).
    ///
    /// **METHOD and path:** PUT /api/cms/marketplace/packages/{id}
    ///
    /// **Example request body:** { "name": "Premium Plus", "price": 14.99, "isActive": true }
    /// </remarks>
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

    /// <summary>Delete package (soft delete).</summary>
    /// <remarks>
    /// Soft-deletes a package by Id. Admin only.
    ///
    /// **Route:** id (Guid, required): Package ID.
    ///
    /// **Body:** None.
    ///
    /// **METHOD and path:** DELETE /api/cms/marketplace/packages/{id}
    ///
    /// **Example request:** DELETE /api/cms/marketplace/packages/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// </remarks>
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

    /// <summary>Batch update package status.</summary>
    /// <remarks>
    /// Sets isActive for multiple packages. Returns successCount, failedCount, notFoundIds. Admin only.
    ///
    /// **Body (JSON):**
    /// - packageIds (array of Guid, required): Package IDs.
    /// - isActive (bool, required): New active status.
    ///
    /// **METHOD and path:** POST /api/cms/marketplace/packages/batch/status
    ///
    /// **Example request body:** { "packageIds": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ], "isActive": false }
    /// </remarks>
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

    /// <summary>Payment report (by date range, groupBy).</summary>
    /// <remarks>
    /// Returns payment report. Query: from, to (date), groupBy (Day|Month|Year). Includes both OrbitCoin amount and VND amount fields. Admin only.
    ///
    /// **Query:**
    /// - from (DateTime?, optional): Start date.
    /// - to (DateTime?, optional): End date.
    /// - groupBy (string, optional): "Day", "Month", or "Year". Default "Day".
    ///
    /// **METHOD and path:** GET /api/cms/marketplace/reports/payments
    ///
    /// **Example request:** GET /api/cms/marketplace/reports/payments?from=2025-01-01&amp;to=2025-03-01&amp;groupBy=Month
    /// </remarks>
    [HttpGet("reports/payments")]
    [AuthorizeRoles(nameof(RoleEnum.Admin))]
    [ProducesResponseType(typeof(Result<PaymentReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Payment report", Description = "Returns payment report. Query: from, to (date), groupBy (Day|Month|Year). Response includes totalAmount/amount (OrbitCoin) and totalAmountVnd/amountVnd (VND). Admin only.", OperationId = "Cms_GetPaymentReport", Tags = new[] { "CMS - Marketplace" })]
    public async Task<IActionResult> GetPaymentReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? groupBy = "Day")
    {
        var result = await _mediator.Send(new GetPaymentReportQuery(from, to, groupBy));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
