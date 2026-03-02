using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.ApproveMap;

public record ApproveMapCommand(Guid MapId, string? ReviewNote = null) : IRequest<Result>;
