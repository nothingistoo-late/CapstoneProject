using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateTag;

public record CreateTagCommand(string Name) : IRequest<Result<Guid>>;
