using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface I커뮤니티활동공개ProjectionRecorder
{
    Task RecordAsync(
        CommunityActivityBoardDefinition definition,
        object occurrence,
        CancellationToken cancellationToken = default);
}

public sealed class 커뮤니티활동공개ProjectionRecorder(
    SsalddelContext db,
    TimeProvider timeProvider) : I커뮤니티활동공개ProjectionRecorder
{
    private static readonly JsonSerializerOptions IdentityJsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly string[] OccurredAtPropertyNames =
    [
        "발생시각Utc",
        "OccurredAtUtc",
        "OccurredAt"
    ];

    public async Task RecordAsync(
        CommunityActivityBoardDefinition definition,
        object occurrence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(occurrence);

        if (!definition.PublishesActivityPost)
        {
            return;
        }

        var occurrenceKey = BuildOccurrenceKey(definition, occurrence);
        var occurredAtUtc = ResolveOccurredAtUtc(occurrence);
        var bucketStartUtc = 커뮤니티활동공개Policy.주간시작Utc(occurredAtUtc);
        var aggregateKey = BuildAggregateKey(definition, bucketStartUtc);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (await db.Set<커뮤니티활동처리기록>()
                    .AsNoTracking()
                    .AnyAsync(x => x.OccurrenceKey == occurrenceKey, cancellationToken))
            {
                return;
            }

            var projection = await db.Set<커뮤니티활동공개Projection>()
                .SingleOrDefaultAsync(x => x.AggregateKey == aggregateKey, cancellationToken);

            if (projection is null)
            {
                var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
                projection = new 커뮤니티활동공개Projection
                {
                    AggregateKey = aggregateKey,
                    AppKey = definition.ProductName,
                    CommunityScope = definition.Board.Key,
                    ActivityKind = definition.SourceName,
                    Title = definition.ActivityDisplayName,
                    PublicSummary = definition.PublicActivitySummary,
                    TopicTagsJson = JsonSerializer.Serialize(new[]
                    {
                        definition.SourceKind,
                        definition.ProductVersion,
                        definition.Board.Key
                    }),
                    TimeBucketStartUtc = bucketStartUtc,
                    TimeBucketEndUtc = 커뮤니티활동공개Policy.주간종료Utc(bucketStartUtc),
                    ActivityCount = 1,
                    VisibilityScope = 커뮤니티활동공개Policy.공개범위,
                    PrivacyPolicyVersion = 커뮤니티활동공개Policy.개인정보PolicyVersion,
                    CreatedAtUtc = nowUtc,
                    UpdatedAtUtc = nowUtc
                };
                db.Add(projection);
            }
            else
            {
                projection.ActivityCount++;
                projection.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            }

            db.Add(new 커뮤니티활동처리기록
            {
                OccurrenceKey = occurrenceKey,
                AggregateKey = aggregateKey,
                SourceKind = definition.SourceKind,
                SourceName = definition.SourceName,
                RecordedAtUtc = timeProvider.GetUtcNow().UtcDateTime
            });

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                DetachProjectionEntries(db);
            }
        }
    }

    private DateTime ResolveOccurredAtUtc(object occurrence)
    {
        foreach (var propertyName in OccurredAtPropertyNames)
        {
            var value = occurrence.GetType().GetProperty(propertyName)?.GetValue(occurrence);
            if (value is DateTime dateTime)
            {
                return NormalizeUtc(dateTime);
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.UtcDateTime;
            }
        }

        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static string BuildOccurrenceKey(
        CommunityActivityBoardDefinition definition,
        object occurrence)
    {
        var payload = JsonSerializer.Serialize(
            occurrence,
            occurrence.GetType(),
            IdentityJsonOptions);
        return Hash(
            string.Join(
                "\n",
                definition.SourceKind,
                definition.SourceName,
                payload));
    }

    private static string BuildAggregateKey(
        CommunityActivityBoardDefinition definition,
        DateTime bucketStartUtc)
        => Hash(
            string.Join(
                "\n",
                definition.SourceKind,
                definition.SourceName,
                bucketStartUtc.ToString("O")));

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static void DetachProjectionEntries(SsalddelContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries()
                     .Where(entry =>
                         entry.Entity is 커뮤니티활동공개Projection
                         or 커뮤니티활동처리기록))
        {
            entry.State = EntityState.Detached;
        }
    }
}
