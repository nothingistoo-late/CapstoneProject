using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetWalletDashboardGames;

public class GetWalletDashboardGamesQueryHandler : IRequestHandler<GetWalletDashboardGamesQuery, Result<WalletDashboardGamesResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetWalletDashboardGamesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<WalletDashboardGamesResultDto>> Handle(GetWalletDashboardGamesQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<WalletDashboardGamesResultDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var fromDate = request.From ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.To ?? DateTime.UtcNow;
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var exchangeRate = await _unitOfWork.Repository<ExchangeRate>().GetQueryable()
            .Where(er => !er.IsDeleted && er.IsActive && er.FromCurrency == "OrbitCoin" && er.ToCurrency == "VND")
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .Select(er => (decimal?)er.Rate)
            .FirstOrDefaultAsync(cancellationToken) ?? 1m;

        var myGames = _unitOfWork.Repository<Game>().GetQueryable()
            .Where(g => !g.IsDeleted && g.CreatedBy == userId.Value);

        var rows = await (
            from g in myGames
            join p in _unitOfWork.Repository<PaymentRecord>().GetQueryable()
                on g.Id equals p.GameId
            where !p.IsDeleted
                  && p.PaidAt.HasValue
                  && p.PaidAt >= fromDate
                  && p.PaidAt <= toDate
            group p by new { g.Id, g.Title } into grp
            select new WalletDashboardGameItemDto
            {
                GameId = grp.Key.Id,
                GameTitle = grp.Key.Title,
                BuyersCount = grp.Where(x => x.PaymentStatus == PaymentStatusEnum.Completed).Select(x => x.UserId).Distinct().Count(),
                OrdersCount = grp.Count(x => x.PaymentStatus == PaymentStatusEnum.Completed),
                PendingOrdersCount = grp.Count(x => x.PaymentStatus == PaymentStatusEnum.Pending),
                RefundedOrdersCount = grp.Count(x => x.PaymentStatus == PaymentStatusEnum.Refunded),
                Gross = grp.Where(x => x.PaymentStatus == PaymentStatusEnum.Completed).Sum(x => x.AmountVnd.HasValue ? x.AmountVnd.Value : (long)Math.Round(x.Amount * exchangeRate)),
                LastSoldAt = grp.Where(x => x.PaymentStatus == PaymentStatusEnum.Completed).Max(x => x.PaidAt)
            })
            .OrderByDescending(x => x.Gross)
            .ToListAsync(cancellationToken);

        foreach (var item in rows)
        {
            item.Fee = Math.Round(item.Gross * 0.05m, 4);
            item.Net = item.Gross - item.Fee;
            item.AverageOrderValue = item.OrdersCount > 0 ? Math.Round(item.Gross / item.OrdersCount, 4) : 0m;
        }

        var totalCount = rows.Count;
        var items = rows.Skip((page - 1) * size).Take(size).ToList();

        return Result<WalletDashboardGamesResultDto>.Success(new WalletDashboardGamesResultDto
        {
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = size,
            Items = items
        }, "Lấy phân tích doanh thu game thành công.");
    }
}
