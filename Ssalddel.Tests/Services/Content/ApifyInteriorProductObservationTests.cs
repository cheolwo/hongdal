using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Ssalddel.Interior.Contracts;
using Ssalddel.Services.External.Apify;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Content;

public sealed class ApifyInteriorProductObservationTests
{
    [Fact]
    public void AmazonAndAlibabaNormalizersOnlyKeepSafeReferenceFields()
    {
        var observedAt = new DateTimeOffset(2026, 8, 24, 0, 0, 0, TimeSpan.Zero);
        using var amazonJson = JsonDocument.Parse("""
            {"asin":"A-1","title":"Small bedside lamp","url":"https://www.amazon.com/dp/A-1","price":"99.00","image":"https://example.invalid/a.png"}
            """);
        using var alibabaJson = JsonDocument.Parse("""
            {"productId":"B-1","subject":"Steel kitchen tool","itemUrl":"https://www.alibaba.com/product-detail/B-1","price":"3.00"}
            """);

        var amazon = new ApifyAmazonInteriorProductNormalizer().Normalize(
            amazonJson.RootElement,
            Source("amazon-source", InteriorProductObservationCodes.Amazon),
            observedAt);
        var alibaba = new ApifyAlibabaInteriorProductNormalizer().Normalize(
            alibabaJson.RootElement,
            Source("alibaba-source", InteriorProductObservationCodes.Alibaba),
            observedAt);

        Assert.NotNull(amazon);
        Assert.NotNull(alibaba);
        Assert.Equal("Small bedside lamp", amazon!.OriginalTitle);
        Assert.Equal("Steel kitchen tool", alibaba!.OriginalTitle);
        Assert.DoesNotContain("99.00", JsonSerializer.Serialize(amazon));
        Assert.DoesNotContain("image", JsonSerializer.Serialize(amazon), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExplicitApprovalBuildsImmutableHashedReferenceCatalog()
    {
        var observation = new NormalizedInteriorProductObservation(
            "interior-observation:amazon:lamp",
            InteriorProductObservationCodes.Amazon,
            "A-1",
            "Small bedside lamp",
            "https://www.amazon.com/dp/A-1",
            new DateTimeOffset(2026, 8, 24, 0, 0, 0, TimeSpan.Zero),
            new string('a', 64),
            "actor.r1");
        var service = new InteriorReferenceApprovalService();

        var catalog = service.BuildCatalog(
            "catalog:town-house",
            "catalog.r1",
            [observation],
            [
                new InteriorReferenceApprovalDecision(
                    observation.ObservationStableId,
                    true,
                    "Lighting",
                    [InteriorLayoutCodes.Bedroom],
                    ["BedsideLighting"]),
            ]);

        var item = Assert.Single(catalog.Items);
        Assert.StartsWith("interior-reference:", item.ReferenceStableId, StringComparison.Ordinal);
        Assert.Equal(InteriorProductObservationCodes.ReferenceOnly, item.UsageRestrictionCode);
        Assert.Equal(64, catalog.CatalogHashSha256.Length);
        Assert.Equal(catalog.CatalogHashSha256,
            Ssalddel.Interior.Domain.InteriorLayoutHash.ComputeCatalogHash(catalog));
    }

    [Fact]
    public async Task CollectorIsFailClosedUntilProductObservationIsExplicitlyEnabled()
    {
        var collector = new Apify상품관측Collector(
            new NeverCalledGateway(),
            new NeverCalledRawStore(),
            Options.Create(new ApifyInteriorProductsOptions
            {
                Enabled = false,
                Sources = [Source("amazon-source", InteriorProductObservationCodes.Amazon)],
            }));
        using var input = JsonDocument.Parse("{}");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => collector.CollectAsync(
            "amazon-source",
            input.RootElement,
            DateTimeOffset.UtcNow,
            CancellationToken.None));

        Assert.Contains("비활성화", exception.Message);
    }

    [Fact]
    public async Task EnabledCollectorStoresPrivateRawSnapshotBeforeNormalization()
    {
        var temporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "ssalddel-interior-observation-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var raw = JsonDocument.Parse("""
                {"asin":"A-1","title":"Lamp","url":"https://www.amazon.com/dp/A-1"}
                """);
            var options = new ApifyInteriorProductsOptions
            {
                Enabled = true,
                RawObservationDirectory = "private-raw",
                Sources = [Source("amazon-source", InteriorProductObservationCodes.Amazon)],
            };
            var collector = new Apify상품관측Collector(
                new FixtureGateway(raw.RootElement.Clone()),
                new FileInteriorProductRawObservationStore(
                    new FixtureHostEnvironment(temporaryRoot),
                    Options.Create(options)),
                Options.Create(options));
            using var input = JsonDocument.Parse("{}");

            var batch = await collector.CollectAsync(
                "amazon-source",
                input.RootElement,
                new DateTimeOffset(2026, 8, 24, 0, 0, 0, TimeSpan.Zero),
                CancellationToken.None);

            Assert.Single(batch.RawItems);
            Assert.Equal(64, batch.RawSnapshot.RawObservationHashSha256.Length);
            Assert.Single(Directory.GetFiles(
                Path.Combine(temporaryRoot, "private-raw"),
                "*.json",
                SearchOption.AllDirectories));
        }
        finally
        {
            if (Directory.Exists(temporaryRoot)) Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    private static ApifyInteriorProductSourceOptions Source(string id, string marketplace)
        => new()
        {
            SourceStableId = id,
            MarketplaceCode = marketplace,
            ActorId = "owner~actor",
            ActorBuild = "1.0.0",
            InputContractRevision = "input.r1",
            OutputContractRevision = "output.r1",
            NormalizerCode = marketplace + ".r1",
            Enabled = true,
            TermsReviewStatus = InteriorProductObservationCodes.Approved,
        };

    private sealed class NeverCalledGateway : IApifyActorGateway
    {
        public Task<ApifyActorSyncResult> RunSyncGetDatasetItemsAsync(
            ApifyActorSyncRequest request,
            CancellationToken cancellationToken)
            => throw new Xunit.Sdk.XunitException("Gateway should not be called.");
    }

    private sealed class FixtureGateway(JsonElement item) : IApifyActorGateway
    {
        public Task<ApifyActorSyncResult> RunSyncGetDatasetItemsAsync(
            ApifyActorSyncRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult(new ApifyActorSyncResult(request.ActorId, [item]));
    }

    private sealed class NeverCalledRawStore : IInteriorProductRawObservationStore
    {
        public Task<InteriorProductRawObservationSnapshot> StoreAsync(
            ApifyInteriorProductSourceOptions source,
            IReadOnlyList<JsonElement> rawItems,
            DateTimeOffset collectedAtUtc,
            CancellationToken cancellationToken)
            => throw new Xunit.Sdk.XunitException("Raw store should not be called.");
    }

    private sealed class FixtureHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
