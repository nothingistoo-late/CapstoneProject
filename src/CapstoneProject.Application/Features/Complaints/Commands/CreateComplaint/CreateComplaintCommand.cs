using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Complaints;
using CapstoneProject.Application.Commons.Models.Complaints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;

public record CreateComplaintCommand(
    string Subject,
    string CategoryKey,
    string Description,
    ComplaintCreateContextInput Context,
    IReadOnlyCollection<IFormFile>? Attachments) : IRequest<Result<CreateComplaintResponseDto>>;

public class CreateComplaintResponseDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComplaintStatus { get; set; } = string.Empty;
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? ContextKey { get; set; }
    public string? ContextDataJson { get; set; }
    public DateTime? OccurredAt { get; set; }
    public ComplaintContextResolvedDto? ContextResolved { get; set; }
    public Guid InitialMessageId { get; set; }
    public List<ComplaintAttachmentDto> Attachments { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}

