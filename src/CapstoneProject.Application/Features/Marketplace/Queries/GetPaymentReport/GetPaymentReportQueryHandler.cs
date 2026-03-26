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
            return Result<PaymentReportDto>.Failure("Authentication required. Please log in to view payment reports.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin))
            return Result<PaymentReportDto>.Failure("You do not have permission to view payment reports. Only Admin can access this report.", ErrorCodeEnum.Forbidden);

        var from = request.From ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow.AddYears(-1);
        var to = request.To ?? CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
        var query = _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(pr => !pr.IsDeleted && pr.PaymentStatus == PaymentStatusEnum.Completed && pr.PaidAt >= from && pr.PaidAt <= to);

        var totalAmount = await query.SumAsync(pr => pr.Amount, cancellationToken);
        var totalCount = await query.CountAsync(cancellationToken);

        var groupBy = request.GroupBy ?? "Day";
        List<PaymentReportItemDto> items = groupBy.ToLowerInvariant() switch
        {
            "year" => await query.GroupBy(pr => pr.PaidAt!.Value.Year).Select(g => new PaymentReportItemDto { Period = g.Key.ToString(), Amount = g.Sum(pr => pr.Amount), Count = g.Count() }).OrderBy(x => x.Period).ToListAsync(cancellationToken),
            "month" => await query.GroupBy(pr => new { pr.PaidAt!.Value.Year, pr.PaidAt.Value.Month }).Select(g => new PaymentReportItemDto { Period = $"{g.Key.Year}-{g.Key.Month:D2}", Amount = g.Sum(pr => pr.Amount), Count = g.Count() }).OrderBy(x => x.Period).ToListAsync(cancellationToken),
            _ => await query.GroupBy(pr => pr.PaidAt!.Value.Date).Select(g => new PaymentReportItemDto { Period = g.Key.ToString("yyyy-MM-dd"), Amount = g.Sum(pr => pr.Amount), Count = g.Count() }).OrderBy(x => x.Period).ToListAsync(cancellationToken)
        };

        return Result<PaymentReportDto>.Success(new PaymentReportDto { TotalAmount = totalAmount, TotalCount = totalCount, Items = items });
    }
}



