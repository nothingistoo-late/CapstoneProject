using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Games.Commands.CreateTag;

public record CreateTagCommand(string Name) : IRequest<Result<Guid>>;
