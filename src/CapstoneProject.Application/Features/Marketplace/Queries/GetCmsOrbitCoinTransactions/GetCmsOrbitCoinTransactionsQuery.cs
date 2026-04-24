using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsOrbitCoinTransactions;

public record GetCmsOrbitCoinTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? From = null,
    DateTime? To = null,
    string? TransactionType = null,
    string? Search = null
) : IRequest<Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>>;

public class CmsOrbitCoinTransactionItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetCmsOrbitCoinTransactionsQueryHandler : IRequestHandler<GetCmsOrbitCoinTransactionsQuery, Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCmsOrbitCoinTransactionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>> Handle(GetCmsOrbitCoinTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(Domain.Enums.RoleEnum.Admin))
            return Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var from = request.From.HasValue ? NormalizeTimestamp(request.From.Value) : (DateTime?)null;
        var to = request.To.HasValue ? NormalizeTimestamp(request.To.Value) : (DateTime?)null;

        var query = _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .AsNoTracking();

        if (from.HasValue)
            query = query.Where(tx => tx.PaidAt != null && tx.PaidAt >= from.Value);

        if (to.HasValue)
            query = query.Where(tx => tx.PaidAt != null && tx.PaidAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLowerInvariant();
            query = query.Where(tx =>
                tx.UserId.ToString().Contains(keyword)
                || (tx.ExternalId != null && tx.ExternalId.ToLower().Contains(keyword))
                || (tx.GameId != null && tx.GameId.ToString()!.Contains(keyword))
                || (tx.PackageId != null && tx.PackageId.ToString()!.Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(tx => tx.PaidAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = rows.Select(x => x.UserId).Distinct().ToList();
        var users = await _unitOfWork.Repository<AppUser>()
            .GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var items = rows.Select(row =>
        {
            users.TryGetValue(row.UserId, out var user);
            var fullName = user == null ? row.UserId.ToString() : $"{user.FirstName} {user.LastName}".Trim();
            var txType = row.GameId != null || row.PackageId != null ? "Debit" : "Credit";
            return new CmsOrbitCoinTransactionItemDto
            {
                Id = row.Id,
                UserId = row.UserId,
                UserName = string.IsNullOrWhiteSpace(fullName) ? row.UserId.ToString() : fullName,
                UserEmail = user?.Email ?? string.Empty,
                Amount = row.Amount,
                FeeAmount = 0,
                BalanceAfter = null,
                TransactionType = txType,
                RelatedEntityType = row.PackageId != null ? "Package" : (row.GameId != null ? "Game" : "Other"),
                RelatedEntityId = row.PackageId ?? row.GameId,
                Note = row.ExternalId,
                CreatedAt = row.PaidAt ?? DateTime.UtcNow
            };
        }).ToList();

        var paginated = PaginationResult<CmsOrbitCoinTransactionItemDto>.Success(items, pageNumber, pageSize, totalItems);
        return Result<PaginationResult<CmsOrbitCoinTransactionItemDto>>.Success(paginated, "Đã lấy danh sách giao dịch ví OrbitCoin.");
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
}
