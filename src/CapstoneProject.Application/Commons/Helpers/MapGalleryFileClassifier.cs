using Microsoft.AspNetCore.Http;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Nhận diện ảnh vs video cho upload gallery game (Content-Type hoặc đuôi file).</summary>
public static class MapGalleryFileClassifier
{
    public static GameMediaKind? TryClassify(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;
        var ct = file.ContentType?.ToLowerInvariant() ?? "";
        if (ct.StartsWith("image/", StringComparison.Ordinal))
            return GameMediaKind.Image;
        if (ct.StartsWith("video/", StringComparison.Ordinal))
            return GameMediaKind.Video;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" => GameMediaKind.Image,
            ".mp4" or ".webm" or ".mov" or ".m4v" => GameMediaKind.Video,
            _ => null
        };
    }
}
