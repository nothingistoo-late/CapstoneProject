using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.DeleteComplaintCategoryConfig;

public record DeleteComplaintCategoryConfigCommand(string CategoryKey) : IRequest<Result>;
