using MediatR;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Games.Commands.DeleteMapGalleryMedia;

public record DeleteMapGalleryMediaCommand(Guid GameId, Guid MediaId) : IRequest<Result>;
