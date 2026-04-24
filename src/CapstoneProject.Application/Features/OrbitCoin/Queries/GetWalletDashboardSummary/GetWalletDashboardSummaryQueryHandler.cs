using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardSummary;

public class GetWalletDashboardSummaryQueryHandler : IRequestHandler<GetWalletDashboardSummaryQuery, Result<WalletDashboardSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetWalletDashboardSummaryQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<WalletDashboardSummaryDto>> Handle(GetWalletDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<WalletDashboardSummaryDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;
        var role = string.Equals(request.Role, "Creator", StringComparison.OrdinalIgnoreCase) ? "Creator" : "Buyer";
        var monthStart = new DateTime(to.Year, to.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
        var exchangeRate = await _unitOfWork.Repository<ExchangeRate>().GetQueryable()
            .Where(er => !er.IsDeleted && er.IsActive && er.FromCurrency == "OrbitCoin" && er.ToCurrency == "VND")
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .Select(er => (decimal?)er.Rate)
            .FirstOrDefaultAsync(cancellationToken) ?? 1m;

        var walletBalance = await _unitOfWork.Repository<UserWallet>().GetQueryable()
            .Where(w => !w.IsDeleted && w.UserId == userId.Value)
            .Select(w => (decimal?)w.Balance)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        var paymentQuery = _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.UserId == userId.Value
                        && p.PaidAt.HasValue
                        && p.PaidAt >= from
                        && p.PaidAt <= to
                        && p.PaymentStatus != PaymentStatusEnum.Failed
                        && p.PaymentStatus != PaymentStatusEnum.Cancelled);

        var totalIn = await paymentQuery
            .Where(p => !p.PackageId.HasValue && !p.GameId.HasValue && p.PaymentStatus == PaymentStatusEnum.Completed)
            .SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);
        var totalOut = await paymentQuery
            .Where(p => p.PackageId.HasValue || p.GameId.HasValue)
            .SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);
        var totalTransactions = await paymentQuery.CountAsync(cancellationToken);
        var inflowTransactions = await paymentQuery
            .Where(p => !p.PackageId.HasValue && !p.GameId.HasValue && p.PaymentStatus == PaymentStatusEnum.Completed)
            .CountAsync(cancellationToken);
        var outflowTransactions = await paymentQuery
            .Where(p => p.PackageId.HasValue || p.GameId.HasValue)
            .CountAsync(cancellationToken);
        var incomeThisMonth = await paymentQuery
            .Where(p => p.PaidAt >= monthStart && p.PaidAt <= monthEnd
                && !p.PackageId.HasValue && !p.GameId.HasValue
                && p.PaymentStatus == PaymentStatusEnum.Completed)
            .SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);
        var spendingThisMonth = await paymentQuery
            .Where(p => p.PaidAt >= monthStart && p.PaidAt <= monthEnd
                && (p.PackageId.HasValue || p.GameId.HasValue))
            .SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);
        var pendingBalance = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(p => !p.IsDeleted
                        && p.UserId == userId.Value
                        && p.PaymentStatus == PaymentStatusEnum.Pending
                        && p.PaidAt.HasValue
                        && p.PaidAt >= from
                        && p.PaidAt <= to)
            .SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);

        var dto = new WalletDashboardSummaryDto
        {
            Role = role,
            Currency = "VND",
            PeriodFrom = from,
            PeriodTo = to,
            CurrentBalance = Math.Round(walletBalance * exchangeRate, 0),
            CurrentBalanceVnd = Math.Round(walletBalance * exchangeRate, 0),
            CurrentBalanceOc = walletBalance,
            PendingBalance = pendingBalance,
            TotalIn = totalIn,
            TotalOut = totalOut,
            NetFlow = totalIn - totalOut,
            IncomeThisMonth = incomeThisMonth,
            SpendingThisMonth = spendingThisMonth,
            NetProfitThisMonth = incomeThisMonth - spendingThisMonth,
            TotalTransactions = totalTransactions,
            InflowTransactions = inflowTransactions,
            OutflowTransactions = outflowTransactions
        };

        if (role == "Creator")
        {
            var myGameIds = await _unitOfWork.Repository<Game>().GetQueryable()
                .Where(g => !g.IsDeleted && g.CreatedBy == userId.Value)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            if (myGameIds.Count > 0)
            {
                var creatorPayments = _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .Where(p => !p.IsDeleted
                                && p.GameId.HasValue
                                && myGameIds.Contains(p.GameId.Value)
                                && p.PaidAt.HasValue
                                && p.PaidAt >= from
                                && p.PaidAt <= to
                                && p.PaymentStatus == PaymentStatusEnum.Completed);

                dto.UnitsSold = await creatorPayments.CountAsync(cancellationToken);
                dto.UniqueBuyers = await creatorPayments.Select(p => p.UserId).Distinct().CountAsync(cancellationToken);
                dto.GrossRevenue = await creatorPayments.SumAsync(p => p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate), cancellationToken);
                dto.PlatformFee = Math.Round(dto.GrossRevenue * 0.05m, 4);
                dto.NetRevenue = dto.GrossRevenue - dto.PlatformFee;
                dto.AverageOrderValue = dto.UnitsSold > 0 ? Math.Round(dto.GrossRevenue / dto.UnitsSold, 4) : 0m;
            }
        }

        return Result<WalletDashboardSummaryDto>.Success(dto, "Lấy tổng quan ví thành công.");
    }
}
