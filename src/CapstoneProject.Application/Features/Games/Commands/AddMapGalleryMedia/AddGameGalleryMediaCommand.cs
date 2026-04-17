using MediatR;
using Microsoft.AspNetCore.Http;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Games;

namespace CapstoneProject.Application.Features.Games.Commands.AddMapGalleryMedia;

public record AddMapGalleryMediaCommand(Guid GameId, IReadOnlyList<IFormFile> Files) : IRequest<Result<List<GameMediaItemDto>>>;
