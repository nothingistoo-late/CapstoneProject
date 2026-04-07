using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.API.Attributes;

/// <summary>
/// Base filter for system access control
/// </summary>
public abstract class SystemAccessFilterBase : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Method == "OPTIONS")
            return;
            
        if (context.HttpContext.Request.Method == "POST" && 
            context.HttpContext.Request.HasJsonContentType())
        {
            context.HttpContext.Items["ProcessLoginResult"] = true;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items["ProcessLoginResult"] == null)
            return;
            
        if (context.Result is not ObjectResult objectResult || objectResult.StatusCode != 200)
            return;
        
        if (objectResult.Value is not Result<AuthResponse> authResult || !authResult.IsSuccess)
            return;
            
        var authResponse = authResult.Data;
        
        if (authResponse != null && !IsAuthorizedForSystem(authResponse))
        {
            var systemName = GetSystemName();
            var allowedRoles = GetAllowedRolesDescription();
            
            context.Result = new ObjectResult(Result.Failure(
                $"Bạn không có quyền truy cập vào {systemName}.",
                ErrorCodeEnum.InsufficientPermissions,
                new List<string> { $"Chỉ có {allowedRoles} mới có thể truy cập khu vực này. Vui lòng sử dụng một tài khoản thích hợp." }))
            {
                StatusCode = 403
            };
        }
    }
    
    protected abstract bool IsAuthorizedForSystem(AuthResponse user);
    protected abstract string GetSystemName();
    protected abstract string GetAllowedRolesDescription();
    
    /// <summary>
    /// Check if the user has a specific role based on the AppRole list
    /// </summary>
    protected bool HasRole(AuthResponse user, string role)
    {
        return user.Roles?.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
    }
}

/// <summary>
/// Filter that allows Learner (vÃ  Admin) truy cáº­p khu vá»±c learner.
/// </summary>
public class LearnerRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Learner.ToString()) || HasRole(user, RoleEnum.Admin.ToString());
    }

    protected override string GetSystemName()
    {
        return "Learner Website";
    }

    protected override string GetAllowedRolesDescription()
    {
        return "Learners and Administrators";
    }
}

/// <summary>
/// Filter that allows Moderator and Admin access (portal kiá»ƒm duyá»‡t).
/// </summary>
public class ModeratorRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Moderator.ToString()) || HasRole(user, RoleEnum.Admin.ToString());
    }

    protected override string GetSystemName()
    {
        return "Moderator Portal";
    }

    protected override string GetAllowedRolesDescription()
    {
        return "Moderators and Administrators";
    }
}

/// <summary>
/// Filter that allows Admin and Moderator access to CMS.
/// </summary>
public class AdminRoleAccessFilter : SystemAccessFilterBase
{
    protected override bool IsAuthorizedForSystem(AuthResponse user)
    {
        return HasRole(user, RoleEnum.Admin.ToString()) || HasRole(user, RoleEnum.Moderator.ToString());
    }

    protected override string GetSystemName()
    {
        return "CMS System";
    }

    protected override string GetAllowedRolesDescription()
    {
        return "Moderators and Administrators";
    }
} 