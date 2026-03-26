using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;

public record SendComplaintMessageAsStaffCommand(
    Guid ComplaintId,
    string Content,
    bool IsInternal = false) : IRequest<Result<Guid>>;

