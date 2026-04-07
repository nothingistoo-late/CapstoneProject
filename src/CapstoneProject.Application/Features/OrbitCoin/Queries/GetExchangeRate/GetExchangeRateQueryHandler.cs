using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRate;

public class GetExchangeRateQueryHandler : IRequestHandler<GetExchangeRateQuery, Result<ExchangeRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetExchangeRateQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExchangeRateDto>> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _unitOfWork.Repository<ExchangeRate>()
            .GetQueryable()
            .Where(er => er.FromCurrency == request.FromCurrency 
                && er.ToCurrency == request.ToCurrency
                && er.IsActive
                && !er.IsDeleted)
            .OrderByDescending(er => er.CreatedAt ?? DateTime.MinValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (exchangeRate == null)
            return Result<ExchangeRateDto>.Failure(
                $"Không tìm thấy tỷ giá hối đoái cho {request.FromCurrency}/{request.ToCurrency}.", 
                ErrorCodeEnum.NotFound);

        var dto = new ExchangeRateDto
        {
            Id = exchangeRate.Id,
            FromCurrency = exchangeRate.FromCurrency,
            ToCurrency = exchangeRate.ToCurrency,
            Rate = exchangeRate.Rate,
            EffectiveFrom = exchangeRate.EffectiveFrom,
            EffectiveTo = exchangeRate.EffectiveTo,
            IsActive = exchangeRate.IsActive,
            Reason = exchangeRate.Reason,
            CreatedAt = exchangeRate.CreatedAt,
            UpdatedAt = exchangeRate.UpdatedAt,
        };

        return Result<ExchangeRateDto>.Success(dto, "Tỷ giá hối đoái được truy xuất thành công.");
    }
}
