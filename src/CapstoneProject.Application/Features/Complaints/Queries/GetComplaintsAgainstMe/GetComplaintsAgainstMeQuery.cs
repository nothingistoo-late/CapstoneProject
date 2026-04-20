using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaints;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintsAgainstMe;

public record GetComplaintsAgainstMeQuery(
    ComplaintStatusEnum? Status = null,
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? Keyword = null) : IRequest<Result<PaginationResult<ComplaintListItemDto>>>;
