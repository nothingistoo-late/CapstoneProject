using MediatR;
using Microsoft.AspNetCore.Http;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;

namespace CapstoneProject.Application.Features.Maps.Commands.AddMapGalleryMedia;

public record AddMapGalleryMediaCommand(Guid MapId, IReadOnlyList<IFormFile> Files) : IRequest<Result<List<MapMediaItemDto>>>;
