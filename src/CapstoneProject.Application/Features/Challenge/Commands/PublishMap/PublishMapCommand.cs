using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.PublishMap;

public record PublishMapCommand(Guid MapId) : IRequest<Result>;
