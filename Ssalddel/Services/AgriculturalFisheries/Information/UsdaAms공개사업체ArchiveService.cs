using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed class UsdaAms공개사업체ArchiveService(
    IUsdaAms공개사업체DirectoryClient client,
    AgriculturalFisheriesDbContext db,
    TimeProvider timeProvider,
    IOptions<PublicDataOptions> options,
    ILogger<UsdaAms공개사업체ArchiveService> logger)
    : IUsdaAms공개사업체ArchiveService
{
    private readonly UsdaAmsLocalFoodDirectoryOptions _options =
        options.Value.UsdaAmsLocalFoodDirectory;

    public async Task<UsdaAms공개사업체수집응답> CollectAsync(
        UsdaAms공개사업체수집요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var directoryTypes =
            UsdaAms공개사업체DirectoryCatalog.NormalizeMany(
                request.DirectoryTypes);
        var startedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var run = new UsdaAms공개사업체수집Run
        {
            RequestedDirectoryTypesJson = JsonSerializer.Serialize(directoryTypes),
            SourceUrl = _options.DataSharingUrl,
            SourceMessagesJson = JsonSerializer.Serialize(new[]
            {
                "USDA AMS Local Food Directories의 공개 bulk download만 사용합니다.",
                "상세 주소·좌표·담당자·전화·이메일은 저장하지 않습니다."
            }),
            StartedAtUtc = startedAtUtc
        };
        db.UsdaAmsPublicBusinessCollectionRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);
        var runId = run.Id;
        db.ChangeTracker.Clear();

        try
        {
            foreach (var directoryType in directoryTypes)
            {
                var rows = await client.GetDirectoryAsync(
                    directoryType,
                    cancellationToken);
                var result = await PersistDirectoryAsync(
                    runId,
                    directoryType,
                    rows,
                    timeProvider.GetUtcNow().UtcDateTime,
                    cancellationToken);

                run = await db.UsdaAmsPublicBusinessCollectionRuns
                    .SingleAsync(item => item.Id == runId, cancellationToken);
                run.CompletedDirectoryCount++;
                run.FetchedCount += result.FetchedCount;
                run.InsertedCount += result.InsertedCount;
                run.UpdatedCount += result.UpdatedCount;
                run.UnchangedCount += result.UnchangedCount;
                run.NoLongerListedCount += result.NoLongerListedCount;
                run.RejectedCount += result.RejectedCount;
                await db.SaveChangesAsync(cancellationToken);
                db.ChangeTracker.Clear();
            }

            run = await db.UsdaAmsPublicBusinessCollectionRuns
                .SingleAsync(item => item.Id == runId, cancellationToken);
            run.StatusCode = UsdaAms공개사업체Archive상태Codes.완료;
            run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            await db.SaveChangesAsync(cancellationToken);

            return ToResponse(run, directoryTypes);
        }
        catch (Exception exception)
        {
            db.ChangeTracker.Clear();
            run = await db.UsdaAmsPublicBusinessCollectionRuns
                .SingleAsync(item => item.Id == runId, CancellationToken.None);
            run.StatusCode = UsdaAms공개사업체Archive상태Codes.실패;
            run.CompletedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            run.ErrorMessage = UsdaAms공개사업체TextNormalizer.Truncate(
                exception.Message,
                2000);
            await db.SaveChangesAsync(CancellationToken.None);
            logger.LogError(
                exception,
                "USDA AMS 공개 사업체 directory 수집 실패. RunId={RunId}",
                runId);
            throw;
        }
    }

    private async Task<DirectoryPersistenceResult> PersistDirectoryAsync(
        long runId,
        string directoryType,
        IReadOnlyList<UsdaAms공개사업체원본Row> rows,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        var acceptedRows = rows
            .Where(row =>
                !string.IsNullOrWhiteSpace(row.ExternalListingId)
                && !string.IsNullOrWhiteSpace(row.BusinessName))
            .GroupBy(
                row => row.ExternalListingId.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(item => item.SourceUpdatedAt)
                .First())
            .ToArray();
        var rejectedCount = rows.Count - acceptedRows.Length;
        var existing = await db.UsdaAmsPublicBusinessProfiles
            .Include(item => item.Products)
            .Where(item =>
                item.SourceKey
                    == UsdaAms공개사업체원천Keys.LocalFoodDirectories
                && item.DirectoryTypeCode == directoryType)
            .ToDictionaryAsync(
                item => item.ExternalListingId,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long insertedCount = 0;
        long updatedCount = 0;
        long unchangedCount = 0;

        foreach (var row in acceptedRows)
        {
            var candidate = CreateCandidate(row);
            seenIds.Add(candidate.ExternalListingId);
            if (!existing.TryGetValue(candidate.ExternalListingId, out var profile))
            {
                profile = new UsdaAms공개사업체Profile
                {
                    FirstCollectionRunId = runId,
                    LastCollectionRunId = runId,
                    FirstSeenAtUtc = observedAtUtc,
                    LastSeenAtUtc = observedAtUtc,
                    LastChangedAtUtc = observedAtUtc
                };
                ApplyCandidate(profile, candidate, replaceProducts: true);
                db.UsdaAmsPublicBusinessProfiles.Add(profile);
                insertedCount++;
                continue;
            }

            profile.LastCollectionRunId = runId;
            profile.LastSeenAtUtc = observedAtUtc;
            profile.IsCurrentlyListed = true;
            if (!string.Equals(
                    profile.SourceFingerprint,
                    candidate.SourceFingerprint,
                    StringComparison.Ordinal))
            {
                ApplyCandidate(profile, candidate, replaceProducts: true);
                profile.LastChangedAtUtc = observedAtUtc;
                updatedCount++;
            }
            else
            {
                unchangedCount++;
            }
        }

        long noLongerListedCount = 0;
        foreach (var profile in existing.Values.Where(item =>
                     item.IsCurrentlyListed
                     && !seenIds.Contains(item.ExternalListingId)))
        {
            profile.IsCurrentlyListed = false;
            profile.LastCollectionRunId = runId;
            profile.LastChangedAtUtc = observedAtUtc;
            noLongerListedCount++;
        }

        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return new DirectoryPersistenceResult(
            rows.Count,
            insertedCount,
            updatedCount,
            unchangedCount,
            noLongerListedCount,
            rejectedCount);
    }

    private static Candidate CreateCandidate(UsdaAms공개사업체원본Row row)
    {
        var externalListingId = UsdaAms공개사업체TextNormalizer.Truncate(
            row.ExternalListingId.Trim(),
            80);
        var businessName = UsdaAms공개사업체TextNormalizer.Truncate(
            UsdaAms공개사업체TextNormalizer.CollapseWhitespace(
                row.BusinessName),
            500);
        var location = UsdaAms공개사업체LocationParser.Parse(
            row.LocationAddress);
        var products = row.Products
            .Select(name => new ProductCandidate(
                UsdaAms공개사업체TextNormalizer.CreateProductKey(name),
                UsdaAms공개사업체TextNormalizer.Truncate(
                    UsdaAms공개사업체TextNormalizer.CollapseWhitespace(name),
                    300)))
            .Where(item => item.ProductKey.Length > 0)
            .DistinctBy(item => item.ProductKey, StringComparer.Ordinal)
            .OrderBy(item => item.ProductKey, StringComparer.Ordinal)
            .ToArray();
        var directorySlug = UsdaAms공개사업체DirectoryCatalog.GetSlug(
            row.DirectoryTypeCode);
        var officialListingUrl =
            "https://www.usdalocalfoodportal.com/fe/flisting/"
            + $"?lid={Uri.EscapeDataString(externalListingId)}"
            + $"&directory_type={Uri.EscapeDataString(directorySlug)}";
        var fingerprintInput = string.Join(
            '\n',
            row.DirectoryTypeCode,
            externalListingId,
            businessName,
            location.CityName,
            location.StateCode,
            row.EstablishedYear?.ToString(CultureInfo.InvariantCulture)
                ?? string.Empty,
            UsdaAms공개사업체TextNormalizer.CollapseWhitespace(
                row.LegalStatus),
            row.HasRetailChannel,
            row.HasWholesaleChannel,
            row.HasProducerService,
            row.HasProcurementService,
            row.SourceUpdatedAt?.ToString("O", CultureInfo.InvariantCulture)
                ?? string.Empty,
            string.Join('|', products.Select(item => item.ProductKey)));
        return new Candidate(
            UsdaAms공개사업체TextNormalizer.CreateSha256(
                $"{UsdaAms공개사업체원천Keys.LocalFoodDirectories}|{row.DirectoryTypeCode}|{externalListingId}"),
            row.DirectoryTypeCode,
            externalListingId,
            businessName,
            UsdaAms공개사업체TextNormalizer.Truncate(
                UsdaAms공개사업체TextNormalizer.NormalizeSearchText(
                    businessName),
                500),
            location.CityName,
            location.StateCode,
            location.PrecisionCode,
            row.EstablishedYear,
            UsdaAms공개사업체TextNormalizer.Truncate(
                UsdaAms공개사업체TextNormalizer.CollapseWhitespace(
                    row.LegalStatus),
                300),
            UsdaAms공개사업체TextNormalizer.Truncate(
                string.Join(
                    "; ",
                    products.Select(item => item.ProductName)),
                4000),
            row.HasRetailChannel,
            row.HasWholesaleChannel,
            row.HasProducerService,
            row.HasProcurementService,
            row.SourceUpdatedAt,
            officialListingUrl,
            UsdaAms공개사업체TextNormalizer.CreateSha256(fingerprintInput),
            products);
    }

    private static void ApplyCandidate(
        UsdaAms공개사업체Profile profile,
        Candidate candidate,
        bool replaceProducts)
    {
        profile.ProfileKey = candidate.ProfileKey;
        profile.SourceKey =
            UsdaAms공개사업체원천Keys.LocalFoodDirectories;
        profile.DirectoryTypeCode = candidate.DirectoryTypeCode;
        profile.ExternalListingId = candidate.ExternalListingId;
        profile.BusinessName = candidate.BusinessName;
        profile.BusinessNameNormalized = candidate.BusinessNameNormalized;
        profile.CityName = candidate.CityName;
        profile.StateCode = candidate.StateCode;
        profile.LocationPrecisionCode = candidate.LocationPrecisionCode;
        profile.EstablishedYear = candidate.EstablishedYear;
        profile.LegalStatus = candidate.LegalStatus;
        profile.ProductSummary = candidate.ProductSummary;
        profile.HasRetailChannel = candidate.HasRetailChannel;
        profile.HasWholesaleChannel = candidate.HasWholesaleChannel;
        profile.HasProducerService = candidate.HasProducerService;
        profile.HasProcurementService = candidate.HasProcurementService;
        profile.IsCurrentlyListed = true;
        profile.SourceUpdatedAt = candidate.SourceUpdatedAt;
        profile.OfficialListingUrl = candidate.OfficialListingUrl;
        profile.SourceFingerprint = candidate.SourceFingerprint;

        if (!replaceProducts)
        {
            return;
        }

        profile.Products.Clear();
        foreach (var product in candidate.Products)
        {
            profile.Products.Add(new UsdaAms공개사업체취급품목
            {
                ProductKey = product.ProductKey,
                ProductName = product.ProductName
            });
        }
    }

    private static UsdaAms공개사업체수집응답 ToResponse(
        UsdaAms공개사업체수집Run run,
        IReadOnlyList<string> directoryTypes)
        => new()
        {
            CollectionRunId = run.Id,
            DirectoryTypes = directoryTypes,
            CompletedDirectoryCount = run.CompletedDirectoryCount,
            FetchedCount = run.FetchedCount,
            InsertedCount = run.InsertedCount,
            UpdatedCount = run.UpdatedCount,
            UnchangedCount = run.UnchangedCount,
            NoLongerListedCount = run.NoLongerListedCount,
            RejectedCount = run.RejectedCount,
            CollectedAtUtc = run.CompletedAtUtc ?? run.StartedAtUtc,
            SourceUrl = run.SourceUrl,
            Notices =
            [
                "사업자가 자발적으로 등재한 공개 directory이며 USDA 인증·허가 명부가 아닙니다.",
                "상세 주소·좌표·연락처는 저장하지 않았습니다.",
                "AMS 시장가격 행의 시장·사무소·원산지를 업체로 추정하지 않았습니다."
            ]
        };

    private sealed record DirectoryPersistenceResult(
        long FetchedCount,
        long InsertedCount,
        long UpdatedCount,
        long UnchangedCount,
        long NoLongerListedCount,
        long RejectedCount);

    private sealed record ProductCandidate(
        string ProductKey,
        string ProductName);

    private sealed record Candidate(
        string ProfileKey,
        string DirectoryTypeCode,
        string ExternalListingId,
        string BusinessName,
        string BusinessNameNormalized,
        string CityName,
        string StateCode,
        string LocationPrecisionCode,
        int? EstablishedYear,
        string LegalStatus,
        string ProductSummary,
        bool HasRetailChannel,
        bool HasWholesaleChannel,
        bool HasProducerService,
        bool HasProcurementService,
        DateTime? SourceUpdatedAt,
        string OfficialListingUrl,
        string SourceFingerprint,
        IReadOnlyList<ProductCandidate> Products);
}
