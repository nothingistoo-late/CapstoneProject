using System.Text.Json.Serialization;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Auth.Queries.GetProfile;

public class ProfileResponse
{
    
    public string Email { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
   
    public string LastName { get; set; } = null!;

    public string? PhoneNumber { get; set; }
    
    public string? Gender { get; set; }
   
    public DateTime? DateOfBirth { get; set; }
    
    public string? Bio { get; set; }

    public string? AvatarPath { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LearnerCode { get; set; }

}
