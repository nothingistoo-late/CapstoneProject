using System.Text.Json;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using MapEntity = CapstoneProject.Domain.Entities.Maps;

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

        string json;
        if (command.Request.Level.HasValue && command.Request.Level.Value.ValueKind != JsonValueKind.Null && command.Request.Level.Value.ValueKind != JsonValueKind.Undefined)
            json = command.Request.Level.Value.GetRawText();
        else if (!string.IsNullOrWhiteSpace(command.Request.JsonContent))
            json = command.Request.JsonContent;
        else
            return Result<MapsResponseDto>.Failure("Either Level (object) or JsonContent (string) is required.", ErrorCodeEnum.ValidationFailed);

        string? externalId = null;
        string name = "Unnamed";
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("id", out var idEl))
                externalId = idEl.GetString();
            if (root.TryGetProperty("name", out var nameEl))
                name = nameEl.GetString() ?? name;
        }
        catch
        {
            // Giữ name mặc định nếu JSON không hợp lệ
        }

        var entity = new MapEntity
        {
            ExternalId = externalId,
            Name = name,
            JsonContent = json
        };
        entity.InitializeEntity(userId);

        var repo = _unitOfWork.Repository<MapEntity>();
        await repo.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MapsResponseDto
        {
            Id = entity.Id,
            ExternalId = entity.ExternalId,
            Name = entity.Name,
            JsonContent = entity.JsonContent,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
        return Result<MapsResponseDto>.Success(dto, "Map created successfully.");
    }
}
