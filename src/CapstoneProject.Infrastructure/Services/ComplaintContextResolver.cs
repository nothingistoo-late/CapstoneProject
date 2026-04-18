using System.Text.Json;
using CapstoneProject.Application.Common.Interfaces;
using CapstoneProject.Application.Commons.Models.Complaints;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Services;

public class ComplaintContextResolver : IComplaintContextResolver
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CapstoneProjectDbContext _dbContext;

    public ComplaintContextResolver(IUnitOfWork unitOfWork, CapstoneProjectDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    public async Task<ComplaintContextResolvedDto?> ResolveAsync(
        string? contextType,
        Guid? contextId,
        string? contextDataJson,
        Guid? complaintUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(contextType) && !contextId.HasValue)
            return null;

        var normalizedType = (contextType ?? string.Empty).Trim();
        var id = contextId ?? TryExtractContextId(normalizedType, contextDataJson);
        if (!id.HasValue)
        {
            return new ComplaintContextResolvedDto
            {
                DisplayTitle = normalizedType,
                DisplaySubtitle = "Context reference unavailable"
            };
        }

        return normalizedType switch
        {
            "PaymentRecord" => await ResolvePaymentRecordAsync(id.Value, cancellationToken),
            "Game" => await ResolveMapAsync(id.Value, complaintUserId, cancellationToken),
            "Package" => await ResolvePackageAsync(id.Value, complaintUserId, cancellationToken),
            "Submission" => await ResolveSubmissionAsync(id.Value, cancellationToken),
            "PlayHistory" => await ResolvePlayHistoryAsync(id.Value, cancellationToken),
            "XpTransaction" => await ResolveXpTransactionAsync(id.Value, cancellationToken),
            "OrbitCoinTransaction" => await ResolveOrbitCoinTransactionAsync(id.Value, cancellationToken),
            _ => new ComplaintContextResolvedDto
            {
                DisplayTitle = normalizedType,
                DisplaySubtitle = id.Value.ToString(),
                ReferenceCode = id.Value.ToString()
            }
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolvePaymentRecordAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Amount,
                x.AmountVnd,
                x.PaidAt,
                x.ExternalId,
                x.PaymentStatus,
                x.PackageId,
                x.GameId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return null;

        string? targetName = null;
        if (payment.GameId.HasValue)
        {
            targetName = await _unitOfWork.Repository<Game>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == payment.GameId.Value)
                .Select(x => x.Title)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else if (payment.PackageId.HasValue)
        {
            targetName = await _unitOfWork.Repository<Package>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == payment.PackageId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var subtitle = payment.AmountVnd.HasValue
            ? $"{payment.AmountVnd.Value} VND"
            : payment.Amount.ToString("0.##");

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = targetName ?? "Payment record",
            DisplaySubtitle = subtitle,
            ReferenceCode = payment.ExternalId ?? payment.Id.ToString(),
            EventTime = payment.PaidAt,
            AmountValue = payment.Amount,
            LinkedOrder = new ComplaintLinkedOrderDto
            {
                OrderId = payment.Id,
                OrderCode = payment.ExternalId,
                OrderStatus = payment.PaymentStatus.ToString(),
                AmountOrbitCoin = payment.Amount,
                AmountVnd = payment.AmountVnd,
                PaidAt = payment.PaidAt,
                PaymentTargetType = payment.GameId.HasValue ? "Game" : payment.PackageId.HasValue ? "Package" : "Deposit",
                PaymentTargetId = payment.GameId ?? payment.PackageId,
                PaymentTargetName = targetName
            }
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolveMapAsync(Guid id, Guid? complaintUserId, CancellationToken cancellationToken)
    {
        var game = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new { x.Id, x.Title, x.Difficulty, x.Price })
            .FirstOrDefaultAsync(cancellationToken);

        if (game == null)
            return null;

        var linkedOrder = await FindLatestPaymentForMapAsync(game.Id, complaintUserId, cancellationToken);

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = game.Title,
            DisplaySubtitle = $"Difficulty {game.Difficulty}",
            ReferenceCode = game.Id.ToString(),
            AmountValue = game.Price,
            LinkedOrder = linkedOrder
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolvePackageAsync(Guid id, Guid? complaintUserId, CancellationToken cancellationToken)
    {
        var package = await _unitOfWork.Repository<Package>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new { x.Id, x.Name, x.DurationDays, x.Price })
            .FirstOrDefaultAsync(cancellationToken);

        if (package == null)
            return null;

        var linkedOrder = await FindLatestPaymentForPackageAsync(package.Id, complaintUserId, cancellationToken);

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = package.Name,
            DisplaySubtitle = $"{package.DurationDays} days",
            ReferenceCode = package.Id.ToString(),
            AmountValue = package.Price,
            LinkedOrder = linkedOrder
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolveSubmissionAsync(Guid id, CancellationToken cancellationToken)
    {
        var submission = await _unitOfWork.Repository<Submission>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.GameId,
                x.ResultStatus,
                x.Score,
                x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (submission == null)
            return null;

        var mapTitle = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == submission.GameId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = mapTitle ?? "Submission",
            DisplaySubtitle = submission.ResultStatus.ToString(),
            ReferenceCode = submission.Id.ToString(),
            EventTime = submission.CreatedAt,
            DeltaValue = submission.Score
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolvePlayHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var history = await _unitOfWork.Repository<UserGamePlayHistory>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.GameId,
                x.PlayMode,
                x.StartTime,
                x.EndTime,
                x.Stars
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (history == null)
            return null;

        var mapTitle = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == history.GameId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = mapTitle ?? "Play history",
            DisplaySubtitle = history.PlayMode.ToString(),
            ReferenceCode = history.Id.ToString(),
            EventTime = history.EndTime ?? history.StartTime,
            DeltaValue = history.Stars
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolveXpTransactionAsync(Guid id, CancellationToken cancellationToken)
    {
        var tx = await _unitOfWork.Repository<XpTransaction>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new { x.Id, x.SourceType, x.Reason, x.Delta, x.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (tx == null)
            return null;

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = "XP transaction",
            DisplaySubtitle = string.IsNullOrWhiteSpace(tx.Reason) ? tx.SourceType.ToString() : tx.Reason,
            ReferenceCode = tx.Id.ToString(),
            EventTime = tx.CreatedAt,
            DeltaValue = tx.Delta
        };
    }

    private async Task<ComplaintContextResolvedDto?> ResolveOrbitCoinTransactionAsync(Guid id, CancellationToken cancellationToken)
    {
        var tx = await _dbContext.OrbitCoinTransactions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Amount,
                x.TransactionType,
                x.Note,
                x.BalanceAfter,
                x.CreatedAt,
                x.RelatedEntityType,
                x.RelatedEntityId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tx == null)
            return null;

        ComplaintLinkedOrderDto? linkedOrder = null;
        if (tx.RelatedEntityId.HasValue
            && string.Equals(tx.RelatedEntityType, "PaymentRecord", StringComparison.OrdinalIgnoreCase))
        {
            linkedOrder = await BuildLinkedOrderFromPaymentRecordAsync(tx.RelatedEntityId.Value, cancellationToken);
        }

        return new ComplaintContextResolvedDto
        {
            DisplayTitle = "OrbitCoin transaction",
            DisplaySubtitle = string.IsNullOrWhiteSpace(tx.Note) ? tx.TransactionType.ToString() : tx.Note,
            ReferenceCode = tx.Id.ToString(),
            EventTime = tx.CreatedAt,
            AmountValue = tx.Amount,
            LinkedOrder = linkedOrder
        };
    }

    private async Task<ComplaintLinkedOrderDto?> FindLatestPaymentForMapAsync(
        Guid gameId,
        Guid? complaintUserId,
        CancellationToken cancellationToken)
    {
        if (!complaintUserId.HasValue)
            return null;

        var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == complaintUserId.Value && x.GameId == gameId)
            .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ExternalId,
                x.PaymentStatus,
                x.Amount,
                x.AmountVnd,
                x.PaidAt,
                x.GameId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return null;

        var mapName = await _unitOfWork.Repository<Game>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == gameId)
            .Select(x => x.Title)
            .FirstOrDefaultAsync(cancellationToken);

        return new ComplaintLinkedOrderDto
        {
            OrderId = payment.Id,
            OrderCode = payment.ExternalId,
            OrderStatus = payment.PaymentStatus.ToString(),
            AmountOrbitCoin = payment.Amount,
            AmountVnd = payment.AmountVnd,
            PaidAt = payment.PaidAt,
            PaymentTargetType = "Game",
            PaymentTargetId = payment.GameId,
            PaymentTargetName = mapName
        };
    }

    private async Task<ComplaintLinkedOrderDto?> FindLatestPaymentForPackageAsync(
        Guid packageId,
        Guid? complaintUserId,
        CancellationToken cancellationToken)
    {
        if (!complaintUserId.HasValue)
            return null;

        var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.UserId == complaintUserId.Value && x.PackageId == packageId)
            .OrderByDescending(x => x.PaidAt ?? x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ExternalId,
                x.PaymentStatus,
                x.Amount,
                x.AmountVnd,
                x.PaidAt,
                x.PackageId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return null;

        var packageName = await _unitOfWork.Repository<Package>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == packageId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new ComplaintLinkedOrderDto
        {
            OrderId = payment.Id,
            OrderCode = payment.ExternalId,
            OrderStatus = payment.PaymentStatus.ToString(),
            AmountOrbitCoin = payment.Amount,
            AmountVnd = payment.AmountVnd,
            PaidAt = payment.PaidAt,
            PaymentTargetType = "Package",
            PaymentTargetId = payment.PackageId,
            PaymentTargetName = packageName
        };
    }

    private async Task<ComplaintLinkedOrderDto?> BuildLinkedOrderFromPaymentRecordAsync(
        Guid paymentRecordId,
        CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Repository<PaymentRecord>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Id == paymentRecordId)
            .Select(x => new
            {
                x.Id,
                x.ExternalId,
                x.PaymentStatus,
                x.Amount,
                x.AmountVnd,
                x.PaidAt,
                x.GameId,
                x.PackageId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
            return null;

        string? targetName = null;
        if (payment.GameId.HasValue)
        {
            targetName = await _unitOfWork.Repository<Game>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == payment.GameId.Value)
                .Select(x => x.Title)
                .FirstOrDefaultAsync(cancellationToken);
        }
        else if (payment.PackageId.HasValue)
        {
            targetName = await _unitOfWork.Repository<Package>().GetQueryable()
                .Where(x => !x.IsDeleted && x.Id == payment.PackageId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ComplaintLinkedOrderDto
        {
            OrderId = payment.Id,
            OrderCode = payment.ExternalId,
            OrderStatus = payment.PaymentStatus.ToString(),
            AmountOrbitCoin = payment.Amount,
            AmountVnd = payment.AmountVnd,
            PaidAt = payment.PaidAt,
            PaymentTargetType = payment.GameId.HasValue ? "Game" : payment.PackageId.HasValue ? "Package" : "Deposit",
            PaymentTargetId = payment.GameId ?? payment.PackageId,
            PaymentTargetName = targetName
        };
    }

    private static Guid? TryExtractContextId(string contextType, string? contextDataJson)
    {
        if (string.IsNullOrWhiteSpace(contextDataJson))
            return null;

        var propertyName = contextType switch
        {
            "PaymentRecord" => "paymentRecordId",
            "Game" => "gameId",
            "Package" => "packageId",
            "Submission" => "submissionId",
            "PlayHistory" => "playHistoryId",
            "XpTransaction" => "xpTransactionId",
            "OrbitCoinTransaction" => "orbitCoinTransactionId",
            _ => null
        };

        if (propertyName == null)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(contextDataJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!doc.RootElement.TryGetProperty(propertyName, out var p) || p.ValueKind != JsonValueKind.String)
                return null;

            return Guid.TryParse(p.GetString(), out var guid) ? guid : null;
        }
        catch
        {
            return null;
        }
    }
}