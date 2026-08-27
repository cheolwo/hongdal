using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Interior.Contracts;
using Ssalddel.Interior.Domain;
using 살뜰.Services.Options;

namespace Ssalddel.Services.External.Apify;

public static class InteriorProductObservationCodes
{
    public const string Amazon = "Amazon";
    public const string Alibaba = "Alibaba";
    public const string Approved = "Approved";
    public const string ReferenceOnly = "ReferenceOnly";
}

public sealed record ApifyInteriorProductObservationBatch(
    ApifyInteriorProductSourceOptions Source,
    DateTimeOffset CollectedAtUtc,
    InteriorProductRawObservationSnapshot RawSnapshot,
    IReadOnlyList<JsonElement> RawItems);

public sealed record InteriorProductRawObservationSnapshot(
    string SourceStableId,
    string RawObservationHashSha256,
    string PrivateStorageRelativePath,
    DateTimeOffset CollectedAtUtc,
    int ItemCount);

public interface IInteriorProductRawObservationStore
{
    Task<InteriorProductRawObservationSnapshot> StoreAsync(
        ApifyInteriorProductSourceOptions source,
        IReadOnlyList<JsonElement> rawItems,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken);
}

public sealed class FileInteriorProductRawObservationStore : IInteriorProductRawObservationStore
{
    private readonly string storageRoot;

    public FileInteriorProductRawObservationStore(
        IHostEnvironment environment,
        IOptions<ApifyInteriorProductsOptions> options)
    {
        var contentRoot = Path.GetFullPath(environment.ContentRootPath);
        storageRoot = Path.GetFullPath(Path.Combine(
            contentRoot,
            options.Value.RawObservationDirectory ?? string.Empty));
        if (!storageRoot.StartsWith(contentRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("실내 상품 Raw 저장 경로는 서버 ContentRoot 내부여야 합니다.");
    }

    public async Task<InteriorProductRawObservationSnapshot> StoreAsync(
        ApifyInteriorProductSourceOptions source,
        IReadOnlyList<JsonElement> rawItems,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var content = JsonSerializer.SerializeToUtf8Bytes(rawItems);
        var hash = ApifyInteriorProductNormalizerBase.Hash(Encoding.UTF8.GetString(content));
        var sourceDirectory = Path.Combine(
            storageRoot,
            ApifyInteriorProductNormalizerBase.Hash(source.SourceStableId)[..24]);
        Directory.CreateDirectory(sourceDirectory);
        var fullPath = Path.Combine(sourceDirectory, hash + ".json");
        if (!File.Exists(fullPath))
        {
            await using var stream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous);
            await stream.WriteAsync(content, cancellationToken);
        }
        return new InteriorProductRawObservationSnapshot(
            source.SourceStableId,
            hash,
            Path.GetRelativePath(storageRoot, fullPath).Replace('\\', '/'),
            collectedAtUtc,
            rawItems.Count);
    }
}

public sealed record NormalizedInteriorProductObservation(
    string ObservationStableId,
    string MarketplaceCode,
    string ExternalProductId,
    string OriginalTitle,
    string SourceUrl,
    DateTimeOffset ObservedAtUtc,
    string RawObservationHashSha256,
    string SourceRevision);

public interface IApifyInteriorProductNormalizer
{
    string MarketplaceCode { get; }
    NormalizedInteriorProductObservation? Normalize(
        JsonElement raw,
        ApifyInteriorProductSourceOptions source,
        DateTimeOffset observedAtUtc);
}

public sealed class ApifyAmazonInteriorProductNormalizer : ApifyInteriorProductNormalizerBase
{
    public override string MarketplaceCode => InteriorProductObservationCodes.Amazon;
    protected override string[] IdFields => ["asin", "productId", "id"];
    protected override string[] TitleFields => ["title", "name", "productTitle"];
    protected override string[] UrlFields => ["url", "productUrl", "detailPageUrl"];
}

public sealed class ApifyAlibabaInteriorProductNormalizer : ApifyInteriorProductNormalizerBase
{
    public override string MarketplaceCode => InteriorProductObservationCodes.Alibaba;
    protected override string[] IdFields => ["productId", "id", "itemId"];
    protected override string[] TitleFields => ["title", "name", "subject"];
    protected override string[] UrlFields => ["url", "productUrl", "itemUrl"];
}

public abstract class ApifyInteriorProductNormalizerBase : IApifyInteriorProductNormalizer
{
    public abstract string MarketplaceCode { get; }
    protected abstract string[] IdFields { get; }
    protected abstract string[] TitleFields { get; }
    protected abstract string[] UrlFields { get; }

    public NormalizedInteriorProductObservation? Normalize(
        JsonElement raw,
        ApifyInteriorProductSourceOptions source,
        DateTimeOffset observedAtUtc)
    {
        if (raw.ValueKind != JsonValueKind.Object
            || !string.Equals(source.MarketplaceCode, MarketplaceCode, StringComparison.OrdinalIgnoreCase))
            return null;
        var id = FirstString(raw, IdFields);
        var title = FirstString(raw, TitleFields);
        var url = FirstString(raw, UrlFields);
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title) || !SafeMarketplaceUrl(url))
            return null;
        var rawHash = Hash(raw.GetRawText());
        return new NormalizedInteriorProductObservation(
            "interior-observation:" + MarketplaceCode.ToLowerInvariant() + ":" + Hash(id)[..24],
            MarketplaceCode,
            id.Trim(),
            title.Trim(),
            url.Trim(),
            observedAtUtc,
            rawHash,
            string.Join("|", new[]
            {
                source.ActorId.Trim(), source.ActorBuild.Trim(), source.OutputContractRevision.Trim(),
            }));
    }

    private static string FirstString(JsonElement value, IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            if (value.TryGetProperty(name, out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
                return property.GetString()!;
        }
        return string.Empty;
    }

    internal static bool SafeMarketplaceUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return false;
        var host = uri.Host.ToLowerInvariant();
        return host == "amazon.com" || host.EndsWith(".amazon.com", StringComparison.Ordinal)
               || host == "amazon.co.kr" || host.EndsWith(".amazon.co.kr", StringComparison.Ordinal)
               || host == "alibaba.com" || host.EndsWith(".alibaba.com", StringComparison.Ordinal);
    }

    internal static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public interface IApify상품관측Collector
{
    Task<ApifyInteriorProductObservationBatch> CollectAsync(
        string sourceStableId,
        JsonElement input,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken);
}

