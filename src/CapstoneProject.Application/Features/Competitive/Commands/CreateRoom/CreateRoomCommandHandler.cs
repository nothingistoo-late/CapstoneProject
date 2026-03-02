using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Competitive.Commands.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Result<CreateRoomResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoomCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CreateRoomResultDto>> Handle(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<CreateRoomResultDto>.Failure("Authentication required. Please log in to create a room.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var match = await _unitOfWork.Repository<Match>().GetQueryable().FirstOrDefaultAsync(m => m.Id == command.MatchId && !m.IsDeleted, cancellationToken);
        if (match == null)
            return Result<CreateRoomResultDto>.Failure("Match not found", ErrorCodeEnum.NotFound);

        var maxPlayers = Math.Clamp(command.MaxPlayers, 2, 8);
        var code = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var room = new Room
        {
            MatchId = match.Id,
            MaxPlayers = maxPlayers,
            Code = code,
            RoomStatus = RoomStatusEnum.Waiting
        };
        room.InitializeEntity(userId);
        await _unitOfWork.Repository<Room>().AddAsync(room);

        var participant = new RoomParticipant
        {
            RoomId = room.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            IsOwner = true,
            IsReady = false
        };
        participant.InitializeEntity(userId);
        await _unitOfWork.Repository<RoomParticipant>().AddAsync(participant);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateRoomResultDto>.Success(new CreateRoomResultDto { RoomId = room.Id, RoomCode = code });
    }
}
