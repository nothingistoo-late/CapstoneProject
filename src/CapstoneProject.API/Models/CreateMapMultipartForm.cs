namespace CapstoneProject.API.Models;

/// <summary>multipart/form-data: tạo map kèm avatar + gallery (field <c>data</c> = JSON <see cref="CapstoneProject.Application.Commons.DTOs.Maps.CreateMapRequest"/>).</summary>
public class CreateMapMultipartForm
{
    /// <summary>Chuỗi JSON cùng cấu trúc CreateMapRequest (title, levels, …).</summary>
    public string Data { get; set; } = string.Empty;
    public IFormFile? AvatarFile { get; set; }
    public List<IFormFile>? GalleryFiles { get; set; }
}
