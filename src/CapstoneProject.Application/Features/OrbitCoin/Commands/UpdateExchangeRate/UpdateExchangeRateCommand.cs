using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.UpdateExchangeRate;

public record UpdateExchangeRateCommand(
    decimal Rate,
    string? Reason = null,
    DateTime? EffectiveFrom = null,
    DateTime? EffectiveTo = null,
    string FromCurrency = "OrbitCoin",
    string ToCurrency = "VND"
) : IRequest<Result<ExchangeRateDto>>;
