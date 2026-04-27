using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Common.Security;
using CapstoneProject.Application.Commons.DTOs.Lobby;
using MediatR;

namespace CapstoneProject.Application.Features.Lobby.Commands.SetLobbyRoomLock;

[
    RequiresFeature(FeatureKeys.CanPrivateRoom)
]
public record SetLobbyRoomLockCommand(Guid RoomId, bool IsLocked) : IRequest<Result<LobbyRoomDetailResponse>>;
