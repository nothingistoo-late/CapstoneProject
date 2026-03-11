using CapstoneProject.Application.Commons.Interfaces;

namespace CapstoneProject.Infrastructure.Services;

/// <inheritdoc />

public class AvatarUrlResolverService : IAvatarUrlResolverService
{
    private readonly IFileServiceFactory _fileServiceFactory;

    public AvatarUrlResolverService(IFileServiceFactory fileServiceFactory)
    {
        _fileServiceFactory = fileServiceFactory;
    }

    public string? ResolveAvatarUrl(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;
        try
        {
            var fileService = _fileServiceFactory.CreateFileService();
            return fileService.GetFileUrl(path);
        }
        catch
        {
            return path;
        }
    }
}
