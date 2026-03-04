using MediatR;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Application.Common.Enums;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Interfaces;
using CapstoneProject.Domain.Common;
using CapstoneProject.Domain.Entities;

namespace CapstoneProject.Application.Features.Maps.Commands.DeleteMaps;

public class DeleteMapsCommandHandler : IRequestHandler<DeleteMapsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMapsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteMapsCommand command, CancellationToken cancellationToken)
    {
        var (isValid, userId) = await _currentUserService.IsUserValidAsync();
        if (!isValid)
            return Result.Failure("Authentication required.", ErrorCodeEnum.Unauthorized);

        var repo = _unitOfWork.Repository<LevelCatalog>();
        var entity = await repo.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity == null)
            return Result.Failure("Level not found.", ErrorCodeEnum.NotFound);

        entity.SoftDeleteEntity(userId);
        repo.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success("Level deleted successfully.");
    }
}
