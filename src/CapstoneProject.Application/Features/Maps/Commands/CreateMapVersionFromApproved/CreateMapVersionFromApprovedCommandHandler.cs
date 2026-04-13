using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapVersionFromApproved;

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

        var mapRepo = _unitOfWork.Repository<Map>();
        var source = await mapRepo.GetQueryable()
            .AsNoTracking()
            .Include(m => m.MapDetails)
            .ThenInclude(d => d.Hints)
            .Include(m => m.MapTags)
            .Include(m => m.MapMedias)
            .FirstOrDefaultAsync(m => m.Id == command.SourceMapId && !m.IsDeleted, cancellationToken);

        if (source == null)
            return Result<Guid>.Failure($"Không tìm thấy bản đồ có Id: {command.SourceMapId}.", ErrorCodeEnum.NotFound);

        if (source.CreatedBy != userId && !isAdminOrMod)
            return Result<Guid>.Failure("Bạn không có quyền tạo version từ bản đồ này.", ErrorCodeEnum.Forbidden);

        if (source.MapStatus != MapStatusEnum.Approved && source.MapStatus != MapStatusEnum.Published)
            return Result<Guid>.Failure("Chỉ có thể tạo version mới từ map đã duyệt hoặc đã xuất bản.", ErrorCodeEnum.InvalidOperation);

        var rootMapId = source.RootMapId ?? source.Id;

        var hasPending = await mapRepo.GetQueryable().AnyAsync(
            m => !m.IsDeleted
                 && (m.RootMapId ?? m.Id) == rootMapId
                 && m.MapStatus == MapStatusEnum.PendingReview,
            cancellationToken);
        if (hasPending)
            return Result<Guid>.Failure("Đã có một version đang chờ duyệt trong game line này.", ErrorCodeEnum.InvalidOperation);

        var newVersion = new Map
        {
            Title = source.Title,
            Description = source.Description,
            Difficulty = source.Difficulty,
            IsPublished = false,
            MapStatus = MapStatusEnum.Draft,
            Price = source.Price,
            FreeTrialAttemptLimit = source.FreeTrialAttemptLimit,
            EditorialContent = source.EditorialContent,
            UnlockEditorialAfterStars = source.UnlockEditorialAfterStars,
            LearnedTags = new List<Guid>(source.LearnedTags),
            AvatarUrl = source.AvatarUrl,
            // Keep same content version on clone; first actual edit/save will increment by +1.
            ContentVersion = Math.Max(1, source.ContentVersion),
            RootMapId = rootMapId,
            IsActiveVersion = false
        };
        newVersion.InitializeEntity(userId);
        await mapRepo.AddAsync(newVersion);

        foreach (var mt in source.MapTags.Where(t => !t.IsDeleted))
        {
            var mapTag = new MapTag { MapId = newVersion.Id, TagId = mt.TagId };
            mapTag.InitializeEntity(userId);
            await _unitOfWork.Repository<MapTag>().AddAsync(mapTag);
        }

        foreach (var d in source.MapDetails.Where(d => !d.IsDeleted).OrderBy(d => d.LevelOrder))
        {
            var newDetail = new MapDetail
            {
                MapId = newVersion.Id,
                LevelOrder = d.LevelOrder,
                Title = d.Title,
                JsonContent = d.JsonContent,
                TimeLimitMs = d.TimeLimitMs,
                WinCondition = d.WinCondition,
                Type = d.Type
            };
            newDetail.InitializeEntity(userId);
            await _unitOfWork.Repository<MapDetail>().AddAsync(newDetail);

            foreach (var h in d.Hints.Where(x => !x.IsDeleted).OrderBy(x => x.OrderNo))
            {
                var hint = new Hint { MapDetailId = newDetail.Id, OrderNo = h.OrderNo, Content = h.Content };
                hint.InitializeEntity(userId);
                await _unitOfWork.Repository<Hint>().AddAsync(hint);
            }
        }

        foreach (var mm in source.MapMedias.Where(m => !m.IsDeleted).OrderBy(m => m.SortOrder))
        {
            var media = new MapMedia
            {
                MapId = newVersion.Id,
                Url = mm.Url,
                Kind = mm.Kind,
                SortOrder = mm.SortOrder
            };
            media.InitializeEntity(userId);
            await _unitOfWork.Repository<MapMedia>().AddAsync(media);
        }

        var myMapRepo = _unitOfWork.Repository<MyMap>();
        var existedAuthor = await myMapRepo.GetQueryable()
            .AnyAsync(mm => !mm.IsDeleted && mm.UserId == userId && mm.MapId == newVersion.Id, cancellationToken);
        if (!existedAuthor)
        {
            var myMap = new MyMap { MapId = newVersion.Id, UserId = userId, IsAuthor = true };
            myMap.InitializeEntity(userId);
            await myMapRepo.AddAsync(myMap);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(newVersion.Id, "Đã tạo version mới ở trạng thái Nháp. Vui lòng cập nhật và gửi duyệt lại.");
    }
}
