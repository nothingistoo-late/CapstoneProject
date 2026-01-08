using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Commons.DTOs.User;

public class UserFilter : BasePaginationFilter
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public DateTime? JoiningFrom { get; set; }
    public DateTime? JoiningTo { get; set; }
}
