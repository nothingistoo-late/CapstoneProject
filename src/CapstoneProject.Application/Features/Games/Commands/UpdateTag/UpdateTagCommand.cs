using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.UpdateTag;

public record UpdateTagCommand(Guid TagId, string Name) : IRequest<Result>;
