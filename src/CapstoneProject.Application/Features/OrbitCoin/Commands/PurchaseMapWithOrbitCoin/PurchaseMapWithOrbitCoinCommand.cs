using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.PurchaseMapWithOrbitCoin;

public record PurchaseMapWithOrbitCoinCommand(Guid GameId) : IRequest<Result>;
