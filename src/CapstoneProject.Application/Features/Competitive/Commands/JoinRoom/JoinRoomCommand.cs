using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Competitive;
using MediatR;

namespace CapstoneProject.Application.Features.Competitive.Commands.JoinRoom;

public record JoinRoomCommand(string RoomCode) : IRequest<Result<JoinRoomResultDto>>;
