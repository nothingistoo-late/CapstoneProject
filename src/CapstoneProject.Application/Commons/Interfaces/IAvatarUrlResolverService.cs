namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Resolve avatar path to full URL (Cloudinary URL returned as-is, local path converted via IFileService).
/// Dùng trong handler khi build response (Chat, v.v.) thay vì gán raw AvatarPath.
/// </summary>
public interface IAvatarUrlResolverService
{
    string? ResolveAvatarUrl(string? path);
}
