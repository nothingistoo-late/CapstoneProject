using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Features.Auth.Queries.GetProfile;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<ProfileResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileServiceFactory _fileServiceFactory;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFileServiceFactory fileServiceFactory,
        ICloudinaryService cloudinaryService,
        IMapper mapper,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileServiceFactory = fileServiceFactory;
        _cloudinaryService = cloudinaryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProfileResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId, roles) = await _currentUserService.ValidateUserWithRolesAsync();
            if (!isValid || userId == null)
            {
                return Result<ProfileResponse>.Failure("User is not authenticated", ErrorCodeEnum.Unauthorized);
            }

            var request = command.Request;

            // Get user with profile using IdentityService
            var user = await _identityService.GetUserByIdIncludeProfileAsync(userId.Value);
            if (user == null)
            {
                return Result<ProfileResponse>.Failure("User not found", ErrorCodeEnum.NotFound);
            }

            // Check phone number duplication (if phone number is being changed)
            if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
            {
                var phoneExists = await _unitOfWork.Repository<AppUser>()
                    .GetQueryable()
                    .AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
                    
                if (phoneExists)
                {
                    return Result<ProfileResponse>.Failure("Phone number already exists", ErrorCodeEnum.ValidationFailed);
                }
            }

            var oldAvatarPath = user.AvatarPath;

            // Update basic user info (email cannot be changed in profile update)
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.Gender.HasValue)
                user.Gender = request.Gender;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth;

            if (!string.IsNullOrWhiteSpace(request.Bio))
                user.Bio = request.Bio;

            // Handle avatar upload (Cloudinary)
            if (command.AvatarFile != null)
            {
                var avatarUrl = await _cloudinaryService.UploadImageAsync(
                    command.AvatarFile,
                    "avatars",
                    $"user_{userId:N}",
                    cancellationToken);
                if (!string.IsNullOrEmpty(avatarUrl))
                    user.AvatarPath = avatarUrl;
            }

            // Update user entity with tracking info and SecurityStamp
            user.UpdateEntity(userId);
            
            // Use IdentityService to update user (handles SecurityStamp)
            var updateResult = await _identityService.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Result<ProfileResponse>.Failure("Failed to update profile", ErrorCodeEnum.ValidationFailed, errors);
            }

            // Delete old avatar if a new one was uploaded (fire-and-forget)
            _ = Task.Run(async () =>
            {
                if (command.AvatarFile != null && !string.IsNullOrEmpty(oldAvatarPath))
                {
                    try
                    {
                        var publicId = _cloudinaryService.GetPublicIdFromUrl(oldAvatarPath);
                        if (publicId != null)
                            await _cloudinaryService.DeleteAsync(publicId, cancellationToken);
                        else
                        {
                            var fileService = _fileServiceFactory.CreateFileService();
                            await fileService.DeleteFileAsync(oldAvatarPath, cancellationToken);
                        }
                    }
                    catch { /* ignore */ }
                }
            });

            // Return updated profile
            var response = _mapper.Map<ProfileResponse>(user);
            return Result<ProfileResponse>.Success(response, "Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating profile for user {UserId}", _currentUserService.UserId);
            return Result<ProfileResponse>.Failure("An error occurred while updating profile", ErrorCodeEnum.InternalError);
        }
    }
}
