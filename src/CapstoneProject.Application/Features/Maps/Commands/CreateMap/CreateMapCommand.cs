using CapstoneProject.Application.Commons.DTOs.Maps;
using CapstoneProject.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMap;

public record CreateMapCommand(
    CreateMapRequest Request,
    bool AutoPublish = false,
    IReadOnlyList<IFormFile>? GalleryFiles = null,
    IFormFile? AvatarFile = null) : IRequest<Result<Guid>>;
