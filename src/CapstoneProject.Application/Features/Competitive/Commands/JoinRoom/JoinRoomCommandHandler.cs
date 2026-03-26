using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Competitive;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Competitive.Commands.JoinRoom;

public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, Result<JoinRoomResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public JoinRoomCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<JoinRoomResultDto>> Handle(JoinRoomCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<JoinRoomResultDto>.Failure("Authentication required. Please log in to join a room.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var room = await _unitOfWork.Repository<Room>().GetQueryable()
            .Include(r => r.RoomParticipants)
            .FirstOrDefaultAsync(r => r.Code == command.RoomCode && !r.IsDeleted, cancellationToken);
        if (room == null)
            return Result<JoinRoomResultDto>.Failure("Room not found", ErrorCodeEnum.NotFound);
        if (room.RoomStatus != RoomStatusEnum.Waiting)
            return Result<JoinRoomResultDto>.Failure("Room is not waiting", ErrorCodeEnum.ValidationFailed);
        if (room.RoomParticipants.Count >= room.MaxPlayers)
            return Result<JoinRoomResultDto>.Failure("Room is full", ErrorCodeEnum.ValidationFailed);
        if (room.RoomParticipants.Any(p => p.UserId == userId))
            return Result<JoinRoomResultDto>.Success(new JoinRoomResultDto
            {
                RoomId = room.Id,
                MatchId = room.MatchId,
                RoomCode = room.Code!,
                CurrentPlayers = room.RoomParticipants.Count,
                MaxPlayers = room.MaxPlayers
            });

        var participant = new RoomParticipant
        {
            RoomId = room.Id,
            UserId = userId,
            JoinedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now,
            IsOwner = false,
            IsReady = false
        };
        participant.InitializeEntity(userId);
        await _unitOfWork.Repository<RoomParticipant>().AddAsync(participant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var count = await _unitOfWork.Repository<RoomParticipant>().GetQueryable().CountAsync(p => p.RoomId == room.Id && !p.IsDeleted, cancellationToken);
        return Result<JoinRoomResultDto>.Success(new JoinRoomResultDto
        {
            RoomId = room.Id,
            MatchId = room.MatchId,
            RoomCode = room.Code!,
            CurrentPlayers = count,
            MaxPlayers = room.MaxPlayers
        });
    }
}

