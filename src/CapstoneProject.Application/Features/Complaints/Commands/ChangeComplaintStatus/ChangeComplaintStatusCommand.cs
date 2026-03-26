using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.ChangeComplaintStatus;

public record ChangeComplaintStatusCommand(
    Guid ComplaintId,
    ComplaintStatusEnum ToStatus,
    string? Note = null) : IRequest<Result>;

