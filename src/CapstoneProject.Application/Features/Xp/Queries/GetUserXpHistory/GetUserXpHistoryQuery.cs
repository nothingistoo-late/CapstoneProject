using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Xp.Queries.GetMyXpHistory;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Xp.Queries.GetUserXpHistory;

public record GetUserXpHistoryQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20,
    XpSourceTypeEnum? SourceType = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<Result<PaginationResult<XpHistoryItemDto>>>;

