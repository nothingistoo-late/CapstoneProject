using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Commons.Helpers;

/// <summary>Upload Cloudinary + gắn <see cref="MapMedia"/> vào context (chưa SaveChanges).</summary>
public static class MapGalleryMediaHelper
{
    public const int MaxFilesPerRequest = 20;

    /// <param name="requireAtLeastOneFile">true = endpoint gallery độc lập; false = tạo map kèm gallery (optional).</param>
    public static async Task<Result<List<MapMediaItemDto>>> StageGalleryMediaAsync(
        Guid mapId,
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
                return Result<List<MapMediaItemDto>>.Failure(
                    "At least one image or video file is required.",
                    ErrorCodeEnum.ValidationFailed);
            return Result<List<MapMediaItemDto>>.Success(new List<MapMediaItemDto>());
        }

        if (filtered.Count > MaxFilesPerRequest)
            return Result<List<MapMediaItemDto>>.Failure(
                $"At most {MaxFilesPerRequest} files per request.",
                ErrorCodeEnum.ValidationFailed);

        var mediaRepo = unitOfWork.Repository<MapMedia>();
        var order = await mediaRepo.GetQueryable()
            .Where(x => x.MapId == mapId)
            .MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? -1;

        var created = new List<MapMediaItemDto>();
        foreach (var file in filtered)
        {
            var kind = MapGalleryFileClassifier.TryClassify(file);
            if (kind == null)
                return Result<List<MapMediaItemDto>>.Failure(
                    $"Unsupported file: {file.FileName}. Use images (e.g. jpg, png, webp) or video (mp4, webm, mov).",
                    ErrorCodeEnum.ValidationFailed);

            order++;
            var prefix = $"map_{mapId:N}_g";
            var url = kind == MapMediaKind.Video
                ? await cloudinary.UploadVideoAsync(file, "maps/gallery", prefix, cancellationToken)
                : await cloudinary.UploadImageAsync(file, "maps/gallery", prefix, cancellationToken);
            if (string.IsNullOrEmpty(url))
                return Result<List<MapMediaItemDto>>.Failure(
                    "Upload failed for one or more files.",
                    ErrorCodeEnum.FileUploadFailed);

            var row = new MapMedia
            {
                MapId = mapId,
                Url = url,
                Kind = kind.Value,
                SortOrder = order
            };
            row.InitializeEntity(userId);
            await mediaRepo.AddAsync(row);
            created.Add(new MapMediaItemDto
            {
                Id = row.Id,
                Url = url,
                Kind = kind.Value.ToString(),
                SortOrder = order
            });
        }

        return Result<List<MapMediaItemDto>>.Success(created);
    }
}
