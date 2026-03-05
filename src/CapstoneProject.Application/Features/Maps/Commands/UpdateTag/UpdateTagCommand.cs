using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateTag;

public record UpdateTagCommand(Guid TagId, string Name) : IRequest<Result>;