public sealed class Apify상품관측Collector : IApify상품관측Collector
{
    private readonly IApifyActorGateway gateway;
    private readonly IInteriorProductRawObservationStore rawStore;
    private readonly ApifyInteriorProductsOptions options;

    public Apify상품관측Collector(
        IApifyActorGateway gateway,
        IInteriorProductRawObservationStore rawStore,
        IOptions<ApifyInteriorProductsOptions> options)
    {
        this.gateway = gateway;
        this.rawStore = rawStore;
        this.options = options.Value;
    }

    public async Task<ApifyInteriorProductObservationBatch> CollectAsync(
        string sourceStableId,
        JsonElement input,
        DateTimeOffset collectedAtUtc,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
            throw new InvalidOperationException("실내 상품 관측 수집이 비활성화되어 있습니다.");
        var source = options.Sources.SingleOrDefault(value =>
            string.Equals(value.SourceStableId, sourceStableId?.Trim(), StringComparison.Ordinal))
            ?? throw new KeyNotFoundException("등록되지 않은 실내 상품 관측 Source입니다.");
        if (!source.Enabled || !string.Equals(
                source.TermsReviewStatus,
                InteriorProductObservationCodes.Approved,
                StringComparison.Ordinal))
            throw new InvalidOperationException("Source 활성화와 이용 조건 승인이 모두 필요합니다.");

        var result = await gateway.RunSyncGetDatasetItemsAsync(
            new ApifyActorSyncRequest(
                source.ActorId,
                input,
                options.ActorTimeoutSeconds,
                options.MemoryMegabytes,
                options.MaxDatasetItems,
                options.MaxTotalChargeUsd,
                source.ActorBuild),
            cancellationToken);
        var rawSnapshot = await rawStore.StoreAsync(
            source,
            result.Items,
            collectedAtUtc,
            cancellationToken);
        return new ApifyInteriorProductObservationBatch(source, collectedAtUtc, rawSnapshot, result.Items);
    }
}

public sealed record InteriorReferenceApprovalDecision(
    string ObservationStableId,
    bool Approved,
    string CategoryCode,
    string[] RoomRoleCodes,
    string[] PlacementRoleCodes,
    string UsageRestrictionCode = InteriorProductObservationCodes.ReferenceOnly);

public sealed class InteriorReferenceApprovalService
{
    public ApprovedInteriorReferenceCatalog BuildCatalog(
        string catalogStableId,
        string catalogRevision,
        IEnumerable<NormalizedInteriorProductObservation> observations,
        IEnumerable<InteriorReferenceApprovalDecision> decisions)
    {
        if (string.IsNullOrWhiteSpace(catalogStableId))
            throw new ArgumentException("Catalog StableId가 필요합니다.", nameof(catalogStableId));
        if (string.IsNullOrWhiteSpace(catalogRevision))
            throw new ArgumentException("Catalog Revision이 필요합니다.", nameof(catalogRevision));
        var decisionMap = decisions.ToDictionary(value => value.ObservationStableId, StringComparer.Ordinal);
        var approved = observations
            .Where(value => decisionMap.TryGetValue(value.ObservationStableId, out var decision)
                            && decision.Approved
                            && !string.IsNullOrWhiteSpace(decision.CategoryCode)
                            && ApifyInteriorProductNormalizerBase.SafeMarketplaceUrl(value.SourceUrl))
            .GroupBy(value => value.ObservationStableId, StringComparer.Ordinal)
            .Select(group => group.OrderBy(value => value.RawObservationHashSha256, StringComparer.Ordinal).First())
            .OrderBy(value => value.ObservationStableId, StringComparer.Ordinal)
            .Select(value =>
            {
                var decision = decisionMap[value.ObservationStableId];
                return new ApprovedInteriorReference
                {
                    ReferenceStableId = value.ObservationStableId.Replace(
                        "interior-observation:", "interior-reference:", StringComparison.Ordinal),
                    MarketplaceCode = value.MarketplaceCode,
                    CategoryCode = decision.CategoryCode.Trim(),
                    RoomRoleCodes = decision.RoomRoleCodes.Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                    PlacementRoleCodes = decision.PlacementRoleCodes.Distinct(StringComparer.Ordinal)
                        .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                    ApprovedOriginalTitle = value.OriginalTitle,
                    SourceUrl = value.SourceUrl,
                    ObservedAtUtc = value.ObservedAtUtc.ToUniversalTime().ToString("O"),
                    RawObservationHashSha256 = value.RawObservationHashSha256,
                    SourceRevision = value.SourceRevision,
                    UsageRestrictionCode = decision.UsageRestrictionCode,
                };
            })
            .ToArray();
        var catalog = new ApprovedInteriorReferenceCatalog
        {
            StableId = catalogStableId.Trim(),
            Revision = catalogRevision.Trim(),
            Items = approved,
        };
        catalog.CatalogHashSha256 = InteriorLayoutHash.ComputeCatalogHash(catalog);
        return catalog;
    }
}
