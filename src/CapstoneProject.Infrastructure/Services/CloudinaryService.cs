using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models;

namespace CapstoneProject.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
    {
        _settings = options.Value;
        _logger = logger;
        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder, string? publicIdPrefix = null, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return null;

        var prefix = string.IsNullOrEmpty(_settings.FolderPrefix) ? folder : $"{_settings.FolderPrefix}/{folder}";
        var publicId = string.IsNullOrEmpty(publicIdPrefix)
            ? $"{prefix}/{Guid.NewGuid():N}"
            : $"{prefix}/{publicIdPrefix}_{DateTime.UtcNow.Ticks}";

        try
        {
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Overwrite = true
            };
            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (result.Error != null)
            {
                _logger.LogWarning("Cloudinary upload error: {Error}", result.Error.Message);
                return null;
            }
            _logger.LogInformation("Cloudinary upload success: {PublicId}", result.PublicId);
            return result.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary upload failed");
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;
        try
        {
            var result = await _cloudinary.DeleteResourcesAsync(ResourceType.Image, publicId);
            return result.Deleted?.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary delete failed for {PublicId}", publicId);
            return false;
        }
    }

    public string? GetPublicIdFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.Contains("cloudinary.com"))
            return null;
        // Match pattern like .../upload/v1234567/folder/public_id.jpg
        var match = Regex.Match(url, @"/upload/(?:v\d+/)?(.+?)(?:\.\w+)?(?:\?|$)");
        return match.Success ? match.Groups[1].Value : null;
    }
}
