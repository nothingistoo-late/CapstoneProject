using Microsoft.AspNetCore.Http;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Nhận diện ảnh vs video cho upload gallery map (Content-Type hoặc đuôi file).</summary>
public static class MapGalleryFileClassifier
{
    public static MapMediaKind? TryClassify(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;
        var ct = file.ContentType?.ToLowerInvariant() ?? "";
        if (ct.StartsWith("image/", StringComparison.Ordinal))
            return MapMediaKind.Image;
        if (ct.StartsWith("video/", StringComparison.Ordinal))
            return MapMediaKind.Video;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" => MapMediaKind.Image,
            ".mp4" or ".webm" or ".mov" or ".m4v" => MapMediaKind.Video,
            _ => null
        };
    }
}
