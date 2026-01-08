
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AppUser>> AuthenticateAsync(LoginRequest request);
    Task<IdentityResult> CreateUserAsync(AppUser user, string password);
    Task<IdentityResult> AddUserToRoleAsync(AppUser user, string role);
    Task<Result<IList<string>>> GetUserRolesAsync(AppUser user);
    Task<AppUser> GetUserByIdAsync(string userId);
    Task<AppUser> GetUserByFirstOrDefaultAsync(Expression<Func<AppUser, bool>> predicate);
    Task<Result<bool>> IsEmailDuplicateAsync(AppUser user, string email);
    Task<Result<bool>> IsPhoneNumberDuplicateAsync(AppUser user, string phoneNumber);
    Task<IdentityResult> UpdateUserAsync(AppUser user);
    Task<IdentityResult> RemoveUserRolesAsync(AppUser user, string role);
    Task<IdentityResult> ResetUserPasswordAsync(Expression<Func<AppUser, bool>> contactPredicate, string token, string newPassword);
    Task<string> GeneratePasswordResetToken(AppUser user);
    Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
    Task<AppUser?> GetUserByIdIncludeProfileAsync(Guid userId);

}