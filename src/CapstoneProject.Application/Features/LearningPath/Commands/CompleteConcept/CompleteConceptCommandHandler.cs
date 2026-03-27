using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Application.Commons.Models.Xp;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Domain.Enums;

namespace CapstoneProject.Application.Features.LearningPath.Commands.CompleteConcept;

public class CompleteConceptCommandHandler : IRequestHandler<CompleteConceptCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IXpEngineService _xpEngineService;

    public CompleteConceptCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IXpEngineService xpEngineService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _xpEngineService = xpEngineService;
    }

    public async Task<Result> Handle(CompleteConceptCommand request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result.Failure("Authentication required. Please log in to complete a concept.", ErrorCodeEnum.Unauthorized);

        var concept = await _unitOfWork.Repository<Concept>().GetQueryable()
            .Where(c => c.Id == request.ConceptId && !c.IsDeleted && c.Status == EntityStatusEnum.Active)
            .Select(c => new { c.Id, c.LearningGoalId })
            .FirstOrDefaultAsync(cancellationToken);
        if (concept == null)
            return Result.Failure("Concept not found.", ErrorCodeEnum.NotFound);

        var repo = _unitOfWork.Repository<UserConceptProgress>();
        var existing = await repo.GetQueryable()
            .FirstOrDefaultAsync(p => p.UserId == userId.Value && p.ConceptId == request.ConceptId && !p.IsDeleted, cancellationToken);

        var justCompleted = false;
        if (existing != null)
        {
            if (existing.IsCompleted)
                return Result.Success("Concept already completed.");
            existing.IsCompleted = true;
            existing.CompletedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            existing.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow;
            existing.UpdatedBy = userId;
            repo.Update(existing);
            justCompleted = true;
        }
        else
        {
            var progress = new UserConceptProgress
            {
                UserId = userId.Value,
                ConceptId = request.ConceptId,
                IsCompleted = true,
                CompletedAt = CapstoneProject.Domain.Common.VietnamDateTime.DbNow
            };
            progress.InitializeEntity(userId);
            await repo.AddAsync(progress);
            justCompleted = true;
        }

        if (justCompleted)
        {
            var conceptXpResult = await _xpEngineService.GrantXpAsync(new XpGrantInput
            {
                UserId = userId.Value,
                RequestedXp = 0,
                SourceType = XpSourceTypeEnum.ConceptComplete,
                SourceId = concept.Id,
                IdempotencyKey = $"xp:concept:{userId}:{concept.Id}",
                Reason = "Concept completed",
                Metadata = $"{{\"conceptId\":\"{concept.Id}\"}}"
            }, cancellationToken);
            if (!conceptXpResult.IsSuccess)
                return Result.Failure(conceptXpResult.Message ?? "Failed to grant concept XP.", ErrorCodeEnum.DatabaseError);

            var pathItems = await _unitOfWork.Repository<LearningPathItem>().GetQueryable()
                .Where(i => i.LearningGoalId == concept.LearningGoalId && !i.IsDeleted)
                .Select(i => new { i.ItemType, i.ConceptId, i.MapId })
                .ToListAsync(cancellationToken);

            if (pathItems.Count > 0)
            {
                var conceptIdsInGoal = pathItems.Where(i => i.ConceptId.HasValue).Select(i => i.ConceptId!.Value).ToHashSet();
                var mapIdsInGoal = pathItems.Where(i => i.MapId.HasValue).Select(i => i.MapId!.Value).ToHashSet();

                var completedConceptIds = await repo.GetQueryable()
                    .Where(p => p.UserId == userId.Value && !p.IsDeleted && p.IsCompleted && conceptIdsInGoal.Contains(p.ConceptId))
                    .Select(p => p.ConceptId)
                    .ToListAsync(cancellationToken);
                completedConceptIds.Add(concept.Id);
                var completedConceptSet = completedConceptIds.ToHashSet();

                var completedMapIds = await _unitOfWork.Repository<UserMapResult>().GetQueryable()
                    .Where(r => r.UserId == userId.Value && !r.IsDeleted && r.BestStars >= 1 && mapIdsInGoal.Contains(r.MapId))
                    .Select(r => r.MapId)
                    .ToListAsync(cancellationToken);
                var completedMapSet = completedMapIds.ToHashSet();

                var isLearningPathCompleted = pathItems.All(i =>
                    i.ItemType == LearningPathItemTypeEnum.Concept
                        ? i.ConceptId.HasValue && completedConceptSet.Contains(i.ConceptId.Value)
                        : i.MapId.HasValue && completedMapSet.Contains(i.MapId.Value));

                if (isLearningPathCompleted)
                {
                    var pathXpResult = await _xpEngineService.GrantXpAsync(new XpGrantInput
                    {
                        UserId = userId.Value,
                        RequestedXp = 0,
                        SourceType = XpSourceTypeEnum.LearningPathComplete,
                        SourceId = concept.LearningGoalId,
                        IdempotencyKey = $"xp:learningpath:{userId}:{concept.LearningGoalId}",
                        Reason = "Learning path completed",
                        Metadata = $"{{\"learningGoalId\":\"{concept.LearningGoalId}\"}}"
                    }, cancellationToken);
                    if (!pathXpResult.IsSuccess)
                        return Result.Failure(pathXpResult.Message ?? "Failed to grant learning path XP.", ErrorCodeEnum.DatabaseError);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Concept completed. Next item in your path is now unlocked.");
    }
}



