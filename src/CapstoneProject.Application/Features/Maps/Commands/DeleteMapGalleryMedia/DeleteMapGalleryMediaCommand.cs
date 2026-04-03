using MediatR;
using CapstoneProject.Application.Common.Models;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMapGalleryMedia;

public record DeleteMapGalleryMediaCommand(Guid MapId, Guid MediaId) : IRequest<Result>;
