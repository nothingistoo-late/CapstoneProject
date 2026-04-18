using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMapFromJsonFile;

public record CreateMapFromJsonFileCommand(
    CreateMapFromJsonFileInput Input,
    bool AutoPublish = false,
    IFormFile? AvatarFile = null,
    IReadOnlyList<IFormFile>? GalleryFiles = null) : IRequest<Result<Guid>>;
