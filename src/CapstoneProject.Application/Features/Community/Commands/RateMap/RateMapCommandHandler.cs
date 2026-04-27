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

namespace CapstoneProject.Application.Features.Community.Commands.RateMap;

public class RateMapCommandHandler : IRequestHandler<RateMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public RateMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(RateMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để đánh giá trò chơi.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;
        if (command.Rating < 1 || command.Rating > 5)
            return Result.Failure("Đánh giá phải từ 1 đến 5 sao. Vui lòng cung cấp đánh giá hợp lệ.", ErrorCodeEnum.ValidationFailed);

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable()
            .FirstOrDefaultAsync(g => g.Id == command.GameId && !g.IsDeleted && g.Status == EntityStatusEnum.Active, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy trò chơi có Id: {command.GameId}. Trò chơi có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (game.CreatedBy.HasValue && game.CreatedBy.Value == userId)
            return Result.Failure("Bạn không thể xếp hạng trò chơi của riêng bạn.", ErrorCodeEnum.Forbidden);

        // Only allow rating games the user can actually play:
        // - Free games (Price null or <= 0)
        // - OR paid games that the user has already purchased (PaymentRecord Completed for this game)
        var isFreeMap = !game.Price.HasValue || game.Price <= 0;
        if (!isFreeMap)
        {
            var paymentRepo = _unitOfWork.Repository<PaymentRecord>();
            var hasPurchased = await paymentRepo.GetQueryable()
                .AnyAsync(p => !p.IsDeleted
                               && p.UserId == userId
                               && p.GameId == game.Id
                               && p.PaymentStatus == PaymentStatusEnum.Completed,
                    cancellationToken);
            if (!hasPurchased)
                return Result.Failure("Bạn chỉ có thể xếp hạng trò chơi mà bạn có quyền truy cập (trò chơi miễn phí hoặc trò chơi bạn đã mua).", ErrorCodeEnum.Forbidden);
        }

        var repo = _unitOfWork.Repository<GameRating>();
        var existing = await repo.GetQueryable().FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == command.GameId && !r.IsDeleted, cancellationToken);
        if (existing != null)
        {
            existing.Rating = command.Rating;
            existing.Comment = command.Comment;
            existing.UpdateEntity(userId);
            repo.Update(existing);
        }
        else
        {
            var rating = new GameRating { UserId = userId, GameId = command.GameId, Rating = command.Rating, Comment = command.Comment };
            rating.InitializeEntity(userId);
            await repo.AddAsync(rating);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (game.CreatedBy.HasValue && game.CreatedBy.Value != userId)
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(new
                {
                    gameId = game.Id,
                    mapTitle = game.Title,
                    rating = command.Rating,
                    hasComment = !string.IsNullOrWhiteSpace(command.Comment)
                });

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.GameRatingReceived,
                    "Game nhận được đánh giá mới",
                    $"Game \"{game.Title}\" vừa nhận {command.Rating} sao.",
                    new List<Guid> { game.CreatedBy.Value },
                    userId,
                    payloadJson,
                    $"/learner/games/{game.Id}",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not block rating flow.
            }
        }

        return Result.Success("Đã lưu xếp hạng.");
    }
}
