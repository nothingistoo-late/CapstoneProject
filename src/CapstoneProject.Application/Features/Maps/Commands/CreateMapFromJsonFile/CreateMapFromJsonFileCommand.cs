using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.DTOs.Maps;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Maps.Commands.CreateMapFromJsonFile;

public record CreateMapFromJsonFileCommand(CreateMapFromJsonFileInput Input, bool AutoPublish = false, IFormFile? AvatarFile = null) : IRequest<Result<Guid>>;
