using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateTag;

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTagCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to create a tag.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<Guid>.Failure("You do not have permission to create tags. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<Guid>.Failure("Tag name is required and cannot be empty.", ErrorCodeEnum.ValidationFailed);

        var tag = new Tag { Name = command.Name.Trim() };
        tag.InitializeEntity(userIdNullable.Value);
        await _unitOfWork.Repository<Tag>().AddAsync(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(tag.Id, "Tag created.");
    }
}
