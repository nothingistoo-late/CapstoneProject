using CapstoneProject.Application.Commons.DTOs.Games;
using CapstoneProject.Application.Common.Attributes;
using CapstoneProject.Application.Common.Security;
using CapstoneProject.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Games.Commands.CreateMap;

[
    RequiresFeature(FeatureKeys.CanCreateGame)
]
public record CreateMapCommand(
    CreateMapRequest Request,
    bool AutoPublish = false,
    IReadOnlyList<IFormFile>? GalleryFiles = null,
    IFormFile? AvatarFile = null) : IRequest<Result<Guid>>;
