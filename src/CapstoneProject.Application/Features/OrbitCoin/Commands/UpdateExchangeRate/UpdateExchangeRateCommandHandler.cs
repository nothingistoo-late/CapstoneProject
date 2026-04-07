using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, Result<ExchangeRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateExchangeRateCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ExchangeRateDto>> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        // Verify admin user
        var (isValid, adminIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !adminIdNullable.HasValue)
            return Result<ExchangeRateDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);
        var adminId = adminIdNullable.Value;

        // Validate rate
        if (request.Rate <= 0)
            return Result<ExchangeRateDto>.Failure("Tỷ giá hối đoái phải dương.", ErrorCodeEnum.ValidationFailed);

        var now = VietnamDateTime.DbNow;
        var normalizedEffectiveFrom = VietnamDateTime.ToDbDateTime(request.EffectiveFrom) ?? now;
        var normalizedEffectiveTo = VietnamDateTime.ToDbDateTime(request.EffectiveTo);

        if (normalizedEffectiveTo.HasValue && normalizedEffectiveTo.Value < normalizedEffectiveFrom)
            return Result<ExchangeRateDto>.Failure("Hiệu quảTo phải lớn hơn hoặc bằng Hiệu quảTừ.", ErrorCodeEnum.ValidationFailed);

        // Find existing active rate
        var existingRate = await _unitOfWork.Repository<ExchangeRate>()
            .GetQueryable()
            .Where(er => er.FromCurrency == request.FromCurrency
                && er.ToCurrency == request.ToCurrency
                && er.IsActive
                && !er.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingRate != null)
        {
            // Archive previous active rate for audit timeline.
            existingRate.IsActive = false;
            existingRate.EffectiveTo = now;
            existingRate.UpdatedAt = now;
            existingRate.UpdatedBy = adminId;
            _unitOfWork.Repository<ExchangeRate>().Update(existingRate);
        }

        // Always insert a new active row to preserve full change history.
        var newRate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            FromCurrency = request.FromCurrency,
            ToCurrency = request.ToCurrency,
            Rate = request.Rate,
            EffectiveFrom = normalizedEffectiveFrom,
            EffectiveTo = normalizedEffectiveTo,
            IsActive = true,
            Reason = request.Reason,
            CreatedAt = now,
            CreatedBy = adminId,
            UpdatedAt = now,
            UpdatedBy = adminId,
            Status = EntityStatusEnum.Active,
        };

        await _unitOfWork.Repository<ExchangeRate>().AddAsync(newRate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var dto = new ExchangeRateDto
        {
            Id = newRate.Id,
            FromCurrency = newRate.FromCurrency,
            ToCurrency = newRate.ToCurrency,
            Rate = newRate.Rate,
            EffectiveFrom = newRate.EffectiveFrom,
            EffectiveTo = newRate.EffectiveTo,
            IsActive = newRate.IsActive,
            Reason = newRate.Reason,
            CreatedAt = newRate.CreatedAt,
            UpdatedAt = newRate.UpdatedAt,
        };

        return Result<ExchangeRateDto>.Success(dto, "Tỷ giá hối đoái được cập nhật thành công.");
    }
}
