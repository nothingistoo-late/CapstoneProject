using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRateHistory;

public class GetExchangeRateHistoryQueryHandler : IRequestHandler<GetExchangeRateHistoryQuery, Result<List<ExchangeRateDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExchangeRateHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ExchangeRateDto>>> Handle(GetExchangeRateHistoryQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Max(1, Math.Min(100, request.Take));

        var history = await _unitOfWork.Repository<ExchangeRate>()
            .GetQueryable()
            .Where(er => er.FromCurrency == request.FromCurrency
                && er.ToCurrency == request.ToCurrency
                && !er.IsDeleted)
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .Take(take)
            .Select(er => new ExchangeRateDto
            {
                Id = er.Id,
                FromCurrency = er.FromCurrency,
                ToCurrency = er.ToCurrency,
                Rate = er.Rate,
                EffectiveFrom = er.EffectiveFrom,
                EffectiveTo = er.EffectiveTo,
                IsActive = er.IsActive,
                Reason = er.Reason,
                CreatedAt = er.CreatedAt,
                UpdatedAt = er.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return Result<List<ExchangeRateDto>>.Success(history, "Exchange rate history retrieved successfully.");
    }
}
