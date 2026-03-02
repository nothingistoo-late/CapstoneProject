using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Community.Commands.RateMap;

public record RateMapCommand(Guid MapId, int Rating, string? Comment = null) : IRequest<Result>;
