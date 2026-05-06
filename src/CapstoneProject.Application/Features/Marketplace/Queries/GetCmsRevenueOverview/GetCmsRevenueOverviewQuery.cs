using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetCmsRevenueOverview;

public record GetCmsRevenueOverviewQuery(DateTime? From = null, DateTime? To = null, string GroupBy = "Day")
    : IRequest<Result<CmsRevenueOverviewDto>>;

public class CmsRevenueOverviewDto
{
    public long GrossRevenueVnd { get; set; }
    public long PlatformFeeVnd { get; set; }
    public long CreatorPayoutVnd { get; set; }
    public long NetPlatformRevenueVnd { get; set; }
    public int TotalTransactions { get; set; }
    public List<CmsRevenuePointDto> Trend { get; set; } = new();
}

public class CmsRevenuePointDto
{
    public string Period { get; set; } = string.Empty;
    public long GrossRevenueVnd { get; set; }
    public long PlatformFeeVnd { get; set; }
    public long NetPlatformRevenueVnd { get; set; }
    public int TransactionCount { get; set; }
}

public class GetCmsRevenueOverviewQueryHandler : IRequestHandler<GetCmsRevenueOverviewQuery, Result<CmsRevenueOverviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private const decimal PlatformFeeRate = 0.05m;

    public GetCmsRevenueOverviewQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CmsRevenueOverviewDto>> Handle(GetCmsRevenueOverviewQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<CmsRevenueOverviewDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<CmsRevenueOverviewDto>.Failure("Chỉ quản trị viên mới có thể truy cập.", ErrorCodeEnum.Forbidden);

        var now = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        var from = NormalizeTimestamp(request.From ?? now.AddDays(-30));
        var to = NormalizeTimestamp(request.To ?? now);

        var exchangeRate = await _unitOfWork.Repository<ExchangeRate>().GetQueryable()
            .Where(er => !er.IsDeleted && er.IsActive && er.FromCurrency == "OrbitCoin" && er.ToCurrency == "VND")
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .Select(er => (decimal?)er.Rate)
            .FirstOrDefaultAsync(cancellationToken) ?? 1m;

        var query = _unitOfWork.Repository<PaymentRecord>()
            .GetQueryable()
            .Where(pr => !pr.IsDeleted
                && pr.PaymentStatus == PaymentStatusEnum.Completed
                && pr.PaidAt != null
                && pr.PaidAt >= from
                && pr.PaidAt <= to);

        var data = await query
            .Select(pr => new
            {
                PaidAt = pr.PaidAt!.Value,
                pr.Amount,
                pr.AmountVnd,
                IsPackage = pr.PackageId != null
            })
            .ToListAsync(cancellationToken);

        var normalized = data.Select(x => new
        {
            x.PaidAt,
            AmountVnd = x.AmountVnd ?? (long)Math.Round(x.Amount * exchangeRate, MidpointRounding.AwayFromZero),
            x.IsPackage
        }).ToList();

        var grossRevenue = normalized.Sum(x => x.AmountVnd);
        var gameRevenue = normalized.Where(x => !x.IsPackage).Sum(x => x.AmountVnd);
        var packageRevenue = normalized.Where(x => x.IsPackage).Sum(x => x.AmountVnd);
        var platformFee = (long)Math.Round(gameRevenue * PlatformFeeRate, MidpointRounding.AwayFromZero);
        var creatorPayout = gameRevenue - platformFee;
        var netPlatformRevenue = platformFee + packageRevenue;

        var trend = BuildTrend(normalized.Select(x => new RevenueRawRow(x.PaidAt, x.AmountVnd, x.IsPackage)).ToList(), request.GroupBy);
        var result = new CmsRevenueOverviewDto
        {
            GrossRevenueVnd = grossRevenue,
            PlatformFeeVnd = platformFee,
            CreatorPayoutVnd = creatorPayout,
            NetPlatformRevenueVnd = netPlatformRevenue,
            TotalTransactions = normalized.Count,
            Trend = trend
        };

        return Result<CmsRevenueOverviewDto>.Success(result, "Đã lấy tổng quan doanh thu.");
    }

    private sealed record RevenueRawRow(DateTime PaidAt, long AmountVnd, bool IsPackage);

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);

    private static List<CmsRevenuePointDto> BuildTrend(IEnumerable<RevenueRawRow> rows, string groupBy)
    {
        return groupBy.ToLowerInvariant() switch
        {
            "year" => rows.GroupBy(r => r.PaidAt.Year)
                .OrderBy(g => g.Key)
                .Select(g => BuildPoint(g.Key.ToString(), g.Sum(x => x.AmountVnd), g.Count(), g.Where(x => x.IsPackage).Sum(x => x.AmountVnd)))
                .ToList(),
            "month" => rows.GroupBy(r => new { r.PaidAt.Year, r.PaidAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => BuildPoint($"{g.Key.Year}-{g.Key.Month:D2}", g.Sum(x => x.AmountVnd), g.Count(), g.Where(x => x.IsPackage).Sum(x => x.AmountVnd)))
                .ToList(),
            "week" => rows.GroupBy(r =>
                {
                    var date = DateOnly.FromDateTime(r.PaidAt);
                    var dayOfWeek = (int)date.DayOfWeek;
                    var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                    return date.AddDays(-diff);
                })
                .OrderBy(g => g.Key)
                .Select(g => BuildPoint(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.AmountVnd), g.Count(), g.Where(x => x.IsPackage).Sum(x => x.AmountVnd)))
                .ToList(),
            _ => rows.GroupBy(r => DateOnly.FromDateTime(r.PaidAt))
                .OrderBy(g => g.Key)
                .Select(g => BuildPoint(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.AmountVnd), g.Count(), g.Where(x => x.IsPackage).Sum(x => x.AmountVnd)))
                .ToList()
        };
    }

    private static CmsRevenuePointDto BuildPoint(string period, long grossRevenue, int transactionCount, long packageRevenue)
    {
        var gameRevenue = grossRevenue - packageRevenue;
        var fee = (long)Math.Round(gameRevenue * PlatformFeeRate, MidpointRounding.AwayFromZero);
        var netPlatformRevenue = fee + packageRevenue;
        return new CmsRevenuePointDto
        {
            Period = period,
            GrossRevenueVnd = grossRevenue,
            PlatformFeeVnd = fee,
            NetPlatformRevenueVnd = netPlatformRevenue,
            TransactionCount = transactionCount
        };
    }
}
