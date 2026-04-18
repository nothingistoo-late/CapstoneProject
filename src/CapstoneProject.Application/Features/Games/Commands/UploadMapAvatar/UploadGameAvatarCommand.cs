using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Games.Commands.UploadMapAvatar;

public record UploadMapAvatarCommand(Guid GameId, IFormFile AvatarFile) : IRequest<Result<string>>;
