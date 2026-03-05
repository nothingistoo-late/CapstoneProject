using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateTag;

public record CreateTagCommand(string Name) : IRequest<Result<Guid>>;
