using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using System.Text.Json;

namespace CapstoneProject.Application.Features.Games.Commands.RejectMap;

public class RejectMapCommandHandler : IRequestHandler<RejectMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public RejectMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(RejectMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện hành động này.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("Bạn không có quyền từ chối trò chơi. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này.", ErrorCodeEnum.Forbidden);

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy trò chơi có Id: {command.GameId}. Trò chơi có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (game.GameStatus != GameStatusEnum.PendingReview)
            return Result.Failure($"Trò chơi không thể bị từ chối. Trạng thái dự kiến: Đang chờ xem xét. Trạng thái hiện tại: {game.GameStatus}. Chỉ những trò chơi đang chờ xem xét mới có thể bị từ chối.", ErrorCodeEnum.InvalidOperation);

        var normalizedRejectReason = NormalizeNote(command.RejectReason);
        game.GameStatus = GameStatusEnum.Rejected;
        game.ReviewNote = normalizedRejectReason;
        game.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (game.CreatedBy.HasValue)
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
                    new List<Guid> { game.CreatedBy.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-games",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break reject flow.
            }
        }

        return Result.Success("Trò chơi đã bị từ chối thành công.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}
