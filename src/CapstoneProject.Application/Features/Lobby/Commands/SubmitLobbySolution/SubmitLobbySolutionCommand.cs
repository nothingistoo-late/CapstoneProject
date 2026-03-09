using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Gameplay;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.SubmitLobbySolution;

public record SubmitLobbySolutionCommand(Guid RoomId, SubmissionSubmitRequest Request) : IRequest<Result<SubmitGameResponse>>;
