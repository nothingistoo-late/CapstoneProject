using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Complaints.Commands.SendComplaintMessage;

public record SendComplaintMessageCommand(
    Guid ComplaintId,
    string Content,
    IReadOnlyCollection<IFormFile>? Attachments = null) : IRequest<Result<ComplaintMessagePostedDto>>;

