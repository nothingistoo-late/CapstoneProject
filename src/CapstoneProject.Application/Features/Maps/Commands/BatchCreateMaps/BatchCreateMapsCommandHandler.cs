using System.Text.Json;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.BatchCreateMaps;

public class BatchCreateMapsCommandHandler : IRequestHandler<BatchCreateMapsCommand, Result<BatchCreateMapsResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BatchCreateMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BatchCreateMapsResultDto>> Handle(BatchCreateMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<BatchCreateMapsResultDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var jsonStrings = new List<string>();
        if (command.Request.Levels?.Count > 0)
        {
            foreach (var el in command.Request.Levels)
            {
                if (el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined)
                    jsonStrings.Add("null");
                else
                    jsonStrings.Add(el.GetRawText());
            }
        }
        else if (command.Request.JsonContents?.Count > 0)
            jsonStrings = command.Request.JsonContents;

        var createdIds = new List<Guid>();
        var errors = new List<string>();
        var catalogRepo = _unitOfWork.Repository<LevelCatalog>();
        var detailRepo = _unitOfWork.Repository<LevelDetail>();
        var createdCatalogs = new List<(LevelCatalog Catalog, string Json)>();

        foreach (var json in jsonStrings)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "null")
            {
                errors.Add("Empty JSON content.");
                continue;
            }

            string name = "Unnamed";
            string? type = null;
            string? difficulty = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("name", out var nameEl)) name = nameEl.GetString() ?? name;
                if (root.TryGetProperty("type", out var typeEl)) type = typeEl.GetString();
                if (root.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("difficulty", out var diffEl)) difficulty = diffEl.GetString();
            }
            catch (Exception ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                continue;
            }

            var catalog = new LevelCatalog
            {
                Name = name,
                Type = type,
                Difficulty = difficulty
            };
            catalog.InitializeEntity(userId);
            await catalogRepo.AddAsync(catalog);
            createdCatalogs.Add((catalog, json));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (catalog, json) in createdCatalogs)
        {
            var detail = new LevelDetail { LevelCatalogId = catalog.Id, JsonContent = json };
            detail.InitializeEntity(userId);
            await detailRepo.AddAsync(detail);
            createdIds.Add(catalog.Id);
        }

        if (createdCatalogs.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchCreateMapsResultDto
        {
            SuccessCount = createdIds.Count,
            FailedCount = errors.Count,
            CreatedIds = createdIds,
            Errors = errors
        };
        return Result<BatchCreateMapsResultDto>.Success(dto, $"Created {dto.SuccessCount} level(s), {dto.FailedCount} failed.");
    }
}
