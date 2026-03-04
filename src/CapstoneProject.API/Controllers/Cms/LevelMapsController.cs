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
/// Quản lý Level Maps dành cho CMS: catalog (name, type, difficulty) + JSON chi tiết level (layers, start/goal, metadata...).
/// </summary>
[ApiController]
[Route("api/cms/level-maps")]
[ApiExplorerSettings(GroupName = "v1")]
[Configurations.Tags("CMS - Level Maps")]
[SwaggerTag("Manage level catalog and raw level JSON: list/search, create/update/delete single level, batch import from level editor, and sync catalog info from FE.")]
public class LevelMapsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LevelMapsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lấy danh sách level maps có phân trang (catalog view).</summary>
    /// <remarks>
    /// Trả về danh sách level (catalog) cho CMS, có phân trang và lọc.
    ///
    /// **Query (MapsFilter/BasePaginationFilter):**
    /// - page (int?, optional): trang, bắt đầu từ 1. Mặc định 1.
    /// - pageSize (int?, optional): số bản ghi / trang. Mặc định 10.
    /// - search (string?, optional): tìm theo tên level (không phân biệt hoa thường).
    /// - sortBy (string?, optional): name | createdAt | updatedAt. Mặc định createdAt.
    /// - isAscending (bool?, optional): true = tăng dần, false = giảm dần. Mặc định false.
    /// - status (EntityStatusEnum?, optional): lọc theo trạng thái Active/Inactive.
    ///
    /// **Response:** PaginationResult&lt;MapsListItemDto&gt;.
    /// </remarks>
    [HttpGet]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(PaginationResult<MapsListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Get paged level catalog",
        Description =
            "Returns paginated level catalog for CMS.\n\n" +
            "**Query params (from BasePaginationFilter):**\n" +
            "- page (int?, optional): page index (1-based). Default 1.\n" +
            "- pageSize (int?, optional): page size. Default 10.\n" +
            "- search (string?, optional): case-insensitive search by level name.\n" +
            "- sortBy (string?, optional): name | createdAt | updatedAt. Default createdAt.\n" +
            "- isAscending (bool?, optional): sort direction. Default false.\n" +
            "- status (EntityStatusEnum?, optional): filter by Active/Inactive.\n\n" +
            "**Response:** PaginationResult<MapsListItemDto> (id, name, type, difficulty, createdAt).",
        OperationId = "GetPagedLevelMaps",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> GetPaged([FromQuery] MapsFilter filter)
    {
        var result = await _mediator.Send(new GetPagedMapsQuery(filter));
        return Ok(result);
    }

    /// <summary>Lấy chi tiết một level (catalog + JSON) theo Id.</summary>
    /// <remarks>
    /// Dùng cho CMS xem đầy đủ thông tin một level.
    ///
    /// **Route:**
    /// - id (Guid, bắt buộc): Id của LevelCatalog.
    ///
    /// **Response (MapsResponseDto):**
    /// - id, name, type, difficulty: thông tin catalog.
    /// - jsonContent: JSON chi tiết level (có thể null nếu mới chỉ có catalog).
    /// - createdAt, updatedAt.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get level by ID",
        Description =
            "Returns catalog info and, if available, full JSON level definition.\n\n" +
            "**Route:** id (Guid, required): LevelCatalog Id.\n\n" +
            "**Response data (MapsResponseDto):**\n" +
            "- id: Guid of LevelCatalog.\n" +
            "- name, type, difficulty: catalog fields.\n" +
            "- jsonContent: raw JSON from LevelDetail (can be null if only catalog created).\n" +
            "- createdAt, updatedAt.\n",
        OperationId = "GetLevelMapById",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMapsByIdQuery(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Tạo một level mới từ JSON level editor.</summary>
    /// <remarks>
    /// Tạo mới một level từ JSON đầy đủ (export từ level editor).
    ///
    /// **Body (CreateMapsRequest):**
    /// - level (object, bắt buộc): JSON đầy đủ của level (id, name, width, height, layers, startPosition, goalPosition, metadata...).
    /// - name (string?, optional): override tên lưu ở catalog (nếu null thì lấy từ level.name).
    /// - type (string?, optional): ví dụ: platform | topdown (nếu null thì cố gắng đọc từ level/type/metadata).
    /// - difficulty (string?, optional): ví dụ: easy | medium | hard (nếu null thì cố gắng đọc từ metadata).
    ///
    /// **Hành vi:**
    /// - Lưu LevelCatalog (name, type, difficulty,...).
    /// - Lưu JSON thô vào LevelDetail.JsonContent, 1-1 với LevelCatalog.
    /// </remarks>
    [HttpPost]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Create level map",
        Description =
            "Creates a new level from a full JSON level object (usually exported from level editor).\n\n" +
            "**Body (CreateMapsRequest):**\n" +
            "- level (object, required): full level JSON with fields like id, name, width, height, tileset, layers, startPosition, goalPosition, metadata (difficulty, description, ...).\n" +
            "- name (string?, optional): override level name stored in catalog; falls back to level.name.\n" +
            "- type (string?, optional): e.g. platform | topdown; falls back to level.type or metadata.\n" +
            "- difficulty (string?, optional): e.g. easy | medium | hard; falls back to level.metadata.difficulty.\n\n" +
            "**Behavior:**\n" +
            "- Saves catalog in LevelCatalog (id, name, type, difficulty).\n" +
            "- Saves raw JSON into LevelDetail.JsonContent for the created catalog.\n",
        OperationId = "CreateLevelMap",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Create([FromBody] CreateMapsRequest request)
    {
        var result = await _mediator.Send(new CreateMapsCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Cập nhật catalog và/hoặc JSON của một level.</summary>
    /// <remarks>
    /// Cập nhật thông tin catalog và/hoặc JSON chi tiết cho một level đã tồn tại.
    ///
    /// **Route:**
    /// - id (Guid, bắt buộc): Id của LevelCatalog cần cập nhật.
    ///
    /// **Body (UpdateMapsRequest):**
    /// - name, type, difficulty (optional): các trường catalog cần đổi.
    /// - jsonContent (string?, optional): JSON level mới, sẽ ghi đè JSON cũ nếu được gửi lên.
    ///
    /// Nếu jsonContent không được gửi, handler chỉ cập nhật phần catalog.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<MapsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update level map",
        Description =
            "Updates a level's catalog info (name, type, difficulty) and optionally replaces the stored JSON.\n\n" +
            "**Route:** id (Guid, required): LevelCatalog Id.\n\n" +
            "**Body (UpdateMapsRequest):**\n" +
            "- name (string?, optional): new display name.\n" +
            "- type (string?, optional): new type (platform | topdown...).\n" +
            "- difficulty (string?, optional): new difficulty label.\n" +
            "- jsonContent (string?, optional): full JSON string to replace existing LevelDetail.JsonContent.\n\n" +
            "If jsonContent is provided, the old JSON is overwritten; otherwise only catalog fields are updated.",
        OperationId = "UpdateLevelMap",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMapsRequest request)
    {
        var result = await _mediator.Send(new UpdateMapsCommand(id, request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Xóa mềm một level (catalog và detail vẫn còn trong DB nhưng bị đánh dấu IsDeleted).</summary>
    /// <remarks>
    /// Đánh dấu xóa mềm một level trong catalog.
    ///
    /// **Route:**
    /// - id (Guid, bắt buộc): Id của LevelCatalog.
    ///
    /// Sau khi xóa mềm, level sẽ không xuất hiện trong các query bình thường (do global query filter), nhưng dữ liệu vẫn còn trong DB.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete level map",
        Description =
            "Soft-deletes a level in catalog (IsDeleted = true). LevelDetail JSON remains in DB but is filtered out by global query filter.\n\n" +
            "**Route:** id (Guid, required): LevelCatalog Id.\n",
        OperationId = "DeleteLevelMap",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMapsCommand(id));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch tạo nhiều level từ danh sách JSON (import từ editor).</summary>
    /// <remarks>
    /// Import nhiều level cùng lúc từ JSON.
    ///
    /// **Body (BatchCreateMapsRequest):**
    /// - levels (array of object, optional): mỗi phần tử là JSON đầy đủ cho 1 level (giống create đơn).
    /// - jsonContents (array of string, optional): mỗi phần tử là string JSON cho 1 level.
    ///
    /// **Hành vi:**
    /// - Với mỗi JSON hợp lệ: tạo 1 LevelCatalog + 1 LevelDetail.
    /// - Trả về: successCount, failedCount, createdIds và danh sách lỗi tương ứng.
    /// </remarks>
    [HttpPost("batch/create")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchCreateMapsResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Batch create level maps",
        Description =
            "Imports multiple levels at once.\n\n" +
            "**Body (BatchCreateMapsRequest):**\n" +
            "- levels (array of objects, optional): each item is a full level JSON object (same shape as single create).\n" +
            "- jsonContents (array of strings, optional): each item is a JSON string for a level (for tools that already export strings).\n\n" +
            "**Behavior:**\n" +
            "- For each valid JSON: creates a LevelCatalog + LevelDetail.\n" +
            "- Returns successCount, failedCount, createdIds, and error messages for invalid JSON items.",
        OperationId = "BatchCreateLevelMaps",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> BatchCreate([FromBody] BatchCreateMapsRequest request)
    {
        var result = await _mediator.Send(new BatchCreateMapsCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Đồng bộ catalog từ FE: danh sách { id, file, name, type, difficulty } (không đụng tới JSON chi tiết).</summary>
    /// <remarks>
    /// Dùng cho FE đồng bộ danh sách level (catalog) mà không sửa JSON chi tiết.
    ///
    /// **Body (BatchUpsertCatalogRequest):**
    /// {
    ///   "levels": [
    ///     { "id": "platform-01", "file": "level-platform-01.json", "name": "Platform Challenge", "type": "platform", "difficulty": "medium" },
    ///     ...
    ///   ]
    /// }
    ///
    /// Hiện tại logic upsert dựa trên name (tên level) để quyết định tạo mới hay cập nhật.
    /// Không tạo/cập nhật LevelDetail.JsonContent.
    /// </remarks>
    [HttpPost("batch/upsert-catalog")]
    [AuthorizeRoles(nameof(RoleEnum.Admin), nameof(RoleEnum.Moderator))]
    [ProducesResponseType(typeof(Result<BatchUpsertCatalogResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Batch upsert catalog",
        Description =
            "Sync level catalog from FE (e.g. level editor UI) without changing JSON detail.\n\n" +
            "**Body (BatchUpsertCatalogRequest):**\n" +
            "- levels: array of LevelCatalogItemDto: { id, file, name, type, difficulty }.\n" +
            "  - id, file hiện tại được dùng như metadata từ FE; upsert thực tế đang dựa trên name (tên level) để tránh phụ thuộc ExternalId.\n\n" +
            "**Behavior:**\n" +
            "- If a LevelCatalog with the same name exists → update its type/difficulty (and other catalog fields).\n" +
            "- If not → create a new LevelCatalog.\n" +
            "- Does **not** create/update LevelDetail.JsonContent.",
        OperationId = "BatchUpsertLevelCatalog",
        Tags = new[] { "CMS - Level Maps" })]
    public async Task<IActionResult> BatchUpsertCatalog([FromBody] BatchUpsertCatalogRequest request)
    {
        var result = await _mediator.Send(new BatchUpsertCatalogCommand(request));
        return StatusCode(result.GetHttpStatusCode(), result);
    }

    /// <summary>Batch xóa (soft delete) nhiều level maps.</summary>
    /// <remarks>
    /// Xóa mềm nhiều level theo danh sách Id.
    ///
    /// **Body (BatchDeleteMapsRequest):**
    /// - ids: mảng Guid.
    ///
    /// **Response (BatchDeleteMapsResultDto):**
    /// - successCount: số level xóa được.
    /// - notFoundCount: số level không tìm thấy.
    /// - notFoundIds: danh sách Id không tồn tại.
    /// </remarks>
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
