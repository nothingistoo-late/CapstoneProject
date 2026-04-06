using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Complaints.Commands.UpsertComplaintCategoryConfig;

public record UpsertComplaintCategoryConfigCommand(
    string CategoryKey,
    string DisplayName,
    string? Description,
    bool IsEnabled,
    int SortOrder) : IRequest<Result<UpsertComplaintCategoryConfigDto>>;

public class UpsertComplaintCategoryConfigDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
}
