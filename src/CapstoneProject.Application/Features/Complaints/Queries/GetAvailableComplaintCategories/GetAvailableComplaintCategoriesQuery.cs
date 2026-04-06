using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Complaints.Queries.GetComplaintCategoryConfigs;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Queries.GetAvailableComplaintCategories;

public record GetAvailableComplaintCategoriesQuery() : IRequest<Result<List<ComplaintCategoryConfigDto>>>;
