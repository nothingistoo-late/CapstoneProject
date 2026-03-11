using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Commons.Interfaces;

/// <summary>
/// Service for uploading images to Cloudinary (user avatar, map avatar).
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Upload image from IFormFile to Cloudinary.
    /// </summary>
    /// <param name="file">Image file (e.g. avatar).</param>
    /// <param name="folder">Folder in Cloudinary (e.g. "avatars", "maps").</param>
    /// <param name="publicIdPrefix">Optional prefix for public_id (e.g. "user_guid", "map_guid").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Secure URL of the uploaded image, or null on failure.</returns>
    Task<string?> UploadImageAsync(IFormFile file, string folder, string? publicIdPrefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete image from Cloudinary by public_id (full or without extension).
    /// </summary>
    /// <param name="publicId">Public ID of the asset (e.g. "capstone/avatars/user_xxx").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract public_id from Cloudinary URL (for delete). Returns null if not a Cloudinary URL.
    /// </summary>
    string? GetPublicIdFromUrl(string url);
}
