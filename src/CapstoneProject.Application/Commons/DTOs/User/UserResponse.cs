using System.Text.Json.Serialization;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.DTOs.User;

public class UserResponse : UserListItem
{
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
}
