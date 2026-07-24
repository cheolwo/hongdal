using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Community;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public sealed record CommunityPostEmailNotificationOutboxWork(
    long OutboxId,
    long PostId,
    int Attempt,
    string ProcessingToken);

public interface ICommunityPostEmailNotificationOutboxStore
{
    Task EnqueueAsync(long postId, CancellationToken cancellationToken = default);
    Task<CommunityPostEmailNotificationOutboxWork?> ClaimNextAsync(
        TimeSpan lease,
        CancellationToken cancellationToken = default);
    Task CompleteAsync(
        CommunityPostEmailNotificationOutboxWork work,
        string status,
        string? detail,
        CancellationToken cancellationToken = default);
    Task RetryAsync(
        CommunityPostEmailNotificationOutboxWork work,
        DateTime nextAttemptAtUtc,
        string? error,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityPostEmailNotificationOutboxStore(SsalddelContext db)
    : ICommunityPostEmailNotificationOutboxStore
{
    public async Task EnqueueAsync(long postId, CancellationToken cancellationToken = default)
    {
        if (postId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(postId));
        }

        if (await db.CommunityPostEmailNotificationOutbox
            .AsNoTracking()
            .AnyAsync(x => x.PostId == postId, cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.CommunityPostEmailNotificationOutbox.Add(new CommunityPostEmailNotificationOutbox
        {
            PostId = postId,
            Status = CommunityPostEmailNotificationOutboxStatuses.Pending,
            AttemptCount = 0,
            NextAttemptAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (!await db.CommunityPostEmailNotificationOutbox
                .AsNoTracking()
                .AnyAsync(x => x.PostId == postId, cancellationToken))
            {
                throw;
            }
        }
    }

    public async Task<CommunityPostEmailNotificationOutboxWork?> ClaimNextAsync(
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var candidate = await db.CommunityPostEmailNotificationOutbox
            .Where(x =>
                (x.Status == CommunityPostEmailNotificationOutboxStatuses.Pending
                 || x.Status == CommunityPostEmailNotificationOutboxStatuses.Failed
                 || (x.Status == CommunityPostEmailNotificationOutboxStatuses.Processing
                     && x.LockedUntilUtc <= now))
                && x.NextAttemptAtUtc <= now)
            .OrderBy(x => x.NextAttemptAtUtc)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (candidate is null)
        {
            return null;
        }

        var token = Guid.NewGuid().ToString("N");
        candidate.Status = CommunityPostEmailNotificationOutboxStatuses.Processing;
        candidate.ProcessingToken = token;
        candidate.LockedUntilUtc = now.Add(lease);
        candidate.AttemptCount++;
        candidate.UpdatedAtUtc = now;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            return null;
        }

        return new(candidate.Id, candidate.PostId, candidate.AttemptCount, token);
    }

    public async Task CompleteAsync(
        CommunityPostEmailNotificationOutboxWork work,
        string status,
        string? detail,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindClaimedAsync(work, cancellationToken);
        if (entity is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        entity.Status = status;
        entity.LastError = Trim(detail);
        entity.ProcessingToken = null;
        entity.LockedUntilUtc = null;
        entity.ProcessedAtUtc = now;
        entity.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RetryAsync(
        CommunityPostEmailNotificationOutboxWork work,
        DateTime nextAttemptAtUtc,
        string? error,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindClaimedAsync(work, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.Status = CommunityPostEmailNotificationOutboxStatuses.Failed;
        entity.NextAttemptAtUtc = DateTime.SpecifyKind(nextAttemptAtUtc, DateTimeKind.Utc);
        entity.LastError = Trim(error);
        entity.ProcessingToken = null;
        entity.LockedUntilUtc = null;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private Task<CommunityPostEmailNotificationOutbox?> FindClaimedAsync(
        CommunityPostEmailNotificationOutboxWork work,
        CancellationToken cancellationToken)
        => db.CommunityPostEmailNotificationOutbox.FirstOrDefaultAsync(
            x => x.Id == work.OutboxId
                 && x.Status == CommunityPostEmailNotificationOutboxStatuses.Processing
                 && x.ProcessingToken == work.ProcessingToken,
            cancellationToken);

    private static string? Trim(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim()[..Math.Min(value.Trim().Length, 2000)];
}
