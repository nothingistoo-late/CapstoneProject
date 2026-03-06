using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Commands.CreditOrbitCoin;

/// <summary>
/// Credit user's OrbitCoin (e.g. when user deposits real money). Admin only.
/// </summary>
public record CreditOrbitCoinCommand(
    Guid UserId,
    decimal Amount,
    string? Note = null,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null
) : IRequest<Result>;
