using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.OrbitCoin;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetDepositOrder;

public record GetDepositOrderQuery(Guid OrderId) : IRequest<Result<DepositOrderDetailDto>>;
