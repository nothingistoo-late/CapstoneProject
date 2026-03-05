using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Maps.Commands.UpdateMap;

public record UpdateMapCommand(Guid MapId, UpdateMapRequest Request) : IRequest<Result>;
