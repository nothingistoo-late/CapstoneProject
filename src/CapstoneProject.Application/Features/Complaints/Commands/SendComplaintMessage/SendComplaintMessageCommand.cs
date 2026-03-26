using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;

public record SendComplaintMessageCommand(
    Guid ComplaintId,
    string Content) : IRequest<Result<Guid>>;

