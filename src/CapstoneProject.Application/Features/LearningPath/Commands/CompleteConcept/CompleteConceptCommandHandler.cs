using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Commands.CompleteConcept;

public class CompleteConceptCommandHandler : IRequestHandler<CompleteConceptCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CompleteConceptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(CompleteConceptCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Authentication required. Please log in to complete a concept.", ErrorCodeEnum.Unauthorized);

        var conceptExists = await _unitOfWork.Repository<Concept>().GetQueryable()
            .AnyAsync(c => c.Id == request.ConceptId && !c.IsDeleted && c.Status == EntityStatusEnum.Active, cancellationToken);
        if (!conceptExists)
            return Result.Failure("Concept not found.", ErrorCodeEnum.NotFound);

        var repo = _unitOfWork.Repository<UserConceptProgress>();
        var existing = await repo.GetQueryable()
            .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.ConceptId == request.ConceptId && !p.IsDeleted, cancellationToken);

        if (existing != null)
        {
            if (existing.IsCompleted)
                return Result.Success("Concept already completed.");
            existing.IsCompleted = true;
            existing.CompletedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
            existing.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
            existing.UpdatedBy = userId;
            repo.Update(existing);
        }
        else
        {
            var progress = new UserConceptProgress
            {
                UserId = userId.Value,
                ConceptId = request.ConceptId,
                IsCompleted = true,
                CompletedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now
            };
            progress.InitializeEntity(userId);
            await repo.AddAsync(progress);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Concept completed. Next item in your path is now unlocked.");
    }
}

