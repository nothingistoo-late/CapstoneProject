using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.DuplicateMapAsNew;

public record DuplicateMapAsNewCommand(Guid SourceGameId, DuplicateMapAsNewRequest? Request = null)
    : IRequest<Result<Guid>>;
