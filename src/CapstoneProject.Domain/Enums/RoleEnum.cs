namespace CapstoneProject.Domain.Enums;

/// <summary>
/// RBAC: chỉ 3 vai trò – Learner (người học), Moderator (kiểm duyệt), Admin (quản trị).
/// </summary>
public enum RoleEnum
{
    Admin,
    /// <summary>Người học</summary>
    Learner,
    /// <summary>Người kiểm duyệt nội dung UGC</summary>
    Moderator
}