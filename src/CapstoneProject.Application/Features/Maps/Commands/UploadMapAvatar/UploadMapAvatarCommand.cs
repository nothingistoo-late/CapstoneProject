using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Maps.Commands.UploadMapAvatar;

public record UploadMapAvatarCommand(Guid MapId, IFormFile AvatarFile) : IRequest<Result<string>>;
