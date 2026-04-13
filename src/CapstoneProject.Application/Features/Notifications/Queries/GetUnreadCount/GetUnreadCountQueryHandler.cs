using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<GetUnreadCountResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUnreadCountQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GetUnreadCountResponse>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result<GetUnreadCountResponse>.Failure("Người dùng chưa được xác thực", ErrorCodeEnum.Unauthorized);

        var count = await _unitOfWork.Repository<UserNotification>().GetQueryable()
            .Where(x => x.UserId == userId && !x.IsDeleted && !x.IsRead)
            .CountAsync(cancellationToken);

        var response = new GetUnreadCountResponse { UnreadCount = count };
        return Result<GetUnreadCountResponse>.Success(response, "Đã lấy số lượng thông báo chưa đọc thành công");
    }
}

