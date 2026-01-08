using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CapstoneProject.Application.Commons.Models;
using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.Infrastructure.Services;

/// <summary>
/// Implementation for local file system storage
/// </summary>
public class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<LocalFileService> _logger;
    private readonly FileStorageSettings _storageSettings;
    private const string UploadsFolder = "uploads";

    public LocalFileService(
        IWebHostEnvironment webHostEnvironment,
        ILogger<LocalFileService> logger,
        IOptions<FileStorageSettings> storageSettings)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _storageSettings = storageSettings.Value;
    }

    /// <summary>
    /// Upload file from IFormFile
    /// </summary>
    public async Task<string> UploadFileAsync(
        IFormFile file, 
        string fileName, 
        string subDirectory = "", 
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty", nameof(file));
        }

        if (file.Length > _storageSettings.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File size exceeds the maximum allowed size ({_storageSettings.MaxFileSizeBytes / 1024 / 1024}MB)");
        }

        try
        {
            // Validate file extension if restrictions are set
            if (_storageSettings.AllowedExtensions?.Length > 0)
            {
                var extension = Path.GetExtension(fileName);
                if (!_storageSettings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"File extension {extension} is not allowed");
                }
            }

            // Determine uploads directory path
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, UploadsFolder);
            
            // Add subdirectory if specified
            if (!string.IsNullOrEmpty(subDirectory))
            {
                uploadsFolder = Path.Combine(uploadsFolder, subDirectory);
            }
            
            // Create directory if it doesn't exist
            EnsureDirectoryExists(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, cancellationToken);
            }
            
            // Create relative path for storage in DB
            var relativePath = string.IsNullOrEmpty(subDirectory)
                ? Path.Combine(UploadsFolder, fileName)
                : Path.Combine(UploadsFolder, subDirectory, fileName);
            
            relativePath = relativePath.Replace('\\', '/');
            if (relativePath.StartsWith('/'))
            {
                relativePath = relativePath.Substring(1);
            }

            _logger.LogInformation("File uploaded successfully: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Upload file from stream
    /// </summary>
    public async Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        string subDirectory = "", 
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("File stream is empty", nameof(fileStream));
        }

        if (fileStream.Length > _storageSettings.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File size exceeds the maximum allowed size ({_storageSettings.MaxFileSizeBytes / 1024 / 1024}MB)");
        }

        try
        {
            // Validate file extension if restrictions are set
            if (_storageSettings.AllowedExtensions?.Length > 0)
            {
                var extension = Path.GetExtension(fileName);
                if (!_storageSettings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"File extension {extension} is not allowed");
                }
            }

            // Determine uploads directory path
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, UploadsFolder);
            
            // Add subdirectory if specified
            if (!string.IsNullOrEmpty(subDirectory))
            {
                uploadsFolder = Path.Combine(uploadsFolder, subDirectory);
            }
            
            // Create directory if it doesn't exist
            EnsureDirectoryExists(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            // Save the file
            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream, cancellationToken);
            }
            
            // Create relative path for storage in DB
            var relativePath = string.IsNullOrEmpty(subDirectory)
                ? Path.Combine(UploadsFolder, fileName)
                : Path.Combine(UploadsFolder, subDirectory, fileName);
            
            relativePath = relativePath.Replace('\\', '/');
            if (relativePath.StartsWith('/'))
            {
                relativePath = relativePath.Substring(1);
            }

            _logger.LogInformation("File uploaded from stream successfully: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file from stream: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Delete file from local storage
    /// </summary>
    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Task.FromResult(false);
            }
            
            // Remove "uploads/" from the beginning if present
            if (filePath.StartsWith($"{UploadsFolder}/"))
            {
                filePath = filePath.Substring($"{UploadsFolder}/".Length);
            }
            
            // Get the complete file path
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadsFolder, filePath);
            
            // Check if file exists
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", fullPath);
                return Task.FromResult(false);
            }
            
            // Delete the file
            File.Delete(fullPath);
            _logger.LogInformation("File deleted successfully: {FilePath}", fullPath);
            
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Get the public URL for a file
    /// </summary>
    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        // Ensure relative path format
        var relativePath = filePath.Replace('\\', '/');
        if (!relativePath.StartsWith('/'))
        {
            relativePath = $"/{relativePath}";
        }

        // If base URL is configured, use it
        if (!string.IsNullOrEmpty(_storageSettings.BaseUrl))
        {
            return $"{_storageSettings.BaseUrl.TrimEnd('/')}{relativePath}";
        }

        // Otherwise return relative path
        return relativePath;
    }

    /// <summary>
    /// Get file content from storage
    /// </summary>
    public async Task<(byte[] FileContent, string ContentType)> GetFileContentAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path is empty", nameof(filePath));
            }

            // Remove leading slash and "uploads/" prefix if present
            var cleanPath = filePath.TrimStart('/');
            if (cleanPath.StartsWith($"{UploadsFolder}/"))
            {
                cleanPath = cleanPath.Substring($"{UploadsFolder}/".Length);
            }

            // Get the complete file path
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadsFolder, cleanPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Read file content
            var fileContent = await File.ReadAllBytesAsync(fullPath, cancellationToken);

            // Determine content type based on extension
            var contentType = GetContentType(Path.GetExtension(fullPath));

            return (fileContent, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file content: {FilePath}", filePath);
            throw;
        }
    }

    /// <summary>
    /// Ensure directory exists, create if not
    /// </summary>
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Created directory: {Path}", path);
        }
    }

    /// <summary>
    /// Get content type based on file extension
    /// </summary>
    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
