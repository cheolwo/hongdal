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
    public async Task<Nongsaro감자ProfileArchive> CollectAndArchiveAsync(
        bool approveForSimulationContext,
        CancellationToken cancellationToken = default)
    {
        var profile = await query.조회Async(cancellationToken);
        var disaster = await disasterModule.연도조회Async(cancellationToken);
        Validate(profile);
        if (disaster.RetrievedAtUtc == default
            || disaster.RawContentHashSha256.Length != 64)
            throw new InvalidOperationException("NongsaroDisasterPreventionArchiveInvalid");
        var json = JsonSerializer.Serialize(profile);
        var hash = Sha256(string.Join("\n", profile.Sources
            .OrderBy(item => item.SourceStableId, StringComparer.Ordinal)
            .Select(item => string.Join("|", item.SourceStableId,
                item.ServiceName, item.OperationName, item.SourceRecordId,
                item.RetrievedAtUtc.ToUniversalTime().ToString("O"),
                item.RawContentHashSha256))) + "\n" + disaster.RawContentHashSha256);
        var existing = await db.NongsaroPotatoProfiles.SingleOrDefaultAsync(
            item => item.StableId == profile.StableId
                && item.SourceSetHashSha256 == hash,
            cancellationToken);
        if (existing is not null)
        {
            if (!string.Equals(existing.SourceSetHashSha256, hash, StringComparison.Ordinal))
                throw new InvalidOperationException("NongsaroPotatoProfileHashConflict");
            if (approveForSimulationContext && !existing.ApprovedForSimulationContext)
            {
                existing.ApprovedForSimulationContext = true;
                existing.ApprovedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
                await db.SaveChangesAsync(cancellationToken);
            }
            return existing;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var archiveRevision = await db.NongsaroPotatoProfiles
            .Where(item => item.StableId == profile.StableId)
            .Select(item => (int?)item.Revision)
            .MaxAsync(cancellationToken) ?? 0;
        var archive = new Nongsaro감자ProfileArchive
        {
            StableId = profile.StableId,
            Revision = archiveRevision + 1,
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
        if (profile.CanonicalProductStableId != "product:potato"
            || profile.WorkScheduleContentNo != "30699"
            || profile.NongsaroProductRelationStatusCode
                != 공통식품품목관계StatusCodes.Unlinked
            || profile.Sources.Count == 0
            || profile.Sources.Any(item => item.RetrievedAtUtc == default
                || item.RawContentHashSha256.Length != 64))
            throw new InvalidOperationException("NongsaroPotatoProfileArchiveInvalid");
    }

    private static string Sha256(string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
