using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateMap;

public record UpdateMapCommand(Guid MapId, UpdateMapRequest Request) : IRequest<Result>;
