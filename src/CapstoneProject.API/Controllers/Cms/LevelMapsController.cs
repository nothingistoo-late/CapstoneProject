using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Extensions;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Maps.Commands.BatchCreateMaps;
using CapstoneProject.Application.Features.Maps.Commands.BatchDeleteMaps;
using CapstoneProject.Application.Features.Maps.Commands.BatchUpsertCatalog;
using CapstoneProject.Application.Features.Maps.Commands.CreateMaps;
using CapstoneProject.Application.Features.Maps.Commands.DeleteMaps;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMaps;
using CapstoneProject.Application.Features.Maps.Queries.GetMapsById;
using CapstoneProject.Application.Features.Maps.Queries.GetPagedMaps;
using CapstoneProject.Application.Commons.DTOs.Maps;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CapstoneProject.API.Controllers.Cms;

/// <summary>
/// CRUD và batch API cho Level Maps (lưu file JSON level/platform).
/// </summary>
[ApiController]
[Route("api/cms/level-maps")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Level Maps")]
[SwaggerTag("APIs to store and manage level/platform JSON files (id, name, layers, startPosition, goalPosition, metadata).")]
public class LevelMapsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LevelMapsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lấy danh sách level maps có phân trang.</summary>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(PaginationResult<MapsListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Get paged level maps", Description = "Returns paginated level maps with optional filter by search, externalId, status.", OperationId = "GetPagedLevelMaps", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> GetPaged([FromQuery] MapsFilter filter)
    {
        var result = await _mediator.Send(new GetPagedMapsQuery(filter));
        return Ok(result);
    }

    /// <summary>Lấy một level map theo Id.</summary>
    [HttpGet("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Get level map by ID", Description = "Returns full level map including JsonContent.", OperationId = "GetLevelMapById", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMapsByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo một level map từ nội dung JSON.</summary>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Create level map", Description = "Creates one level map. Send body as { \"level\": { \"id\", \"name\", \"width\", \"height\", \"layers\", \"startPosition\", \"goalPosition\", \"metadata\" } } to avoid newline escape issues; or use \"jsonContent\" (string) with escaped JSON.", OperationId = "CreateLevelMap", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Create([FromBody] CreateMapsRequest request)
    {
        var result = await _mediator.Send(new CreateMapsCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Cập nhật level map.</summary>
    [HttpPut("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Update level map", Description = "Updates name and/or full JsonContent.", OperationId = "UpdateLevelMap", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMapsRequest request)
    {
        var result = await _mediator.Send(new UpdateMapsCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa (soft delete) một level map.</summary>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Delete level map", Description = "Soft-deletes the level map.", OperationId = "DeleteLevelMap", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapsCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch tạo nhiều level maps từ danh sách JSON.</summary>
    [HttpPost("batch/create")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchCreateMapsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch create level maps", Description = "Creates multiple level maps. Send \"levels\": [{ level1 }, { level2 }, ...] to avoid newline escape; or \"jsonContents\": [\"...\", \"...\"]. Returns successCount, failedCount, createdIds and errors.", OperationId = "BatchCreateLevelMaps", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> BatchCreate([FromBody] BatchCreateMapsRequest request)
    {
        var result = await _mediator.Send(new BatchCreateMapsCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đồng bộ catalog từ FE: gửi { "levels": [{ "id", "file", "name", "type", "difficulty" }] }. Tạo mới hoặc cập nhật theo id (ExternalId).</summary>
    [HttpPost("batch/upsert-catalog")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchUpsertCatalogResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch upsert catalog", Description = "Sync catalog from FE. Body: { \"levels\": [{ \"id\", \"file\", \"name\", \"type\", \"difficulty\" }] }. Creates or updates by id. Does not overwrite JsonContent.", OperationId = "BatchUpsertLevelCatalog", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> BatchUpsertCatalog([FromBody] BatchUpsertCatalogRequest request)
    {
        var result = await _mediator.Send(new BatchUpsertCatalogCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch xóa (soft delete) nhiều level maps.</summary>
    [HttpPost("batch/delete")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchDeleteMapsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(Summary = "Batch delete level maps", Description = "Soft-deletes multiple level maps by Id list. Returns successCount, notFoundCount, notFoundIds.", OperationId = "BatchDeleteLevelMaps", Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteMapsRequest request)
    {
        var result = await _mediator.Send(new BatchDeleteMapsCommand(request.Ids ?? new List<Guid>()));
        return StatusCode(result.GetHttpStatusCode(), result);
    }
}
