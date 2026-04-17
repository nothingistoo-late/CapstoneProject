using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Games.Commands.SubmitMapForReview;

public class SubmitMapForReviewCommandHandler : IRequestHandler<SubmitMapForReviewCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitMapForReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SubmitMapForReviewCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để gửi bản đồ để xem xét.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable().FirstOrDefaultAsync(m => m.Id == command.GameId && !m.IsDeleted, cancellationToken);
        if (game == null)
            return Result.Failure($"Không tìm thấy bản đồ có Id: {command.GameId}. Bản đồ có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (game.CreatedBy != userId)
            return Result.Failure("Bạn chỉ có thể gửi bản đồ của riêng mình để xem xét. Bản đồ này được tạo bởi một người dùng khác.", ErrorCodeEnum.Forbidden);
        if (game.GameStatus != GameStatusEnum.Draft)
            return Result.Failure($"Bản đồ không thể được gửi để xem xét. Trạng thái dự kiến: Bản nháp. Trạng thái hiện tại: {game.GameStatus}. Chỉ có thể gửi bản đồ dự thảo.", ErrorCodeEnum.InvalidOperation);

        var rootGameId = game.RootGameId ?? game.Id;
        if (!game.RootGameId.HasValue)
            game.RootGameId = rootGameId;
        var hasPendingSameLine = await mapRepo.GetQueryable().AnyAsync(
            m => !m.IsDeleted
                 && m.Id != game.Id
                 && (m.RootGameId ?? m.Id) == rootGameId
                 && m.GameStatus == GameStatusEnum.PendingReview,
            cancellationToken);
        if (hasPendingSameLine)
            return Result.Failure(
                "Đã có một version khác đang chờ duyệt trong game line này. Vui lòng xử lý version đó trước.",
                ErrorCodeEnum.InvalidOperation);

        game.GameStatus = GameStatusEnum.PendingReview;
        game.UpdateEntity(userId);
        mapRepo.Update(game);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Bản đồ đã được gửi để xem xét thành công.");
    }
}
