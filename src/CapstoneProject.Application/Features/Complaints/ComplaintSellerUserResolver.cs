using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Features.Complaints;

internal static class ComplaintSellerUserResolver
{
    /// <summary>Resolves the game author's user id for seller/buyer labeling on complaints.</summary>
    public static async Task<Guid?> ResolveSellerUserIdAsync(
        IUnitOfWork unitOfWork,
        Complaint complaint,
        CancellationToken cancellationToken)
    {
        Guid? gameId = null;

        if (string.Equals(complaint.ContextType, "Game", StringComparison.OrdinalIgnoreCase) && complaint.ContextId.HasValue)
            gameId = complaint.ContextId.Value;

        if (gameId == null
            && string.Equals(complaint.ContextType, "PaymentRecord", StringComparison.OrdinalIgnoreCase)
            && complaint.ContextId.HasValue)
        {
            gameId = await unitOfWork.Repository<PaymentRecord>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == complaint.ContextId.Value)
                .Select(x => x.GameId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!gameId.HasValue)
            return null;

        return await unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == gameId.Value)
            .Select(x => x.CreatedBy)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
