using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateConcept;

public class UpdateConceptCommandHandler : IRequestHandler<UpdateConceptCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateConceptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateConceptCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result.Failure("Authentication required. Please log in to update a concept.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result.Failure("You do not have permission to update concepts. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result.Failure("Concept name is required and cannot be empty.", ErrorCodeEnum.ValidationFailed);

        var concept = await _unitOfWork.Repository<Concept>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == command.ConceptId && !c.IsDeleted, cancellationToken);
        if (concept == null)
            return Result.Failure($"Concept not found with Id: {command.ConceptId}. The concept may have been deleted or does not exist.", ErrorCodeEnum.NotFound);

        concept.Name = command.Name.Trim();
        concept.Description = command.Description?.Trim();
        concept.UpdateEntity(userIdNullable!.Value);
        _unitOfWork.Repository<Concept>().Update(concept);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Concept updated.");
    }
}
