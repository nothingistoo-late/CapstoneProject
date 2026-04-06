using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessageAsStaff;

public record SendComplaintMessageAsStaffCommand(
    Guid ComplaintId,
    string Content,
    bool IsInternal = false,
    IReadOnlyCollection<IFormFile>? Attachments = null) : IRequest<Result<ComplaintMessagePostedDto>>;

