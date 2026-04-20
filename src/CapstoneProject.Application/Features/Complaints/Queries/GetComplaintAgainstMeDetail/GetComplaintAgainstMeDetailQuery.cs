using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintDetail;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetComplaintAgainstMeDetail;

public record GetComplaintAgainstMeDetailQuery(Guid ComplaintId) : IRequest<Result<ComplaintDetailDto>>;
