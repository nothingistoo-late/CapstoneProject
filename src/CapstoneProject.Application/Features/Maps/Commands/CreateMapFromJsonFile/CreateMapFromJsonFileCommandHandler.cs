using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Features.Maps.Commands.CreateMap;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;

public class CreateMapFromJsonFileCommandHandler : IRequestHandler<CreateMapFromJsonFileCommand, Result<Guid>>
{
    private readonly IMediator _mediator;

    public CreateMapFromJsonFileCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(CreateMapFromJsonFileCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var (levelsFromFile, detailJson, parseErr) = MapFileJsonLevelsParser.ParseFromCreateMapInput(input);
        if (parseErr != null)
            return Result<Guid>.Failure(parseErr, ErrorCodeEnum.ValidationFailed);

        // Không dùng HintsJson nữa: hints được extract trực tiếp từ JSON map detail (mỗi level).
        if (levelsFromFile == null && detailJson.HasValue)
        {
            levelsFromFile = new List<MapLevelInputDto>
            {
                new() { LevelOrder = 0, Title = null, JsonContent = detailJson.Value }
            };
            detailJson = null;
        }
        if (levelsFromFile != null)
            MapHintsExtractor.MergeHintsFromJson(levelsFromFile);

        if (MapLevelMetadataExtractor.TryParseMapType(input.Type, out var defaultType) && levelsFromFile != null)
        {
            foreach (var level in levelsFromFile)
            {
                if (level.Type == null)
                    level.Type = defaultType;
            }
        }

        if (levelsFromFile == null && detailJson.HasValue && MapLevelMetadataExtractor.TryParseMapType(input.Type, out var singleType))
        {
            levelsFromFile = new List<MapLevelInputDto>
            {
                new() { LevelOrder = 0, Title = null, JsonContent = detailJson.Value, Type = singleType }
            };
            detailJson = null;
        }

        var tagIds = ParseTagIdsCsv(input.TagIdsCsv);
        if (tagIds == null)
            return Result<Guid>.Failure("TagIdsCsv contains invalid Guid(s).", ErrorCodeEnum.ValidationFailed);
        var learnedTags = ParseTagIdsCsv(input.LearnedTagsCsv);
        if (learnedTags == null)
            return Result<Guid>.Failure("LearnedTagsCsv contains invalid Guid(s).", ErrorCodeEnum.ValidationFailed);

        var createRequest = new CreateMapRequest
        {
            Title = input.Title,
            Description = input.Description,
            Difficulty = input.Difficulty,
            Price = input.Price,
            FreeTrialAttemptLimit = input.FreeTrialAttemptLimit ?? 0,
            TagIds = tagIds,
            LearnedTags = learnedTags,
            Levels = levelsFromFile,
            MapDetailJson = detailJson,
            AvatarUrl = null
        };

        var result = await _mediator.Send(
            new CreateMapCommand(createRequest, command.AutoPublish, command.GalleryFiles, command.AvatarFile),
            cancellationToken);
        return result;
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

