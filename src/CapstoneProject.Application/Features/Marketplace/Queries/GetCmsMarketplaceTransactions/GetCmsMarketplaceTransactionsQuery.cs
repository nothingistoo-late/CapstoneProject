using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsMarketplaceTransactions;

public record GetCmsMarketplaceTransactionsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? From = null,
    DateTime? To = null,
    string? PaymentStatus = null,
    string? Search = null
) : IRequest<Result<PaginationResult<CmsMarketplaceTransactionItemDto>>>;

public class CmsMarketplaceTransactionItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid? GameId { get; set; }
    public Guid? PackageId { get; set; }
    public decimal Amount { get; set; }
    public long AmountVnd { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class GetCmsMarketplaceTransactionsQueryHandler : IRequestHandler<GetCmsMarketplaceTransactionsQuery, Result<PaginationResult<CmsMarketplaceTransactionItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCmsMarketplaceTransactionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<CmsMarketplaceTransactionItemDto>>> Handle(GetCmsMarketplaceTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaginationResult<CmsMarketplaceTransactionItemDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<PaginationResult<CmsMarketplaceTransactionItemDto>>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var from = request.From.HasValue ? NormalizeTimestamp(request.From.Value) : (DateTime?)null;
        var to = request.To.HasValue ? NormalizeTimestamp(request.To.Value) : (DateTime?)null;

        var query = _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .Where(pr => !pr.IsDeleted);

        if (from.HasValue)
            query = query.Where(pr => pr.PaidAt != null && pr.PaidAt >= from.Value);

        if (to.HasValue)
            query = query.Where(pr => pr.PaidAt != null && pr.PaidAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) && Enum.TryParse<PaymentStatusEnum>(request.PaymentStatus, true, out var parsedStatus))
            query = query.Where(pr => pr.PaymentStatus == parsedStatus);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLowerInvariant();
            query = query.Where(pr =>
                (pr.ExternalId != null && pr.ExternalId.ToLower().Contains(keyword))
                || (pr.GameId != null && pr.GameId.ToString().Contains(keyword))
                || (pr.PackageId != null && pr.PackageId.ToString().Contains(keyword))
                || pr.UserId.ToString().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(pr => pr.PaidAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(pr => new
            {
                pr.Id,
                pr.UserId,
                pr.GameId,
                pr.PackageId,
                pr.Amount,
                AmountVnd = pr.AmountVnd ?? 0,
                PaymentStatus = pr.PaymentStatus.ToString(),
                pr.ExternalId,
                pr.PaidAt
            })
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

            return new CmsMarketplaceTransactionItemDto
            {
                Id = row.Id,
                UserId = row.UserId,
                UserName = string.IsNullOrWhiteSpace(fullName) ? row.UserId.ToString() : fullName,
                UserEmail = user?.Email ?? string.Empty,
                GameId = row.GameId,
                PackageId = row.PackageId,
                Amount = row.Amount,
                AmountVnd = row.AmountVnd,
                PaymentStatus = row.PaymentStatus,
                ExternalId = row.ExternalId,
                PaidAt = row.PaidAt
            };
        }).ToList();

        var paginated = PaginationResult<CmsMarketplaceTransactionItemDto>.Success(items, pageNumber, pageSize, totalItems);
        return Result<PaginationResult<CmsMarketplaceTransactionItemDto>>.Success(paginated, "Đã lấy danh sách giao dịch marketplace.");
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
}
