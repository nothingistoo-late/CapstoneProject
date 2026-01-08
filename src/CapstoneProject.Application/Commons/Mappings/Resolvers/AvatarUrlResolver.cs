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

        try
        {
            // Convert relative path to full URL
            var fileService = _fileServiceFactory.CreateFileService();
            return fileService.GetFileUrl(source.AvatarPath);
        }
        catch
        {
            // If any error occurs, return the original path
            return source.AvatarPath;
        }
    }
}
