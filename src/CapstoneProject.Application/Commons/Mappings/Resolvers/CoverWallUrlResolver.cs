using AutoMapper;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Commons.Mappings.Resolvers;

/// <summary>
/// Resolves profile cover image path to a full URL (same rules as avatar).
/// </summary>
public class CoverWallUrlResolver : IValueResolver<AppUser, object, string?>
{
    private readonly IFileServiceFactory _fileServiceFactory;

    public CoverWallUrlResolver(IFileServiceFactory fileServiceFactory)
    {
        _fileServiceFactory = fileServiceFactory;
    }

    public string? Resolve(AppUser source, object destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.CoverImagePath))
            return null;

        if (source.CoverImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            source.CoverImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return source.CoverImagePath;

        try
        {
            var fileService = _fileServiceFactory.CreateFileService();
            return fileService.GetFileUrl(source.CoverImagePath);
        }
        catch
        {
            return source.CoverImagePath;
        }
    }
}
