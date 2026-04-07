using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.LearningPath.Queries.GetConceptCompletion;

public class GetConceptCompletionQueryHandler : IRequestHandler<GetConceptCompletionQuery, Result<ConceptCompletionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetConceptCompletionQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ConceptCompletionDto>> Handle(GetConceptCompletionQuery request, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid || !userId.HasValue)
            return Result<ConceptCompletionDto>.Failure("Yêu cầu xác thực.", ErrorCodeEnum.Unauthorized);

        var progress = await _unitOfWork.Repository<UserConceptProgress>().GetQueryable()
            .Where(p => p.UserId == userId.Value && p.ConceptId == request.ConceptId && !p.IsDeleted)
            .Select(p => new ConceptCompletionDto
            {
                IsCompleted = p.IsCompleted,
                CompletedAt = p.CompletedAt
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var dto = progress ?? new ConceptCompletionDto { IsCompleted = false, CompletedAt = null };
        return Result<ConceptCompletionDto>.Success(dto, "Đã lấy trạng thái hoàn thành khái niệm.");
    }
}
