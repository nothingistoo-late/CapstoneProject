using System.Text.Json;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMaps;

public class CreateMapsCommandHandler : IRequestHandler<CreateMapsCommand, Result<MapsResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MapsResponseDto>> Handle(CreateMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<MapsResponseDto>.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var req = command.Request;
        if (!req.Level.HasValue || req.Level.Value.ValueKind == JsonValueKind.Null || req.Level.Value.ValueKind == JsonValueKind.Undefined)
            return Result<MapsResponseDto>.Failure("Level (object) is required.", ErrorCodeEnum.ValidationFailed);

        var json = req.Level.Value.GetRawText();
        string name = req.Name ?? "Unnamed";
        string? type = req.Type;
        string? difficulty = req.Difficulty;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (string.IsNullOrWhiteSpace(req.Name) && root.TryGetProperty("name", out var nameEl)) name = nameEl.GetString() ?? name;
            if (root.TryGetProperty("type", out var typeEl)) type ??= typeEl.GetString();
            if (root.TryGetProperty("metadata", out var meta) && meta.TryGetProperty("difficulty", out var diffEl)) difficulty ??= diffEl.GetString();
        }
        catch { /* keep from request */ }

        var catalog = new LevelCatalog { Name = name, Type = type, Difficulty = difficulty };
        catalog.InitializeEntity(userId);

        var catalogRepo = _unitOfWork.Repository<LevelCatalog>();
        await catalogRepo.AddAsync(catalog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = new LevelDetail { LevelCatalogId = catalog.Id, JsonContent = json };
        detail.InitializeEntity(userId);
        var detailRepo = _unitOfWork.Repository<LevelDetail>();
        await detailRepo.AddAsync(detail);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MapsResponseDto
        {
            Id = catalog.Id,
            Name = catalog.Name,
            Type = catalog.Type,
            Difficulty = catalog.Difficulty,
            JsonContent = json,
            CreatedAt = catalog.CreatedAt,
            UpdatedAt = catalog.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Level created successfully.");
    }
}
