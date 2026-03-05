using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Common.DTOs.Auth;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public GenderEnum? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
}
