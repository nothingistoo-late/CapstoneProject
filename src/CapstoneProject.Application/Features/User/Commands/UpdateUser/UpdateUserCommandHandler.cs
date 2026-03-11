
using System.Transactions;
using AutoMapper;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.User.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileServiceFactory _fileServiceFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateUserCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IFileServiceFactory fileServiceFactory,
        ICloudinaryService cloudinaryService)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _fileServiceFactory = fileServiceFactory;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var request = command.Request;

            // Get user
            var user = await _identityService.GetUserByIdIncludeProfileAsync(command.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check email duplication (if email is being changed)
            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
            {
                var emailExists = await _unitOfWork.Repository<AppUser>().AnyAsync(u => u.Email == request.Email);
                if (emailExists)
                {
                    throw new ArgumentException("Email already exists");
                }
            }

            // Check phone number duplication (if phone number is being changed)
            if (!string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
            {
                var phoneExists = await _unitOfWork.Repository<AppUser>().AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
                if (phoneExists)
                {
                    throw new ArgumentException("Phone number already exists");
                }
            }

            var oldAvatarPath = user.AvatarPath;

            // Map request to user
            _mapper.Map(request, user);

            // Get current roles once (reuse for both validation and role update)
            var userRolesResult = await _identityService.GetUserRolesAsync(user);
            var currentRole = RoleEnum.Learner;
            var currentRolesList = new List<string>();
            if (userRolesResult.IsSuccess && userRolesResult.Data != null && userRolesResult.Data.Any())
            {
                currentRolesList = userRolesResult.Data.ToList();
                var roleString = currentRolesList.FirstOrDefault();
                if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<RoleEnum>(roleString, out var parsedRole))
                {
                    currentRole = parsedRole;
                }
            }

            // Determine target role (new role if updating, otherwise current role)
            var targetRole = request.NewRole ?? currentRole;

        // Upload new avatar if provided (Cloudinary)
        if (command.AvatarFile != null)
        {
            var avatarUrl = await _cloudinaryService.UploadImageAsync(
                command.AvatarFile,
                "avatars",
                $"user_{user.Id:N}",
                cancellationToken);
            if (!string.IsNullOrEmpty(avatarUrl))
                user.AvatarPath = avatarUrl;
        }
                   
        using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(1)
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update role if specified (reuse currentRolesList from above)
                if (request.NewRole.HasValue)
                {
                    if (currentRolesList.Any())
                    {
                        foreach (var role in currentRolesList)
                        {
                            await _identityService.RemoveUserRolesAsync(user, role);
                        }
                    }

                    var addRoleResult = await _identityService.AddUserToRoleAsync(user, request.NewRole.Value.ToString());
                    if (!addRoleResult.Succeeded)
                    {
                        var errors = addRoleResult.Errors.Select(e => e.Description).ToList();
                        return Result.Failure("Failed to update user role", ErrorCodeEnum.ValidationFailed, errors);
                    }
                }

                // Update user with Identity
                user.UpdateEntity(userId);
                var updateResult = await _identityService.UpdateUserAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to update user", ErrorCodeEnum.ValidationFailed, errors);
                }
                scope.Complete();
            }
            
            // Delete old avatar if a new one was uploaded fire-and-forget
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
                    catch
                    {
                        // Log error if needed, but do not throw
                    }
                }
            });
            return Result.Success("User updated successfully");
        }
        catch
        {
            throw;
        }
    }
}
