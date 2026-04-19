using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateMapFromJsonFile;

public record UpdateMapFromJsonFileCommand(Guid GameId, CreateMapFromJsonFileInput Input) : IRequest<Result<Guid>>;

