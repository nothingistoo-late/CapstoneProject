using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthorizeRolesAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    
    public AuthorizeRolesAttribute(params string[] roles)
    {
        _roles = roles;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
            
        if (allowAnonymous)
            return;
            
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(
                Result.Failure("Bạn chưa được xác thực.", ErrorCodeEnum.Unauthorized));
            return;
        }
        
        if (_roles.Length == 0)
            return;
            
        var userRoleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role);
        if (userRoleClaim == null)
        {
            context.Result = new ObjectResult(
                Result.Failure("You do not have the required role to access this resource.", ErrorCodeEnum.Forbidden))
            {
                StatusCode = 403
            };
            return;
        }
        
        var userRole = userRoleClaim.Value;
        
        if (!_roles.Contains(userRole))
        {
            context.Result = new ObjectResult(
                Result.Failure($"This resource requires one of these roles: {string.Join(", ", _roles)}. Your role: {userRole}", ErrorCodeEnum.InsufficientPermissions))
            {
                StatusCode = 403
            };
            return;
        }
    }
} 