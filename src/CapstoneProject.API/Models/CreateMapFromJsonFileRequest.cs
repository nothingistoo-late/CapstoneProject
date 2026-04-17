namespace CapstoneProject.API.Models;

/// <summary>
/// Shared request for creating a game from an uploaded JSON file. Used by both Learner and CMS game endpoints.
/// </summary>
public class CreateMapFromJsonFileRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public decimal? Price { get; set; }
    /// <summary>Loại game mặc định cho level nếu JSON không tự khai báo. Hỗ trợ Topdown | Platform | Snake.</summary>
    public string? Type { get; set; }
    /// <summary>Số lượt chơi thử miễn phí cho mỗi người chơi. Null/0 = không có trial.</summary>
    public int? FreeTrialAttemptLimit { get; set; }
    public string TagIdsCsv { get; set; } = string.Empty;
    public string LearnedTagsCsv { get; set; } = string.Empty;
    /// <summary>
    /// Một hoặc nhiều file JSON. Mỗi file = một level (order 0,1,2…).
    /// Một file duy nhất có thể chứa object một level, mảng các level, hoặc <c>{ "levels": [...] }</c>.
    /// </summary>
    public List<IFormFile>? GameDetailFiles { get; set; }
    /// <summary>Avatar game (ảnh, optional). Upload lên Cloudinary khi tạo game.</summary>
    public IFormFile? AvatarFile { get; set; }
    /// <summary>Ảnh/video mô tả game (gallery, optional). Cùng lần tạo game (không cần API gallery riêng).</summary>
    public List<IFormFile>? GalleryFiles { get; set; }
}
