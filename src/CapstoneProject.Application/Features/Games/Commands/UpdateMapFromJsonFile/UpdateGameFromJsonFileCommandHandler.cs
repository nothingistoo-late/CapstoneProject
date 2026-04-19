using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Features.Games.Commands.UpdateMap;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateMapFromJsonFile;

public class UpdateMapFromJsonFileCommandHandler : IRequestHandler<UpdateMapFromJsonFileCommand, Result<Guid>>
{
    private readonly IMediator _mediator;

    public UpdateMapFromJsonFileCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(UpdateMapFromJsonFileCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var (levelsFromFile, detailJson, parseErr) = MapFileJsonLevelsParser.ParseFromCreateMapInput(input);
        if (parseErr != null)
            return Result<Guid>.Failure(parseErr, ErrorCodeEnum.ValidationFailed);

        // KhÃ´ng dÃ¹ng HintsJson ná»¯a: hints Ä‘Æ°á»£c extract trá»±c tiáº¿p tá»« JSON game detail (má»—i level).
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
            return Result<Guid>.Failure("TagIdsCsv chứa (các) Hướng dẫn không hợp lệ.", ErrorCodeEnum.ValidationFailed);
        var learnedTags = ParseTagIdsCsv(input.LearnedTagsCsv);
        if (learnedTags == null)
            return Result<Guid>.Failure("LearnedTagsCsv chứa (các) Hướng dẫn không hợp lệ.", ErrorCodeEnum.ValidationFailed);

        var updateRequest = new UpdateMapRequest
        {
            Title = input.Title,
            Description = input.Description,
            Difficulty = input.Difficulty,
            Price = input.Price,
            AvatarUrl = null,
            FreeTrialAttemptLimit = input.FreeTrialAttemptLimit,
            TagIds = tagIds,
            LearnedTags = learnedTags,
            Levels = levelsFromFile,
            GameDetailJson = null
        };

        var result = await _mediator.Send(new UpdateMapCommand(command.GameId, updateRequest), cancellationToken);
        if (!result.IsSuccess)
            return Result<Guid>.Failure(
                result.Message ?? "Cập nhật game thất bại.",
                Enum.TryParse<ErrorCodeEnum>(result.ErrorCode, out var errorCode) ? errorCode : ErrorCodeEnum.InvalidInput,
                result.Errors);

        return Result<Guid>.Success(command.GameId, result.Message ?? "Cập nhật game thành công.");
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

