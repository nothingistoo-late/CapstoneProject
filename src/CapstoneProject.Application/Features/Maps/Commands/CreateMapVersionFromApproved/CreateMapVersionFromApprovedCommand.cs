using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapVersionFromApproved;

public record CreateMapVersionFromApprovedCommand(Guid SourceMapId) : IRequest<Result<Guid>>;
