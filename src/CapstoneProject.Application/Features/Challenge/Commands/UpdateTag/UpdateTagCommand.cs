using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateTag;

public record UpdateTagCommand(Guid TagId, string Name) : IRequest<Result>;
