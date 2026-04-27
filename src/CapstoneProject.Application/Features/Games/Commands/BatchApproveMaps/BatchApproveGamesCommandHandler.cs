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

namespace CapstoneProject.Application.Features.Games.Commands.BatchApproveMaps;

public class BatchApproveMapsCommandHandler : IRequestHandler<BatchApproveMapsCommand, Result<BatchMapResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public BatchApproveMapsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result<BatchMapResultDto>> Handle(BatchApproveMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<BatchMapResultDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện phê duyệt hàng loạt.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<BatchMapResultDto>.Failure("Bạn không có quyền phê duyệt trò chơi. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện phê duyệt hàng loạt.", ErrorCodeEnum.Forbidden);

        var repo = _unitOfWork.Repository<Game>();
        var games = await repo.GetQueryable()
            .Where(m => command.GameIds.Contains(m.Id) && !m.IsDeleted)
            .ToListAsync(cancellationToken);
        var foundIds = games.Select(m => m.Id).ToHashSet();
        var notFoundIds = command.GameIds.Where(id => !foundIds.Contains(id)).ToList();
        var toApprove = games.Where(m => m.GameStatus == GameStatusEnum.PendingReview).ToList();
        var invalidStatusIds = games.Where(m => m.GameStatus != GameStatusEnum.PendingReview).Select(m => m.Id).ToList();
        var normalizedReviewNote = NormalizeNote(command.ReviewNote);

        foreach (var game in toApprove)
        {
            game.GameStatus = GameStatusEnum.Approved;
            game.ReviewNote = normalizedReviewNote;
            game.UpdateEntity(userIdNullable!.Value);
            repo.Update(game);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var game in toApprove.Where(m => m.CreatedBy.HasValue))
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(new
                {
                    gameId = game.Id,
                    mapTitle = game.Title,
                    status = game.GameStatus.ToString(),
                    reviewNote = normalizedReviewNote
                });

                var body = string.IsNullOrWhiteSpace(normalizedReviewNote)
                    ? $"Game \"{game.Title}\" đã được duyệt."
                    : $"Game \"{game.Title}\" đã được duyệt. Ghi chú: {normalizedReviewNote}";

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.SystemAnnouncement,
                    "Game của bạn đã được duyệt",
                    body,
                    new List<Guid> { game.CreatedBy!.Value },
                    userIdNullable.Value,
                    payloadJson,
                    "/app/my-games",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break batch approval flow.
            }
        }

        var dto = new BatchMapResultDto
        {
            SuccessCount = toApprove.Count,
            FailedCount = notFoundIds.Count + invalidStatusIds.Count,
            NotFoundIds = notFoundIds,
            InvalidStatusIds = invalidStatusIds
        };
        return Result<BatchMapResultDto>.Success(dto, $"(Các) trò chơi {dto.SuccessCount} đã được phê duyệt.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;
        return note.Trim();
    }
}
