using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CapstoneProject.Application.Commons.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Enums;
using CapstoneProject.Infrastructure.Services;

namespace CapstoneProject.Infrastructure.Factories;

/// <summary>
/// Factory for creating appropriate file service providers
/// </summary>
public class FileServiceFactory : IFileServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FileStorageSettings _storageSettings;
    private readonly ILogger<FileServiceFactory> _logger;

    public FileServiceFactory(
        IServiceProvider serviceProvider,
        IOptions<FileStorageSettings> storageSettings,
        ILogger<FileServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _storageSettings = storageSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Create the appropriate file service based on configuration
    /// </summary>
    /// <returns>File service instance</returns>
    public IFileService CreateFileService()
    {
        // _logger.LogInformation("Creating file service of type: {ProviderType}", _storageSettings.ProviderType);
        
        return _storageSettings.ProviderType switch
        {
            StorageProviderType.LocalStorage => _serviceProvider.GetRequiredService<LocalFileService>(),
            
            // Uncomment when implemented and registered in DI
            // StorageProviderType.AmazonS3 => _serviceProvider.GetRequiredService<S3FileService>(),
            StorageProviderType.AmazonS3 => throw new NotImplementedException("Amazon S3 storage is not yet implemented. Please implement S3FileService."),
            
            _ => throw new ArgumentException($"Unsupported storage provider type: {_storageSettings.ProviderType}")
        };
    }
}
