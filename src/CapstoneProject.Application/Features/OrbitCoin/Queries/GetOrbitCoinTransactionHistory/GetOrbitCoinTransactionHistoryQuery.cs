using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using MediatR;

namespace CapstoneProject.Application.Features.OrbitCoin.Queries.GetOrbitCoinTransactionHistory;

public record GetOrbitCoinTransactionHistoryQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? UserId = null
) : IRequest<Result<OrbitCoinTransactionHistoryResult>>;

public class OrbitCoinTransactionHistoryResult
{
    public IReadOnlyList<OrbitCoinTransactionDto> Items { get; set; } = new List<OrbitCoinTransactionDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
