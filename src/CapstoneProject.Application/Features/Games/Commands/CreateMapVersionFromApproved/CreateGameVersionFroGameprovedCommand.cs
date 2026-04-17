using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMapVersionFromApproved;

public record CreateMapVersionFromApprovedCommand(Guid SourceGameId) : IRequest<Result<Guid>>;
