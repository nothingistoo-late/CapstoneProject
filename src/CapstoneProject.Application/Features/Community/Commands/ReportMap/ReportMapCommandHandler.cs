using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Community.Commands.ReportMap;

public class ReportMapCommandHandler : IRequestHandler<ReportMapCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReportMapCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(ReportMapCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để báo cáo trò chơi.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;
        if (string.IsNullOrWhiteSpace(command.Reason))
            return Result<Guid>.Failure("Lý do báo cáo là bắt buộc. Vui lòng cung cấp lý do để báo cáo nội dung này.", ErrorCodeEnum.ValidationFailed);

        var mapRepo = _unitOfWork.Repository<Game>();
        var game = await mapRepo.GetQueryable()
            .FirstOrDefaultAsync(g => g.Id == command.GameId && !g.IsDeleted && g.Status == EntityStatusEnum.Active, cancellationToken);
        if (game == null)
            return Result<Guid>.Failure($"Không tìm thấy trò chơi có Id: {command.GameId}. Trò chơi có thể đã bị xóa hoặc không tồn tại.", ErrorCodeEnum.NotFound);
        if (game.CreatedBy.HasValue && game.CreatedBy.Value == userId)
            return Result<Guid>.Failure("Bạn không thể báo cáo trò chơi của riêng bạn.", ErrorCodeEnum.Forbidden);

        // Only allow reporting games the user can actually play:
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
                return Result<Guid>.Failure("Bạn chỉ có thể báo cáo những trò chơi mà bạn có quyền truy cập (trò chơi miễn phí hoặc trò chơi bạn đã mua).", ErrorCodeEnum.Forbidden);
        }

        var report = new GameReport
        {
            UserId = userId,
            GameId = command.GameId,
            Reason = command.Reason,
            Details = command.Details,
            ReportStatus = ReportStatusEnum.Pending
        };
        report.InitializeEntity(userId);
        await _unitOfWork.Repository<GameReport>().AddAsync(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(report.Id, "Đã gửi báo cáo.");
    }
}
