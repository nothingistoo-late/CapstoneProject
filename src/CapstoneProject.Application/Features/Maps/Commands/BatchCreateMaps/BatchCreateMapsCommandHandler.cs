using System.Text.Json;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using MapEntity = CapstoneProject.Domain.Entities.Maps;

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
        var repo = _unitOfWork.Repository<MapEntity>();

        foreach (var json in jsonStrings)
        {
            if (string.IsNullOrWhiteSpace(json) || json == "null")
            {
                errors.Add("Empty JSON content.");
                continue;
            }

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
            catch (Exception ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                continue;
            }

            var entity = new MapEntity
            {
                ExternalId = externalId,
                Name = name,
                JsonContent = json
            };
            entity.InitializeEntity(userId);
            await repo.AddAsync(entity);
            createdIds.Add(entity.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BatchCreateMapsResultDto
        {
            SuccessCount = createdIds.Count,
            FailedCount = errors.Count,
            CreatedIds = createdIds,
            Errors = errors
        };
        return Result<BatchCreateMapsResultDto>.Success(dto, $"Created {dto.SuccessCount} map(s), {dto.FailedCount} failed.");
    }
}
