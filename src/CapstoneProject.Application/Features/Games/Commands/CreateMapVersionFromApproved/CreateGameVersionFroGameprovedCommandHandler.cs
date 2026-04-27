using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMapVersionFromApproved;

public class CreateMapVersionFromApprovedCommandHandler : IRequestHandler<CreateMapVersionFromApprovedCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateMapVersionFromApprovedCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateMapVersionFromApprovedCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var userId = userIdNullable.Value;
        var roles = await _currentUserService.GetCurrentRolesAsync();
        var isAdminOrMod = roles.Contains(RoleEnum.Admin) || roles.Contains(RoleEnum.Moderator);

        var mapRepo = _unitOfWork.Repository<Game>();
        var source = await mapRepo.GetQueryable()
            .AsNoTracking()
            .Include(m => m.GameDetails)
            .ThenInclude(d => d.Hints)
            .Include(m => m.GameTags)
            .Include(m => m.GameMedias)
            .FirstOrDefaultAsync(m => m.Id == command.SourceGameId && !m.IsDeleted, cancellationToken);

        if (source == null)
            return Result<Guid>.Failure($"Không tìm thấy trò chơi có Id: {command.SourceGameId}.", ErrorCodeEnum.NotFound);

        if (source.CreatedBy != userId && !isAdminOrMod)
            return Result<Guid>.Failure("Bạn không có quyền tạo version từ trò chơi này.", ErrorCodeEnum.Forbidden);

        if (source.GameStatus != GameStatusEnum.Approved && source.GameStatus != GameStatusEnum.Published)
            return Result<Guid>.Failure("Chỉ có thể tạo version mới từ game đã duyệt hoặc đã xuất bản.", ErrorCodeEnum.InvalidOperation);

        var rootGameId = source.RootGameId ?? source.Id;

        var hasPending = await mapRepo.GetQueryable().AnyAsync(
            m => !m.IsDeleted
                 && (m.RootGameId ?? m.Id) == rootGameId
                 && m.GameStatus == GameStatusEnum.PendingReview,
            cancellationToken);
        if (hasPending)
            return Result<Guid>.Failure("Đã có một version đang chờ duyệt trong game line này.", ErrorCodeEnum.InvalidOperation);

        var newVersion = new Game
        {
            Title = source.Title,
            Description = source.Description,
            Difficulty = source.Difficulty,
            IsPublished = false,
            GameStatus = GameStatusEnum.Draft,
            Price = source.Price,
            FreeTrialAttemptLimit = source.FreeTrialAttemptLimit,
            EditorialContent = source.EditorialContent,
            UnlockEditorialAfterStars = source.UnlockEditorialAfterStars,
            LearnedTags = new List<Guid>(source.LearnedTags),
            AvatarUrl = source.AvatarUrl,
            // Keep same content version on clone; first actual edit/save will increment by +1.
            ContentVersion = Math.Max(1, source.ContentVersion),
            RootGameId = rootGameId,
            IsActiveVersion = false
        };
        newVersion.InitializeEntity(userId);
        await mapRepo.AddAsync(newVersion);

        foreach (var mt in source.GameTags.Where(t => !t.IsDeleted))
        {
            var mapTag = new GameTag { GameId = newVersion.Id, TagId = mt.TagId };
            mapTag.InitializeEntity(userId);
            await _unitOfWork.Repository<GameTag>().AddAsync(mapTag);
        }

        foreach (var d in source.GameDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder))
        {
            var newDetail = new GameDetail
            {
                GameId = newVersion.Id,
                LevelOrder = d.LevelOrder,
                Title = d.Title,
                JsonContent = d.JsonContent,
                TimeLimitMs = d.TimeLimitMs,
                WinCondition = d.WinCondition,
                Type = d.Type
            };
            newDetail.InitializeEntity(userId);
            await _unitOfWork.Repository<GameDetail>().AddAsync(newDetail);

            foreach (var h in d.Hints.Where(x => !x.IsDeleted).OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { GameDetailId = newDetail.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await _unitOfWork.Repository<Hint>().AddAsync(hint);
            }
        }

        foreach (var mm in source.GameMedias.Where(m => !m.IsDeleted).OrderBy(m => m.SortOrder))
        {
            var media = new GameMedia
            {
                GameId = newVersion.Id,
                Url = mm.Url,
                Kind = mm.Kind,
                SortOrder = mm.SortOrder
            };
            media.InitializeEntity(userId);
            await _unitOfWork.Repository<GameMedia>().AddAsync(media);
        }

        var myMapRepo = _unitOfWork.Repository<MyGame>();
        var existedAuthor = await myMapRepo.GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.GameId == newVersion.Id, cancellationToken);
        if (!existedAuthor)
        {
            var myMap = new MyGame { GameId = newVersion.Id, UserId = userId, IsAuthor = true };
            myMap.InitializeEntity(userId);
            await myMapRepo.AddAsync(myMap);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(newVersion.Id, "Đã tạo version mới ở trạng thái Nháp. Vui lòng cập nhật và gửi duyệt lại.");
    }
}
