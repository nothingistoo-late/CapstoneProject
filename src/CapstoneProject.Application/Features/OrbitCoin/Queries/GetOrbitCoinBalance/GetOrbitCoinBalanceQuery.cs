using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinBalance;

public record GetOrbitCoinBalanceQuery(Guid? UserId = null) : IRequest<Result<OrbitCoinBalanceDto>>;
