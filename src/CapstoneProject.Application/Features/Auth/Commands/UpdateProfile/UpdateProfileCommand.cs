using CapstoneProject.Application.Common.DTOs.Auth;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(UpdateProfileRequest Request, IFormFile? AvatarFile) : IRequest<Result<ProfileResponse>>;
