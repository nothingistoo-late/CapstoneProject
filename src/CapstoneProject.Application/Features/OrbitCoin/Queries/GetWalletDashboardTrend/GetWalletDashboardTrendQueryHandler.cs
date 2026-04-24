using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardTrend;

public class GetWalletDashboardTrendQueryHandler : IRequestHandler<GetWalletDashboardTrendQuery, Result<WalletDashboardTrendDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetWalletDashboardTrendQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<WalletDashboardTrendDto>> Handle(GetWalletDashboardTrendQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<WalletDashboardTrendDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;
        var role = string.Equals(request.Role, "Creator", StringComparison.OrdinalIgnoreCase) ? "Creator" : "Buyer";
        var bucket = NormalizeBucket(request.Bucket);
        var exchangeRate = await _unitOfWork.Repository<ExchangeRate>().GetQueryable()
            .Where(er => !er.IsDeleted && er.IsActive && er.FromCurrency == "OrbitCoin" && er.ToCurrency == "VND")
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .Select(er => (decimal?)er.Rate)
            .FirstOrDefaultAsync(cancellationToken) ?? 1m;

        var dto = new WalletDashboardTrendDto
        {
            Role = role,
            Bucket = bucket
        };

        if (role == "Buyer")
        {
            var tx = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(p => !p.IsDeleted
                            && p.UserId == userId.Value
                            && p.PaidAt.HasValue
                            && p.PaidAt >= from
                            && p.PaidAt <= to
                            && p.PaymentStatus != PaymentStatusEnum.Failed
                            && p.PaymentStatus != PaymentStatusEnum.Cancelled)
                .Select(p => new
                {
                    CreatedAt = p.PaidAt!.Value,
                    Amount = !p.PackageId.HasValue && !p.GameId.HasValue
                        ? (p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate))
                        : -(p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate))
                })
                .ToListAsync(cancellationToken);

            dto.Items = tx
                .GroupBy(x => FormatPeriod(x.CreatedAt, bucket))
                .OrderBy(x => x.Key)
                .Select(g =>
                {
                    var inflow = g.Where(x => x.Amount > 0).Sum(x => x.Amount);
                    var outflow = g.Where(x => x.Amount < 0).Sum(x => Math.Abs(x.Amount));
                    return new WalletDashboardTrendItemDto
                    {
                        Period = g.Key,
                        Inflow = inflow,
                        Outflow = outflow,
                        Net = inflow - outflow
                    };
                })
                .ToList();
        }
        else
        {
            var myGameIds = await _unitOfWork.Repository<Game>().GetQueryable()
                .Where(g => !g.IsDeleted && g.CreatedBy == userId.Value)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            if (myGameIds.Count > 0)
            {
                var payments = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                    .Where(p => !p.IsDeleted
                                && p.GameId.HasValue
                                && myGameIds.Contains(p.GameId.Value)
                                && p.PaymentStatus == PaymentStatusEnum.Completed
                                && p.PaidAt.HasValue
                                && p.PaidAt >= from
                                && p.PaidAt <= to)
                    .Select(p => new
                    {
                        PaidAt = p.PaidAt!.Value,
                        Amount = p.AmountVnd.HasValue ? p.AmountVnd.Value : (long)Math.Round(p.Amount * exchangeRate)
                    })
                    .ToListAsync(cancellationToken);

                dto.Items = payments
                    .GroupBy(x => FormatPeriod(x.PaidAt, bucket))
                    .OrderBy(x => x.Key)
                    .Select(g =>
                    {
                        var gross = g.Sum(x => x.Amount);
                        var fee = Math.Round(gross * 0.05m, 4);
                        return new WalletDashboardTrendItemDto
                        {
                            Period = g.Key,
                            GrossRevenue = gross,
                            PlatformFee = fee,
                            NetRevenue = gross - fee
                        };
                    })
                    .ToList();
            }
        }

        return Result<WalletDashboardTrendDto>.Success(dto, "Lấy xu hướng ví thành công.");
    }

    private static string NormalizeBucket(string? bucket)
    {
        if (string.Equals(bucket, "month", StringComparison.OrdinalIgnoreCase)) return "Month";
        if (string.Equals(bucket, "week", StringComparison.OrdinalIgnoreCase)) return "Week";
        return "Day";
    }

    private static string FormatPeriod(DateTime dt, string bucket) => bucket switch
    {
        "Month" => $"{dt.Year}-{dt.Month:D2}",
        "Week" => $"{dt.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(dt):D2}",
        _ => dt.ToString("yyyy-MM-dd")
    };
}
