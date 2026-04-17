using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Upload Cloudinary + gáº¯n <see cref="GameMedia"/> vÃ o context (chÆ°a SaveChanges).</summary>
public static class MapGalleryMediaHelper
{
    public const int MaxFilesPerRequest = 20;

    /// <param name="requireAtLeastOneFile">true = endpoint gallery Ä‘á»™c láº­p; false = táº¡o game kÃ¨m gallery (optional).</param>
    public static async Task<Result<List<GameMediaItemDto>>> StageGalleryMediaAsync(
        Guid gameId,
        Guid userId,
        IReadOnlyList<IFormFile>? files,
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinary,
        bool requireAtLeastOneFile,
        CancellationToken cancellationToken)
    {
        var filtered = files?.Where(f => f is { Length: > 0 }).ToList() ?? new List<IFormFile>();
        if (filtered.Count == 0)
        {
            if (requireAtLeastOneFile)
                return Result<List<GameMediaItemDto>>.Failure(
                    "At least one image or video file is required.",
                    ErrorCodeEnum.ValidationFailed);
            return Result<List<GameMediaItemDto>>.Success(new List<GameMediaItemDto>(), "Đã xử lý thư viện media thành công.");
        }

        if (filtered.Count > MaxFilesPerRequest)
            return Result<List<GameMediaItemDto>>.Failure(
                $"At most {MaxFilesPerRequest} files per request.",
                ErrorCodeEnum.ValidationFailed);

        var mediaRepo = unitOfWork.Repository<GameMedia>();
        var order = await mediaRepo.GetQueryable()
            .Where(x => x.GameId == gameId)
            .MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? -1;

        var created = new List<GameMediaItemDto>();
        foreach (var file in filtered)
        {
            var kind = MapGalleryFileClassifier.TryClassify(file);
            if (kind == null)
                return Result<List<GameMediaItemDto>>.Failure(
                    $"Unsupported file: {file.FileName}. Use images (e.g. jpg, png, webp) or video (mp4, webm, mov).",
                    ErrorCodeEnum.ValidationFailed);

            order++;
            var prefix = $"map_{gameId:N}_g";
            var url = kind == GameMediaKind.Video
                ? await cloudinary.UploadVideoAsync(file, "games/gallery", prefix, cancellationToken)
                : await cloudinary.UploadImageAsync(file, "games/gallery", prefix, cancellationToken);
            if (string.IsNullOrEmpty(url))
                return Result<List<GameMediaItemDto>>.Failure(
                    "Upload failed for one or more files.",
                    ErrorCodeEnum.FileUploadFailed);

            var row = new GameMedia
            {
                GameId = gameId,
                Url = url,
                Kind = kind.Value,
                SortOrder = order
            };
            row.InitializeEntity(userId);
            await mediaRepo.AddAsync(row);
            created.Add(new GameMediaItemDto
            {
                Id = row.Id,
                Url = url,
                Kind = kind.Value.ToString(),
                SortOrder = order
            });
        }

        return Result<List<GameMediaItemDto>>.Success(created, "Đã xử lý thư viện media thành công.");
    }
}
