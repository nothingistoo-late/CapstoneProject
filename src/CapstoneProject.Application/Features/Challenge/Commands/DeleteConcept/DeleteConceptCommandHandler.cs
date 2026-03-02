using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.DeleteConcept;

public class DeleteConceptCommandHandler : IRequestHandler<DeleteConceptCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteConceptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteConceptCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to delete a concept.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to delete concepts. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);

        var concept = await _unitOfWork.Repository<Concept>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ConceptId && !c.IsDeleted, cancellationToken);
        if (concept == null)
            return Result.Failure($"Concept not found with Id: {command.ConceptId}. The concept may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        concept.SoftDeleteEntity(userIdNullable!.Value);
        _unitOfWork.Repository<Concept>().Update(concept);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Concept deleted.");
    }
}
