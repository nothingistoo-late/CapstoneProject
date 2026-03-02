using System.Transactions;
using AutoMapper;
using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;
using CapstoneProject.Domain.Common;

namespace CapstoneProject.Application.Features.User.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileServiceFactory _fileServiceFactory;

    public CreateUserCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IFileServiceFactory fileServiceFactory)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _fileServiceFactory = fileServiceFactory;
    }

    public async Task<Result> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var (isValid, userId) = await _currentUserService.IsUserValidAsync();
            if (!isValid)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var request = command.Request;

            // Check if email already exists
            var emailExists = await _identityService.IsEmailDuplicateAsync(new AppUser(), request.Email);
            if (emailExists.IsSuccess && emailExists.Data)
            {
                throw new ArgumentException("Email already exists");
            }

            // Check if phone number already exists (if provided)
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var phoneExists = await _identityService.IsPhoneNumberDuplicateAsync(new AppUser(), request.PhoneNumber);
                if (phoneExists.IsSuccess && phoneExists.Data)
                {
                    throw new ArgumentException("Phone number already exists");
                }
            }

            var user = _mapper.Map<AppUser>(request);
            user.InitializeEntity(userId);
            user.Status = request.Status ?? EntityStatusEnum.Active;

            // Upload avatar if provided
            if (command.AvatarFile != null)
            {
                var fileService = _fileServiceFactory.CreateFileService();
                var fileName = $"{user.Id}_{Path.GetFileName(command.AvatarFile.FileName)}";
                user.AvatarPath = await fileService.UploadFileAsync(
                    command.AvatarFile,
                    fileName,
                    "avatars");
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
                // Create user with Identity
                var createResult = await _identityService.CreateUserAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to create user", ErrorCodeEnum.ValidationFailed, errors);
                }

                // Add role
                var roleResult = await _identityService.AddUserToRoleAsync(user, request.Role.ToString());
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to add user to role", ErrorCodeEnum.ValidationFailed, errors);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }

            return Result.Success("User created successfully");
        }
        catch
        {
            throw;
        }
    }
}