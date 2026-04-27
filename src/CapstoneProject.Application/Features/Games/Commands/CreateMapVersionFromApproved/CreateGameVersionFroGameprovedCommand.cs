using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Security;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMapVersionFromApproved;

[
    RequiresFeature(FeatureKeys.CanCreateGame)
]
public record CreateMapVersionFromApprovedCommand(Guid SourceGameId) : IRequest<Result<Guid>>;
