using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Games.Commands.BatchRejectMaps;

public class BatchRejectMapsCommandHandler : IRequestHandler<BatchRejectMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public BatchRejectMapsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchRejectMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện từ chối hàng loạt.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("Bạn không có quyền từ chối bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện từ chối hàng loạt.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Game>();
        var games = await repo.GetQueryable()
            .Where(m => command.GameIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = games.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.GameIds.Where(id => !foundIds.Contains(id)).ToList();
        var toReject = games.Where(m => m.GameStatus == GameStatusEnum.PendingReview).ToList();
        var invalidStatusIds = games.Where(m => m.GameStatus != GameStatusEnum.PendingReview).Select(m => m.Id).ToList();
        var normalizedRejectReason = NormalizeNote(command.RejectReason);

        foreach (var game in toReject)
        {
            game.GameStatus = GameStatusEnum.Rejected;
            game.ReviewNote = normalizedRejectReason;
            game.UpdateEntity(userIdNullable!.Value);
            repo.Update(game);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var game in toReject.Where(m => m.CreatedBy.HasValue))
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(new
                {
                    gameId = game.Id,
                    mapTitle = game.Title,
                    status = game.GameStatus.ToString(),
                    rejectReason = normalizedRejectReason
                });

                var body = string.IsNullOrWhiteSpace(normalizedRejectReason)
                    ? $"Game \"{game.Title}\" đã bị từ chối."
                    : $"Game \"{game.Title}\" đã bị từ chối. Lý do: {normalizedRejectReason}";

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.SystemAnnouncement,
                    "Game của bạn đã bị từ chối",
                    body,
                    new List<Guid> { game.CreatedBy!.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-games",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break batch reject flow.
            }
        }

        var dto = new BatchMapResultDto
        {
            SuccessCount = toReject.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"Đã từ chối (các) bản đồ {dto.SuccessCount}.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}
