using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMap;

public record DeleteMapCommand(Guid MapId) : IRequest<Result>;
