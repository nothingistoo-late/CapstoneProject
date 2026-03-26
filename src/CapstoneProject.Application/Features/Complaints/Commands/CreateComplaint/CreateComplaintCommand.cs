using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.CreateComplaint;

public record CreateComplaintCommand(
    string Subject,
    string Category,
    string Description) : IRequest<Result<Guid>>;

