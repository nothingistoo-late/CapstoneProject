using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.DeleteMap;

public record DeleteMapCommand(Guid MapId) : IRequest<Result>;
