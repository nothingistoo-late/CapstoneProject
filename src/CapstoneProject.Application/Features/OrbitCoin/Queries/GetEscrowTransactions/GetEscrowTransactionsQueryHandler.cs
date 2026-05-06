using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetEscrowTransactions;

public class GetEscrowTransactionsQueryHandler : IRequestHandler<GetEscrowTransactionsQuery, Result<PaginationResult<EscrowTransactionDto>>>
{
    private const decimal PlatformFeeRate = 0.05m;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetEscrowTransactionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginationResult<EscrowTransactionDto>>> Handle(GetEscrowTransactionsQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<PaginationResult<EscrowTransactionDto>>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var from = request.From.HasValue ? NormalizeTimestamp(request.From.Value) : (DateTime?)null;
        var to = request.To.HasValue ? NormalizeTimestamp(request.To.Value) : (DateTime?)null;

        var payments = _unitOfWork.Repository<PaymentRecord>().GetQueryable().AsNoTracking();
        var games = _unitOfWork.Repository<Game>().GetQueryable().AsNoTracking();
        var users = _unitOfWork.Repository<AppUser>().GetQueryable().AsNoTracking();

        var query = from pr in payments
                    join g in games on pr.GameId equals g.Id
                    join u in users on pr.UserId equals u.Id
                    where !pr.IsDeleted
                          && pr.GameId.HasValue
                          && pr.PaymentStatus == PaymentStatusEnum.Pending
                          && !g.IsDeleted
                          && g.CreatedBy == userId.Value
                    select new
                    {
                        Payment = pr,
                        Game = g,
                        Buyer = u
                    };

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
                || x.Game.Title.ToLower().Contains(keyword)
                || (x.Buyer.Email != null && x.Buyer.Email.ToLower().Contains(keyword))
                || (x.Buyer.UserName != null && x.Buyer.UserName.ToLower().Contains(keyword))
                || ((x.Buyer.FirstName + " " + x.Buyer.LastName).ToLower().Contains(keyword)));
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
                x.Buyer.FirstName,
                x.Buyer.LastName,
                x.Buyer.UserName,
                x.Buyer.Email
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row =>
        {
            var fullName = $"{row.FirstName} {row.LastName}".Trim();
            var buyerName = string.IsNullOrWhiteSpace(fullName)
                ? (row.UserName ?? row.UserId.ToString())
                : fullName;
            var feeAmount = Math.Round(row.Amount * PlatformFeeRate, 4, MidpointRounding.AwayFromZero);
            var sellerReceives = row.Amount - feeAmount;

            return new EscrowTransactionDto
            {
                PaymentRecordId = row.PaymentRecordId,
                GameId = row.GameId,
                GameTitle = row.Title,
                BuyerId = row.UserId,
                BuyerName = buyerName,
                BuyerEmail = row.Email ?? string.Empty,
                Amount = row.Amount,
                FeeAmount = feeAmount,
                SellerReceives = sellerReceives,
                PaidAt = row.PaidAt,
                PaymentStatus = PaymentStatusEnum.Pending.ToString()
            };
        }).ToList();

        var paginated = PaginationResult<EscrowTransactionDto>.Success(items, pageNumber, pageSize, totalItems, "Đã lấy danh sách giao dịch escrow đang chờ.");
        return Result<PaginationResult<EscrowTransactionDto>>.Success(paginated, "Đã lấy danh sách giao dịch escrow đang chờ.");
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
}
