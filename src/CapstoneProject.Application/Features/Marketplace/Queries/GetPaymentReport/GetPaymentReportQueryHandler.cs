using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Marketplace.Queries.GetPaymentReport;

public class GetPaymentReportQueryHandler : IRequestHandler<GetPaymentReportQuery, Result<PaymentReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPaymentReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaymentReportDto>> Handle(GetPaymentReportQuery request, CancellationToken cancellationToken)
    {
        var (isValid, _) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result<PaymentReportDto>.Failure("Yêu cầu xác thực. Vui lòng đăng nhập để xem báo cáo thanh toán.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<PaymentReportDto>.Failure("Bạn không có quyền xem báo cáo thanh toán. Chỉ quản trị viên mới có thể truy cập báo cáo này.", ErrorCodeEnum.Forbidden);

        var from = NormalizeTimestamp(request.From ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow.AddYears(-1));
        var to = NormalizeTimestamp(request.To ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow);
        var query = _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(pr => !pr.IsDeleted && pr.PaymentStatus == PaymentStatusEnum.Completed && pr.PaidAt >= from && pr.PaidAt <= to);

        var totalAmount = await query.SumAsync(pr => pr.Amount, cancellationToken);
        var totalAmountVnd = await query.SumAsync(pr => pr.AmountVnd ?? 0, cancellationToken);
        var totalCount = await query.CountAsync(cancellationToken);

        var groupBy = request.GroupBy ?? "Day";
        List<PaymentReportItemDto> items = groupBy.ToLowerInvariant() switch
        {
            "year" => await BuildYearItemsAsync(query, cancellationToken),
            "month" => await BuildMonthItemsAsync(query, cancellationToken),
            "week" => await BuildWeekItemsAsync(query, cancellationToken),
            _ => await BuildDayItemsAsync(query, cancellationToken)
        };

        return Result<PaymentReportDto>.Success(new PaymentReportDto { TotalAmount = totalAmount, TotalAmountVnd = totalAmountVnd, TotalCount = totalCount, Items = items }, "Đã lấy báo cáo thanh toán.");
    }

    private static async Task<List<PaymentReportItemDto>> BuildYearItemsAsync(IQueryable<PaymentRecord> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(pr => pr.PaidAt!.Value.Year)
            .Select(g => new { Year = g.Key, Amount = g.Sum(pr => pr.Amount), AmountVnd = g.Sum(pr => pr.AmountVnd ?? 0), Count = g.Count() })
            .OrderBy(x => x.Year)
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(x => new PaymentReportItemDto
        {
            Period = x.Year.ToString(),
            Amount = x.Amount,
            AmountVnd = x.AmountVnd,
            Count = x.Count
        });
    }

    private static async Task<List<PaymentReportItemDto>> BuildMonthItemsAsync(IQueryable<PaymentRecord> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(pr => new { pr.PaidAt!.Value.Year, pr.PaidAt.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(pr => pr.Amount), AmountVnd = g.Sum(pr => pr.AmountVnd ?? 0), Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(x => new PaymentReportItemDto
        {
            Period = $"{x.Year}-{x.Month:D2}",
            Amount = x.Amount,
            AmountVnd = x.AmountVnd,
            Count = x.Count
        });
    }

    private static async Task<List<PaymentReportItemDto>> BuildDayItemsAsync(IQueryable<PaymentRecord> query, CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(pr => pr.PaidAt!.Value.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(pr => pr.Amount), AmountVnd = g.Sum(pr => pr.AmountVnd ?? 0), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(x => new PaymentReportItemDto
        {
            Period = x.Date.ToString("yyyy-MM-dd"),
            Amount = x.Amount,
            AmountVnd = x.AmountVnd,
            Count = x.Count
        });
    }

    private static async Task<List<PaymentReportItemDto>> BuildWeekItemsAsync(IQueryable<PaymentRecord> query, CancellationToken cancellationToken)
    {
        var rows = query
            .Where(pr => pr.PaidAt != null)
            .AsEnumerable()
            .GroupBy(pr =>
            {
                var paidDate = DateOnly.FromDateTime(pr.PaidAt!.Value.Date);
                var dayOfWeek = (int)paidDate.DayOfWeek;
                var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                return paidDate.AddDays(-diff);
            })
            .Select(g => new
            {
                WeekStart = g.Key,
                Amount = g.Sum(pr => pr.Amount),
                AmountVnd = g.Sum(pr => pr.AmountVnd ?? 0),
                Count = g.Count()
            })
            .OrderBy(x => x.WeekStart)
            .ToList();

        return await Task.FromResult(rows.ConvertAll(x => new PaymentReportItemDto
        {
            Period = x.WeekStart.ToString("yyyy-MM-dd"),
            Amount = x.Amount,
            AmountVnd = x.AmountVnd,
            Count = x.Count
        }));
    }

    private static DateTime NormalizeTimestamp(DateTime input)
        => input.Kind == DateTimeKind.Unspecified ? input : DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
}

