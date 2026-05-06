using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsEscrowPendingTransactions;

public class GetCmsEscrowPendingTransactionsQueryHandler : IRequestHandler<GetCmsEscrowPendingTransactionsQuery, Result<CmsEscrowPendingResultDto>>
{
    private const decimal PlatformFeeRate = 0.05m;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCmsEscrowPendingTransactionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CmsEscrowPendingResultDto>> Handle(GetCmsEscrowPendingTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<CmsEscrowPendingResultDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<CmsEscrowPendingResultDto>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var from = request.From.HasValue ? NormalizeTimestamp(request.From.Value) : (DateTime?)null;
        var to = request.To.HasValue ? NormalizeTimestamp(request.To.Value) : (DateTime?)null;

        var payments = _unitOfWork.Repository<PaymentRecord>().GetQueryable().AsNoTracking();
        var games = _unitOfWork.Repository<Game>().GetQueryable().AsNoTracking();
        var users = _unitOfWork.Repository<AppUser>().GetQueryable().AsNoTracking();

        var baseQuery = from pr in payments
                        join g in games on pr.GameId equals g.Id
                        join buyer in users on pr.UserId equals buyer.Id
                        join seller in users on g.CreatedBy equals seller.Id
                        where !pr.IsDeleted
                              && pr.GameId.HasValue
                              && pr.PaymentStatus == PaymentStatusEnum.Pending
                              && !g.IsDeleted
                        select new
                        {
                            Payment = pr,
                            Game = g,
                            Buyer = buyer,
                            Seller = seller
                        };

        var totalPendingAmount = await baseQuery
            .SumAsync(x => (decimal?)x.Payment.Amount, cancellationToken) ?? 0m;

        var query = baseQuery;

        if (from.HasValue)
            query = query.Where(x => x.Payment.PaidAt.HasValue && x.Payment.PaidAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.Payment.PaidAt.HasValue && x.Payment.PaidAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Payment.Id.ToString().ToLower().Contains(keyword)
                || x.Payment.UserId.ToString().ToLower().Contains(keyword)
                || x.Game.Id.ToString().ToLower().Contains(keyword)
                || x.Game.Title.ToLower().Contains(keyword)
                || (x.Buyer.Email != null && x.Buyer.Email.ToLower().Contains(keyword))
                || (x.Buyer.UserName != null && x.Buyer.UserName.ToLower().Contains(keyword))
                || ((x.Buyer.FirstName + " " + x.Buyer.LastName).ToLower().Contains(keyword))
                || (x.Seller.Email != null && x.Seller.Email.ToLower().Contains(keyword))
                || (x.Seller.UserName != null && x.Seller.UserName.ToLower().Contains(keyword))
                || ((x.Seller.FirstName + " " + x.Seller.LastName).ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(x => x.Payment.PaidAt ?? x.Payment.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                PaymentRecordId = x.Payment.Id,
                x.Payment.UserId,
                x.Payment.Amount,
                x.Payment.PaidAt,
                GameId = x.Game.Id,
                x.Game.Title,
                BuyerFirstName = x.Buyer.FirstName,
                BuyerLastName = x.Buyer.LastName,
                BuyerUserName = x.Buyer.UserName,
                BuyerEmail = x.Buyer.Email,
                SellerId = x.Seller.Id,
                SellerFirstName = x.Seller.FirstName,
                SellerLastName = x.Seller.LastName,
                SellerUserName = x.Seller.UserName,
                SellerEmail = x.Seller.Email
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row =>
        {
            var buyerFullName = $"{row.BuyerFirstName} {row.BuyerLastName}".Trim();
            var buyerName = string.IsNullOrWhiteSpace(buyerFullName)
                ? (row.BuyerUserName ?? row.UserId.ToString())
                : buyerFullName;
            var sellerFullName = $"{row.SellerFirstName} {row.SellerLastName}".Trim();
            var sellerName = string.IsNullOrWhiteSpace(sellerFullName)
                ? (row.SellerUserName ?? row.SellerId.ToString())
                : sellerFullName;
            var feeAmount = Math.Round(row.Amount * PlatformFeeRate, 4, MidpointRounding.AwayFromZero);
            var sellerReceives = row.Amount - feeAmount;

            return new CmsEscrowPendingTransactionDto
            {
                PaymentRecordId = row.PaymentRecordId,
                GameId = row.GameId,
                GameTitle = row.Title,
                BuyerId = row.UserId,
                BuyerName = buyerName,
                BuyerEmail = row.BuyerEmail ?? string.Empty,
                SellerId = row.SellerId,
                SellerName = sellerName,
                SellerEmail = row.SellerEmail ?? string.Empty,
                Amount = row.Amount,
                FeeAmount = feeAmount,
                SellerReceives = sellerReceives,
                PaidAt = row.PaidAt,
                PaymentStatus = PaymentStatusEnum.Pending.ToString()
            };
        }).ToList();

        var paginated = PaginationResult<CmsEscrowPendingTransactionDto>.Success(items, pageNumber, pageSize, totalItems, "Đã lấy danh sách giao dịch escrow đang chờ.");
        var result = new CmsEscrowPendingResultDto
        {
            TotalPendingAmount = totalPendingAmount,
            Transactions = paginated
        };

        return Result<CmsEscrowPendingResultDto>.Success(result, "Đã lấy danh sách giao dịch escrow đang chờ.");
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
}
