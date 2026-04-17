namespace CapstoneProject.Application.Commons.Models;

/// <summary>
/// Configuration for Cloudinary (avatar, game images).
/// </summary>
public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    /// <summary>Folder prefix in Cloudinary (e.g. "capstone/avatars", "capstone/games").</summary>
    public string FolderPrefix { get; set; } = "capstone";
}
