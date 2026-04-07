using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Xp.Queries.GetMyXpHistory;

public class GetMyXpHistoryQueryHandler : IRequestHandler<GetMyXpHistoryQuery, Result<PaginationResult<XpHistoryItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyXpHistoryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<XpHistoryItemDto>>> Handle(GetMyXpHistoryQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<PaginationResult<XpHistoryItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var userId = userIdNullable.Value;

        var query = _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == userId);

        if (request.SourceType.HasValue)
            query = query.Where(x => x.SourceType == request.SourceType.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(x => x.CreatedAt != null && x.CreatedAt <= request.DateTo.Value);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new XpHistoryItemDto
            {
                Id = x.Id,
                Delta = x.Delta,
                Reason = x.Reason,
                SourceType = x.SourceType.ToString(),
                SourceId = x.SourceId,
                IdempotencyKey = x.IdempotencyKey,
                Metadata = x.Metadata,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = PaginationResult<XpHistoryItemDto>.Success(items, pageNumber, pageSize, total);
        return Result<PaginationResult<XpHistoryItemDto>>.Success(page, "Đã lấy lịch sử XP của bạn.");
    }
}


