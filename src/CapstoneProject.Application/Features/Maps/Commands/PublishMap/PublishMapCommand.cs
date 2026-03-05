using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.PublishMap;

public record PublishMapCommand(Guid MapId) : IRequest<Result>;
