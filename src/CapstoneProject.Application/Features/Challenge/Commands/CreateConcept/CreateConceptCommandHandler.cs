using MediatR;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateConcept;

public class CreateConceptCommandHandler : IRequestHandler<CreateConceptCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateConceptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateConceptCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userIdNullable) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userIdNullable.HasValue)
            return Result<Guid>.Failure("Authentication required. Please log in to create a concept.", ErrorCodeEnum.Unauthorized);
        var roles = await _currentUserService.GetCurrentRolesAsync();
        if (!roles.Contains(RoleEnum.Admin) && !roles.Contains(RoleEnum.Moderator))
            return Result<Guid>.Failure("You do not have permission to create concepts. Only Admin or Moderator can perform this action.", ErrorCodeEnum.Forbidden);
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<Guid>.Failure("Concept name is required and cannot be empty.", ErrorCodeEnum.ValidationFailed);

        var concept = new Concept { Name = command.Name.Trim(), Description = command.Description?.Trim() };
        concept.InitializeEntity(userIdNullable.Value);
        await _unitOfWork.Repository<Concept>().AddAsync(concept);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(concept.Id, "Concept created.");
    }
}
