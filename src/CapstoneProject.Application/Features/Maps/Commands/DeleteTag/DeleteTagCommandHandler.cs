using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteTag;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTagCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to delete a tag.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to delete tags. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        var tag = await _unitOfWork.Repository<Tag>().GetQueryable()
            .FirstOrDefaultAsync(t => t.Id == command.TagId && !t.IsDeleted, cancellationToken);
        if (tag == null)
            return Result.Failure($"Tag not found with Id: {command.TagId}. The tag may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        tag.SoftDeleteEntity(userIdNullable!.Value);
        _unitOfWork.Repository<Tag>().Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Tag deleted.");
    }
}
