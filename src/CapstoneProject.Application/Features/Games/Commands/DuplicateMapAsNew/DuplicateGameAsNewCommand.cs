using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Security;
using CapstoneProject.Application.Commons.DTOs.Games;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.DuplicateMapAsNew;

[
    RequiresFeature(FeatureKeys.CanCreateGame)
]
public record DuplicateMapAsNewCommand(Guid SourceGameId, DuplicateMapAsNewRequest? Request = null)
    : IRequest<Result<Guid>>;
