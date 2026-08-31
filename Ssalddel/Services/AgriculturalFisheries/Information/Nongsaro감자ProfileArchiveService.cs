using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface INongsaro감자ProfileArchiveService
{
    Task<Nongsaro감자ProfileArchive?> 최신자료승인조회Async(
        CancellationToken cancellationToken = default);

    Task<Nongsaro감자ProfileArchive> CollectAndArchiveAsync(
        bool approveForSimulationContext,
        CancellationToken cancellationToken = default);
}

public sealed class Nongsaro감자ProfileArchiveService(
    I농사로감자생육요구Profile조회UseCase query,
    I농사로농작물재해예방Module disasterModule,
    AgriculturalFisheriesDbContext db,
    TimeProvider timeProvider) : INongsaro감자ProfileArchiveService
{
    private const string 감자ProfileStableId = "crop-requirement-profile:nongsaro.potato.1";

    // 마지막 자료가 보류/거부이면 과거 승인본으로 조용히 돌아가지 않는다.
    // 이 조회는 게시 권한이나 게임 규칙 승인이 아니며 관리자 HTTP 경계는 별도다.
    public async Task<Nongsaro감자ProfileArchive?> 최신자료승인조회Async(
        CancellationToken cancellationToken = default)
    {
        var latest = await db.NongsaroPotatoProfiles.AsNoTracking()
            .Where(item => item.StableId == 감자ProfileStableId)
            .OrderByDescending(item => item.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        if (latest is null || !latest.ApprovedForSimulationContext
            || latest.ReviewStatusCode == 작물생육요구검토StatusCodes.Rejected)
            return null;
        Validate(ReadProfile(latest));
        return latest;
    }

    public async Task<Nongsaro감자ProfileArchive> CollectAndArchiveAsync(
        bool approveForSimulationContext,
        CancellationToken cancellationToken = default)
    {
        var profile = await query.조회Async(cancellationToken);
        Validate(profile);
        if (approveForSimulationContext && profile.ReviewStatusCode == 작물생육요구검토StatusCodes.Rejected)
            throw new InvalidOperationException("NongsaroRejectedProfileCannotBeApproved");
        var disaster = await disasterModule.연도조회Async(cancellationToken);
        if (disaster is null || disaster.RetrievedAtUtc == default
            || disaster.ServiceName != Nongsaro공공데이터Catalog.농작물재해예방Service
            || disaster.OperationName != Nongsaro공공데이터Catalog.농작물재해예방연도Operation
            || disaster.ResultCode is not "00" and not "0"
            || !IsHash(disaster.RawContentHashSha256))
            throw new InvalidOperationException("NongsaroDisasterPreventionArchiveInvalid");
        var json = JsonSerializer.Serialize(profile);
        var hash = Sha256(string.Join("\n", profile.Sources
            .OrderBy(item => item.SourceStableId, StringComparer.Ordinal)
            .Select(item => string.Join("|", item.SourceStableId,
                item.ServiceName, item.OperationName, item.SourceRecordId,
                item.RetrievedAtUtc.ToUniversalTime().ToString("O"),
                item.RawContentHashSha256))) + "\n" + disaster.RawContentHashSha256);
        var latest = await db.NongsaroPotatoProfiles
            .Where(item => item.StableId == profile.StableId)
            .OrderByDescending(item => item.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        var storedProfile = latest is null ? null : ReadProfile(latest);
        if (storedProfile is not null) Validate(storedProfile);
        if (latest is not null && ContentFingerprint(storedProfile!, latest.DisasterPreventionHashSha256)
            == ContentFingerprint(profile, disaster.RawContentHashSha256))
        {
            // 원 SourceSetHash/입수시각은 과거 보관 사실 그대로 남긴다.
            // 이번 재조회 시각은 새 내용 revision이나 기존 원문 덮어쓰기 근거가 아니다.
            if (approveForSimulationContext && !latest.ApprovedForSimulationContext)
            {
                latest.ApprovedForSimulationContext = true;
                latest.ApprovedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
                await db.SaveChangesAsync(cancellationToken);
            }
            return latest;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var archiveRevision = latest?.Revision ?? 0;
        var archive = new Nongsaro감자ProfileArchive
        {
            StableId = profile.StableId,
            Revision = checked(archiveRevision + 1),
            CanonicalProductStableId = profile.CanonicalProductStableId,
            WorkScheduleGroupCode = profile.WorkScheduleGroupCode,
            WorkScheduleContentNo = profile.WorkScheduleContentNo,
            ProductRelationStatusCode = profile.NongsaroProductRelationStatusCode,
            ReviewStatusCode = profile.ReviewStatusCode,
            ApprovedForSimulationContext = approveForSimulationContext,
            ProfileJson = json,
            SourceSetHashSha256 = hash,
            DisasterPreventionHashSha256 = disaster.RawContentHashSha256,
            DisasterPreventionRetrievedAtUtc = disaster.RetrievedAtUtc.UtcDateTime,
            RetrievedAtUtc = profile.RetrievedAtUtc.UtcDateTime,
            ArchivedAtUtc = now,
            ApprovedAtUtc = approveForSimulationContext ? now : null,
        };
        db.NongsaroPotatoProfiles.Add(archive);
        await db.SaveChangesAsync(cancellationToken);
        return archive;
    }

    private static void Validate(농사로작물생육요구ProfileResponse profile)
    {
        if (profile is null || profile.StableId != 감자ProfileStableId
            || profile.Revision <= 0 || profile.RetrievedAtUtc == default
            || profile.CanonicalProductStableId != "product:potato"
            || profile.WorkScheduleGroupCode != "210005"
            || profile.WorkScheduleContentNo != "30699"
            || profile.NongsaroProductRelationStatusCode
                != 공통식품품목관계StatusCodes.Unlinked
            || profile.CanPublishSimulationRule
            || profile.ReviewStatusCode is not (작물생육요구검토StatusCodes.PendingHumanReview
                or 작물생육요구검토StatusCodes.ApprovedForRuleDraft or 작물생육요구검토StatusCodes.Rejected)
            || profile.Sources is null || profile.Sources.Count is 0 or > 32
            || profile.EvidenceTopics is null || profile.Limitations is null
            || profile.Sources.Any(item => item is null || item.RetrievedAtUtc == default
                || string.IsNullOrWhiteSpace(item.SourceStableId)
                || string.IsNullOrWhiteSpace(item.ServiceName)
                || string.IsNullOrWhiteSpace(item.OperationName)
                || string.IsNullOrWhiteSpace(item.SourceRecordId)
                || !IsHash(item.RawContentHashSha256))
            || profile.Sources.Select(item => item.SourceStableId).Distinct(StringComparer.Ordinal).Count() != profile.Sources.Count)
            throw new InvalidOperationException("NongsaroPotatoProfileArchiveInvalid");
    }

    private static 농사로작물생육요구ProfileResponse ReadProfile(Nongsaro감자ProfileArchive archive)
        => JsonSerializer.Deserialize<농사로작물생육요구ProfileResponse>(archive.ProfileJson)
            ?? throw new InvalidOperationException("NongsaroStoredPotatoProfileInvalid");

    private static string ContentFingerprint(농사로작물생육요구ProfileResponse profile, string disasterHash)
    {
        var content = profile with
        {
            RetrievedAtUtc = DateTimeOffset.UnixEpoch,
            Sources = profile.Sources.OrderBy(item => item.SourceStableId, StringComparer.Ordinal)
                .Select(item => item with { RetrievedAtUtc = DateTimeOffset.UnixEpoch }).ToArray()
        };
        return Sha256(JsonSerializer.Serialize(content) + "\n" + disasterHash);
    }

    private static bool IsHash(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);

    private static string Sha256(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
