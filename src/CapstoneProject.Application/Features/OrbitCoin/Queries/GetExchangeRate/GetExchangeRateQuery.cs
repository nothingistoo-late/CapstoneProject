using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetExchangeRate;

public record GetExchangeRateQuery(string FromCurrency = "OrbitCoin", string ToCurrency = "VND") : IRequest<Result<ExchangeRateDto>>;
