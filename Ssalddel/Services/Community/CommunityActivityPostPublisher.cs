using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Application.Community;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public sealed class CommunityActivityPostPublisher(
    ICommunityAutomatedPostPublisher automatedPostPublisher,
    TimeProvider timeProvider) : ICommunityActivityPostPublisher
{
    private static readonly JsonSerializerOptions IdentityJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] OccurredAtPropertyNames =
    [
        "발생시각Utc",
        "OccurredAtUtc",
        "OccurredAt"
    ];

    public async Task PublishAsync(
        CommunityActivityBoardDefinition definition,
        object occurrence,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(occurrence);

        var occurredAtUtc = ResolveOccurredAtUtc(occurrence);
        var draft = new CommunityAutomatedPostDraft(
            $"activity-{definition.Board.Key}",
            BuildOccurrenceKey(definition, occurrence),
            definition.Board.DisplayName,
            $"{definition.ProductName} {definition.ProductVersion} 공개 활동",
            "자동 활동",
            $"[{definition.SourceKindDisplayName} 활동] {definition.ActivityDisplayName}",
            BuildPublicBody(definition, occurredAtUtc),
            "살뜰 활동봇");
        await automatedPostPublisher.PublishIfMissingAsync(draft, cancellationToken);
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
        var identity = string.Join(
            "\n",
            definition.SourceKind,
            definition.SourceName,
            payload);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return Convert.ToHexString(hash)[..32].ToLowerInvariant();
    }

    private static string BuildPublicBody(
        CommunityActivityBoardDefinition definition,
        DateTime occurredAtUtc)
        => string.Join(
            Environment.NewLine,
            "[자동 활동 안내] 다른 앱에서 완료된 업무의 발생 사실만 비식별 요약으로 기록했습니다.",
            $"버전: {definition.RoadmapDisplayName}",
            $"발생 유형: {definition.SourceKindDisplayName} · {definition.SourceName}",
            $"발생 시각(UTC): {occurredAtUtc:yyyy-MM-dd HH:mm}",
            string.Empty,
            definition.PublicActivitySummary,
            string.Empty,
            CommunityActivityBoardCatalog.PrivacyBoundary);

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
