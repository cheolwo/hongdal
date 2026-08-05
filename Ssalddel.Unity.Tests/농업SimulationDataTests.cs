using System.Reflection;
using System.Text.Json;
using Ssalddel.Unity.Data;

namespace Ssalddel.Tests.UnityData;

public sealed class 농업SimulationDataTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void GoldenScenarioPackage는_DataHash와_참조무결성을_검증한다()
    {
        var golden = LoadGoldenCase();

        var result = new 농업ScenarioValidator().Validate(golden.Package);

        Assert.True(
            result.IsValid,
            string.Join(Environment.NewLine, result.Issues.Select(issue => $"{issue.Code}: {issue.Path} - {issue.Message}")));
        Assert.Equal(golden.Package.Manifest.ExpectedDataHash, 농업ScenarioHashCalculator.Calculate(golden.Package));
    }

    [Fact]
    public void 같은_Package와_Command는_같은_결과를_재현한다()
    {
        var golden = LoadGoldenCase();
        var engine = new 농업SimulationEngine();

        var first = engine.Run(golden.Package, golden.Commands);
        var second = engine.Run(golden.Package, golden.Commands);

        Assert.Equal(first.FinalStateHash, second.FinalStateHash);
        Assert.True(
            string.Equals(golden.Expected.FinalStateHash, first.FinalStateHash, StringComparison.Ordinal),
            $"Final state hash mismatch. expected={golden.Expected.FinalStateHash}, actual={first.FinalStateHash}");
        Assert.Equal(golden.Expected.EventCount, first.Events.Length);
        Assert.Equal(golden.Expected.ProductionCostKrw, first.State.ProductionCostKrw);
        Assert.Equal(golden.Expected.HarvestQuantityKg, first.State.HarvestQuantityKg);

        var general = Assert.Single(
            first.State.SalesComparisons,
            item => item.SalesChannelKey == 판매방식Codes.General);
        Assert.Equal(golden.Expected.GeneralExpectedProfitKrw, general.ExpectedProfitKrw);

        var collective = Assert.Single(
            first.State.SalesComparisons,
            item => item.SalesChannelKey == 판매방식Codes.Collective);
        Assert.Equal(golden.Expected.CollectiveExpectedProfitKrw, collective.ExpectedProfitKrw);
        Assert.Contains("observed-price-is-not-a-quote", collective.Lineage.ExplanationCodes);
        Assert.Contains("sales-result-is-simulated", collective.Lineage.ExplanationCodes);
    }

    [Fact]
    public void 모호한_외부품목_Mapping과_호환되지_않는_단위를_거부한다()
    {
        var golden = LoadGoldenCase();
        golden.Package.ExternalMappings[0].QualityCode = 데이터품질Codes.AmbiguousMapping;
        golden.Package.MarketObservation.Evidence.NormalizedUnit = "KRW/box";

        var result = new 농업ScenarioValidator().Validate(golden.Package);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "ExternalMappingNotApproved");
        Assert.Contains(result.Issues, issue => issue.Code == "EvidenceUnitMismatch");
    }

    [Fact]
    public void ApiModel은_Mapper를_거쳐_Unity_GameModel로_변환된다()
    {
        var golden = LoadGoldenCase();
        var source = golden.Package.MarketObservation.Evidence;
        var apiModel = new 시장가격관측ApiModel
        {
            SchemaVersion = "1.0.0",
            ObservationKey = golden.Package.MarketObservation.ObservationKey,
            ExternalItemCode = golden.Package.ExternalMappings[0].ExternalCode,
            SourceKey = source.SourceKey,
            SourceRecordId = source.SourceRecordId,
            DatasetKey = source.DatasetKey,
            DatasetVersion = source.DatasetVersion,
            ObservedAt = source.ObservedAt,
            IngestedAt = source.IngestedAt,
            RegionKey = source.RegionKey,
            MarketStageKey = source.MarketStageKey,
            Price = source.OriginalValue,
            Unit = source.OriginalUnit,
            CurrencyCode = source.CurrencyCode,
            QualityCode = source.QualityCode,
            FreshnessCode = source.FreshnessCode,
            LicenseOrTermsReference = source.LicenseOrTermsReference,
            Limitations = source.Limitations,
            PayloadHash = source.PayloadHash,
        };

        var result = new 시장가격관측Mapper().Map(apiModel, golden.Package.ExternalMappings[0]);

        Assert.True(result.IsMapped, string.Join(", ", result.ErrorCodes));
        Assert.NotNull(result.Value);
        Assert.Equal(golden.Package.Crop.CropKey, result.Value.CropKey);
        Assert.Equal(2600m, result.Value.PriceKrwPerKg);
        Assert.NotSame(apiModel, result.Value);
    }

    [Fact]
    public void Unity_DataAssembly는_ServerContracts를_참조하지_않는다()
    {
        var referencedNames = typeof(농업ScenarioPackage)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.DoesNotContain("Ssalddel.Contracts", referencedNames);
        Assert.DoesNotContain("Ssalddel.Community", referencedNames);
        Assert.DoesNotContain("UnityEngine", referencedNames);
    }

    [Fact]
    public async Task DataManager는_Fixture상태를_Live와_구분한다()
    {
        var golden = LoadGoldenCase();
        var repository = new StubScenarioPackageRepository(
            golden.Package,
            ScenarioPackageSourceCodes.Fixture);
        var manager = new DataManager(repository);

        var loaded = await manager.LoadScenarioAsync(
            golden.Package.Manifest.ScenarioKey,
            golden.Package.Manifest.ScenarioVersion);

        Assert.NotNull(loaded);
        Assert.Equal(DataLoadStateCodes.ReadyFixture, manager.StateCode);
        Assert.Same(golden.Package, manager.CurrentScenario);
        Assert.Equal(string.Empty, manager.ErrorCode);
    }

    [Fact]
    public async Task DataManager는_변조된_Package를_Invalid로_차단한다()
    {
        var golden = LoadGoldenCase();
        golden.Package.Crop.BaseYieldKg = 999;
        var repository = new StubScenarioPackageRepository(
            golden.Package,
            ScenarioPackageSourceCodes.Cached);
        var manager = new DataManager(repository);

        var loaded = await manager.LoadScenarioAsync(
            golden.Package.Manifest.ScenarioKey,
            golden.Package.Manifest.ScenarioVersion);

        Assert.Null(loaded);
        Assert.Null(manager.CurrentScenario);
        Assert.Equal(DataLoadStateCodes.Invalid, manager.StateCode);
        Assert.Equal("DataHashMismatch", manager.ErrorCode);
    }

    private static GoldenCaseDocument LoadGoldenCase()
    {
        var assembly = typeof(농업SimulationDataTests).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith("potato-basic-kr-001.v1.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Golden fixture를 열 수 없습니다: {resourceName}");
        return JsonSerializer.Deserialize<GoldenCaseDocument>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Golden fixture를 역직렬화할 수 없습니다.");
    }

    private sealed class GoldenCaseDocument
    {
        public 농업ScenarioPackage Package { get; set; } = new();

        public 농업SimulationCommand[] Commands { get; set; } = [];

        public GoldenExpectedResult Expected { get; set; } = new();
    }

    private sealed class GoldenExpectedResult
    {
        public string FinalStateHash { get; set; } = string.Empty;

        public int EventCount { get; set; }

        public long ProductionCostKrw { get; set; }

        public decimal HarvestQuantityKg { get; set; }

        public long GeneralExpectedProfitKrw { get; set; }

        public long CollectiveExpectedProfitKrw { get; set; }
    }

    private sealed class StubScenarioPackageRepository(
        농업ScenarioPackage package,
        string sourceCode) : IScenarioPackageRepository
    {
        public Task<ScenarioPackageLoadResult> LoadAsync(
            string scenarioKey,
            string scenarioVersion,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ScenarioPackageLoadResult
            {
                Package = package,
                SourceCode = sourceCode,
            });
        }
    }
}
