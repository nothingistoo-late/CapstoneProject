using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRateHistory;

public record GetExchangeRateHistoryQuery(
    string FromCurrency = "OrbitCoin",
    string ToCurrency = "VND",
    int Take = 20
) : IRequest<Result<List<ExchangeRateDto>>>;
