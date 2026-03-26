using System.Text.Json;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Maps.Commands.CreateMap;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;

public class CreateMapFromJsonFileCommandHandler : IRequestHandler<CreateMapFromJsonFileCommand, Result<Guid>>
{
    private readonly IMediator _mediator;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateMapFromJsonFileCommandHandler(IMediator mediator, ICloudinaryService cloudinaryService)
    {
        _mediator = mediator;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<Guid>> Handle(CreateMapFromJsonFileCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (string.IsNullOrWhiteSpace(input.MapDetailJsonContent))
            return Result<Guid>.Failure("MapDetailFile content is required.", ErrorCodeEnum.ValidationFailed);

        JsonElement detailJson;
        try
        {
            detailJson = JsonSerializer.Deserialize<JsonElement>(input.MapDetailJsonContent);
        }
        catch (JsonException)
        {
            return Result<Guid>.Failure("Uploaded file is not valid JSON.", ErrorCodeEnum.ValidationFailed);
        }

        var hints = ParseHintsJson(input.HintsJson);
        if (hints == null)
            return Result<Guid>.Failure("HintsJson must be valid JSON (array or object).", ErrorCodeEnum.ValidationFailed);

        var tagIds = ParseTagIdsCsv(input.TagIdsCsv);
        if (tagIds == null)
            return Result<Guid>.Failure("TagIdsCsv contains invalid Guid(s).", ErrorCodeEnum.ValidationFailed);

        string? avatarUrl = null;
        if (command.AvatarFile != null && command.AvatarFile.Length > 0)
        {
            avatarUrl = await _cloudinaryService.UploadImageAsync(
                command.AvatarFile,
                "maps",
                $"map_new_{CapstoneProject.Domain.Common.VietnamDateTime.DbNow.Ticks}",
                cancellationToken);
        }

        var createRequest = new CreateMapRequest
        {
            Title = input.Title,
            Description = input.Description,
            Difficulty = input.Difficulty,
            Type = input.Type,
            TimeLimitMs = input.TimeLimitMs,
            WinCondition = input.WinCondition,
            Price = input.Price,
            TagIds = tagIds,
            Hints = hints,
            MapDetailJson = detailJson,
            AvatarUrl = avatarUrl
        };

        var result = await _mediator.Send(new CreateMapCommand(createRequest, command.AutoPublish), cancellationToken);
        return result;
    }

    private static List<HintItemDto>? ParseHintsJson(string? hintsJson)
    {
        if (string.IsNullOrWhiteSpace(hintsJson)) return new List<HintItemDto>();

        try
        {
            using var doc = JsonDocument.Parse(hintsJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
                return JsonSerializer.Deserialize<List<HintItemDto>>(hintsJson) ?? new List<HintItemDto>();
            if (root.ValueKind == JsonValueKind.Object)
            {
                var one = JsonSerializer.Deserialize<HintItemDto>(hintsJson);
                return one != null ? new List<HintItemDto> { one } : new List<HintItemDto>();
            }
            if (root.ValueKind == JsonValueKind.String)
            {
                var inner = root.GetString();
                if (string.IsNullOrWhiteSpace(inner)) return new List<HintItemDto>();
                using var innerDoc = JsonDocument.Parse(inner);
                if (innerDoc.RootElement.ValueKind == JsonValueKind.Array)
                    return JsonSerializer.Deserialize<List<HintItemDto>>(inner) ?? new List<HintItemDto>();
                if (innerDoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var one = JsonSerializer.Deserialize<HintItemDto>(inner);
                    return one != null ? new List<HintItemDto> { one } : new List<HintItemDto>();
                }
            }
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<Guid>? ParseTagIdsCsv(string? tagIdsCsv)
    {
        if (string.IsNullOrWhiteSpace(tagIdsCsv)) return new List<Guid>();

        var list = new List<Guid>();
        var tokens = tagIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (!Guid.TryParse(token, out var tagId)) return null;
            list.Add(tagId);
        }
        return list;
    }
}



