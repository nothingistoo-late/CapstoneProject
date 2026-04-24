using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinTransactionHistory;

public record GetOrbitCoinTransactionHistoryQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Direction = null,
    List<CoinTransactionTypeEnum>? Categories = null,
    string? RelatedEntityType = null,
    Guid? RelatedEntityId = null,
    DateTime? From = null,
    DateTime? To = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    string? Status = null,
    List<string>? Statuses = null,
    string? Search = null,
    Guid? UserId = null
) : IRequest<Result<OrbitCoinTransactionHistoryResult>>;

public class OrbitCoinTransactionHistoryResult
{
    public IReadOnlyList<OrbitCoinTransactionDto> Items { get; set; } = new List<OrbitCoinTransactionDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IReadOnlyList<string> AvailableStatuses { get; set; } = Array.Empty<string>();
}
