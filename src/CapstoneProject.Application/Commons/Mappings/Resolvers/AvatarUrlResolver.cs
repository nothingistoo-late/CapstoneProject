using AutoMapper;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Commons.Mappings.Resolvers;

/// <summary>
/// Custom AutoMapper resolver to convert relative avatar path to full URL
/// </summary>
public class AvatarUrlResolver : IValueResolver<AppUser, object, string?>
{
    private readonly IFileServiceFactory _fileServiceFactory;

    public AvatarUrlResolver(IFileServiceFactory fileServiceFactory)
    {
        _fileServiceFactory = fileServiceFactory;
    }

    public string? Resolve(AppUser source, object destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.AvatarPath))
            return null;

        // Cloudinary (hoặc URL đầy đủ) - trả về luôn
        if (source.AvatarPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            source.AvatarPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return source.AvatarPath;

        try
        {
            // Path local - convert to full URL
            var fileService = _fileServiceFactory.CreateFileService();
            return fileService.GetFileUrl(source.AvatarPath);
        }
        catch
        {
            return source.AvatarPath;
        }
    }
}
