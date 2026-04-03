using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Helpers;
using CapstoneProject.Application.Features.Maps.Commands.UpdateMap;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMapFromJsonFile;

public class UpdateMapFromJsonFileCommandHandler : IRequestHandler<UpdateMapFromJsonFileCommand, Result>
{
    private readonly IMediator _mediator;

    public UpdateMapFromJsonFileCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result> Handle(UpdateMapFromJsonFileCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var (levelsFromFile, detailJson, parseErr) = MapFileJsonLevelsParser.ParseFromCreateMapInput(input);
        if (parseErr != null)
            return Result.Failure(parseErr, ErrorCodeEnum.ValidationFailed);

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

        var tagIds = ParseTagIdsCsv(input.TagIdsCsv);
        if (tagIds == null)
            return Result.Failure("TagIdsCsv contains invalid Guid(s).", ErrorCodeEnum.ValidationFailed);
        var learnedTags = ParseTagIdsCsv(input.LearnedTagsCsv);
        if (learnedTags == null)
            return Result.Failure("LearnedTagsCsv contains invalid Guid(s).", ErrorCodeEnum.ValidationFailed);

        var updateRequest = new UpdateMapRequest
        {
            Title = input.Title,
            Description = input.Description,
            Difficulty = input.Difficulty,
            Price = input.Price,
            FreeTrialAttemptLimit = input.FreeTrialAttemptLimit,
            TagIds = tagIds,
            LearnedTags = learnedTags,
            Levels = levelsFromFile,
            MapDetailJson = null
        };

        var result = await _mediator.Send(new UpdateMapCommand(command.MapId, updateRequest), cancellationToken);
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

