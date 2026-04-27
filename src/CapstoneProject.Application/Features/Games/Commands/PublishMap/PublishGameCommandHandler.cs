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

namespace CapstoneProject.Application.Features.Games.Commands.PublishMap;

public class PublishMapCommandHandler : IRequestHandler<PublishMapCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationPersistenceService _notificationPersistenceService;

    public PublishMapCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationPersistenceService notificationPersistenceService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationPersistenceService = notificationPersistenceService;
    }

    public async Task<Result> Handle(PublishMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xuất bản trò chơi.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrModerator = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);
        var isLearner = roles.Contains(RoleEnum.Learner);

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy trò chơi có Id: {command.GameId}. Trò chơi có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (game.GameStatus != GameStatusEnum.Approved)
            return Result.Failure($"Trò chơi không thể được xuất bản. Trạng thái dự kiến: Đã phê duyệt. Trạng thái hiện tại: {game.GameStatus}. Chỉ những trò chơi được phê duyệt mới có thể được xuất bản.", ErrorCodeEnum.InvalidOperation);

        if (isAdminOrModerator)
        {
            // Staff can publish any approved game (Learner API or CMS).
        }
        else if (isLearner)
        {
            if (game.CreatedBy != userIdNullable.Value)
                return Result.Failure("Chỉ tác giả của trò chơi này mới có thể xuất bản nó.", ErrorCodeEnum.Forbidden);
        }
        else
            return Result.Failure("Bạn không có quyền xuất bản trò chơi.", ErrorCodeEnum.Forbidden);

        var rootGameId = game.RootGameId ?? game.Id;
        if (!game.RootGameId.HasValue)
            game.RootGameId = rootGameId;

        var lineMaps = await mapRepo.GetQueryable()
            .Where(m => !m.IsDeleted && (m.RootGameId ?? m.Id) == rootGameId)
            .ToListAsync(cancellationToken);

        foreach (var sibling in lineMaps.Where(m => m.Id != game.Id && m.IsActiveVersion))
        {
            sibling.IsActiveVersion = false;
            sibling.IsPublished = false;
            sibling.UpdateEntity(userIdNullable.Value);
            mapRepo.Update(sibling);
        }

        game.GameStatus = GameStatusEnum.Published;
        game.IsPublished = true;
        game.IsActiveVersion = true;
        game.UpdateEntity(userIdNullable!.Value);
        mapRepo.Update(game);

        // Retention policy: keep current active + 2 latest published inactive versions.
        var publishedInactive = lineMaps
            .Where(m => m.Id != game.Id && !m.IsDeleted && m.GameStatus == GameStatusEnum.Published)
            .OrderByDescending(m => m.ContentVersion)
            .ThenByDescending(m => m.CreatedAt)
            .ToList();

        var keepSet = publishedInactive.Take(2).Select(m => m.Id).ToHashSet();
        foreach (var old in publishedInactive.Where(m => !keepSet.Contains(m.Id)))
        {
            old.IsActiveVersion = false;
            old.IsPublished = false;
            old.IsDeleted = true;
            old.DeletedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            old.DeletedBy = userIdNullable.Value;
            old.UpdateEntity(userIdNullable.Value);
            mapRepo.Update(old);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (game.CreatedBy.HasValue)
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(new
                {
                    gameId = game.Id,
                    mapTitle = game.Title,
                    contentVersion = game.ContentVersion
                });

                await _notificationPersistenceService.CreateNotificationAsync(
                    NotificationTypeEnum.MapVersionPublished,
                    "Game đã được xuất bản",
                    $"Game \"{game.Title}\" đã được xuất bản thành công.",
                    new List<Guid> { game.CreatedBy.Value },
                    userIdNullable.Value,
                    payloadJson,
                    $"/learner/games/{game.Id}",
                    cancellationToken);
            }
            catch
            {
                // Notification failure must not break publish flow.
            }

            // Notify buyers/founders of this game about the update
            try
            {
                var buyers = await _unitOfWork.Repository<MyGame>()
                    .GetQueryable()
                    .Where(mm => mm.GameId == game.Id && !mm.IsDeleted && !mm.IsAuthor)
                    .Select(mm => mm.UserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (buyers.Count > 0)
                {
                    var buyerPayloadJson = JsonSerializer.Serialize(new
                    {
                        gameId = game.Id,
                        mapTitle = game.Title,
                        contentVersion = game.ContentVersion,
                        creatorId = game.CreatedBy.Value
                    });

                    await _notificationPersistenceService.CreateNotificationAsync(
                        NotificationTypeEnum.MapUpdateForBuyers,
                        "Game bạn mua đã được cập nhật",
                        $"Game \"{game.Title}\" vừa được cập nhật lên phiên bản mới.",
                        buyers,
                        game.CreatedBy.Value,
                        buyerPayloadJson,
                        $"/learner/games/{game.Id}",
                        cancellationToken);
                }
            }
            catch
            {
                // Notification failure must not break publish flow.
            }
        }

        return Result.Success("Trò chơi được xuất bản thành công.");
    }
}
