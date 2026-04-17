using CapstoneProject.API.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.API.Helpers;

/// <summary>Đọc <c>mapDetailFiles</c> (một hoặc nhiều JSON) vào <see cref="CreateMapFromJsonFileInput"/>.</summary>
public static class MapJsonUploadFormReader
{
    /// <summary>
    /// Swagger / curl lặp field: <c>List&lt;IFormFile&gt;</c> thường không bind — luôn quét <c>HttpRequest.Form.Files</c>.
    /// </summary>
    public static async Task<(CreateMapFromJsonFileInput? Input, string? Error)> BuildCreateInputAsync(
        CreateMapFromJsonFileRequest request,
        HttpRequest? httpRequest = null)
    {
        var files = CollectGameDetailFiles(request, httpRequest);
        if (files.Count == 0)
            return (null, "Provide mapDetailFiles: at least one JSON file (one file = one or more levels in that file; multiple files = one level per file).");

        List<string> contents = new();
        foreach (var f in files)
        {
            if (f.Length == 0) continue;
            using var reader = new StreamReader(f.OpenReadStream());
            var text = await reader.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(text))
                contents.Add(text);
        }

        if (contents.Count == 0)
            return (null, "Provide mapDetailFiles: at least one file with real JSON content (empty or whitespace-only files are ignored).");

        string mapDetailJsonContent;
        List<string>? mapDetailJsonContents;
        if (contents.Count == 1)
        {
            mapDetailJsonContent = contents[0];
            mapDetailJsonContents = null;
        }
        else
        {
            mapDetailJsonContent = string.Empty;
            mapDetailJsonContents = contents;
        }

        var input = new CreateMapFromJsonFileInput
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = request.Difficulty,
            Price = request.Price,
            Type = request.Type,
            FreeTrialAttemptLimit = request.FreeTrialAttemptLimit,
            TagIdsCsv = request.TagIdsCsv ?? string.Empty,
            LearnedTagsCsv = request.LearnedTagsCsv ?? string.Empty,
            GameDetailJsonContent = mapDetailJsonContent,
            GameDetailJsonContents = mapDetailJsonContents
        };
        return (input, null);
    }

    /// <summary>
    /// Ưu tiên quét toàn bộ <see cref="IFormFileCollection"/> (case-insensitive, không dùng <c>GetFiles</c> vì có thể lệch key).
    /// Fallback: model <see cref="CreateMapFromJsonFileRequest.GameDetailFiles"/>.
    /// </summary>
    private static List<IFormFile> CollectGameDetailFiles(CreateMapFromJsonFileRequest request, HttpRequest? httpRequest)
    {
        var list = new List<IFormFile>();

        void TryAdd(IFormFile? f)
        {
            if (f == null || f.Length == 0) return;
            if (IsAvatarField(f.Name)) return;
            if (!IsGameDetailOrJsonPart(f)) return;
            // Chỉ bỏ trùng cùng instance (model binding + Form.Files); nhiều part giống nội dung vẫn là nhiều IFormFile.
            foreach (var x in list)
            {
                if (ReferenceEquals(x, f)) return;
            }
            list.Add(f);
        }

        // 1) Luôn đọc raw form trước (Swagger/curl gửi đúng nhưng không bind vào List<IFormFile>)
        if (httpRequest != null)
        {
            foreach (var f in httpRequest.Form.Files)
                TryAdd(f);
        }

        // 2) Model binding (khi có)
        if (request.GameDetailFiles is { Count: > 0 })
        {
            foreach (var f in request.GameDetailFiles)
                TryAdd(f);
        }

        // 3) Tên field cũ đơn (một file)
        if (list.Count == 0 && httpRequest != null)
        {
            TryAdd(httpRequest.Form.Files.GetFile("mapDetailFile"));
            TryAdd(httpRequest.Form.Files.GetFile("GameDetailFile"));
        }

        return list;
    }

    private static bool IsAvatarField(string? name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.Equals("AvatarFile", StringComparison.OrdinalIgnoreCase)
               || name.Equals("avatarFile", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Field game JSON hoặc part có Content-Type / tên file giống JSON (Swagger đôi khi đặt tên lạ).</summary>
    private static bool IsGameDetailOrJsonPart(IFormFile f)
    {
        var name = f.Name ?? "";
        if (IsGameDetailFilesFormField(name)) return true;
        if (name.Equals("mapDetailFile", StringComparison.OrdinalIgnoreCase)) return true;

        var ct = f.ContentType ?? "";
        if (ct.Contains("json", StringComparison.OrdinalIgnoreCase))
            return true;

        var fn = f.FileName ?? "";
        if (fn.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static bool IsGameDetailFilesFormField(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (name.Equals("GameDetailFiles", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.Equals("mapDetailFiles", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.StartsWith("GameDetailFiles[", StringComparison.OrdinalIgnoreCase)) return true;
        if (name.StartsWith("mapDetailFiles[", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
