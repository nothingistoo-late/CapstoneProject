using CapstoneProject.Application.Commons.Enums;

namespace CapstoneProject.Application.Commons.Models;

/// <summary>
/// Configuration settings for file storage
/// </summary>
public class FileStorageSettings
{
    /// <summary>
    /// Type of storage provider to use
    /// </summary>
    public StorageProviderType ProviderType { get; set; } = StorageProviderType.LocalStorage;
    
    /// <summary>
    /// Base URL for accessing stored files
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum file size in bytes (default: 10MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    
    /// <summary>
    /// Allowed file extensions (empty = all allowed)
    /// </summary>
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Local storage specific settings
    /// </summary>
    public LocalStorageSettings? Local { get; set; }
    
    /// <summary>
    /// Amazon S3 specific settings
    /// </summary>
    public S3StorageSettings? S3 { get; set; }
}

/// <summary>
/// Local storage specific settings
/// </summary>
public class LocalStorageSettings
{
    /// <summary>
    /// Root path for storing files (relative to wwwroot)
    /// </summary>
    public string RootPath { get; set; } = "uploads";
}

/// <summary>
/// Amazon S3 storage specific settings
/// </summary>
public class S3StorageSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}

