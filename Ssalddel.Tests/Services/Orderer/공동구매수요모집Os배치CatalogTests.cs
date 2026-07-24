using System.Reflection;
using Microsoft.Extensions.Options;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers.Admin.Orderer;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매수요모집Os배치CatalogTests
{
    [Fact]
    public void 배치Catalog관리Api_기능이꺼진상태도보는읽기전용Bootstrap이다()
    {
        var version = typeof(공동구매수요모집Os배치AdminController)
            .GetCustomAttribute<SsalddelApiVersionAttribute>(inherit: true);

        Assert.NotNull(version);
        Assert.True(string.IsNullOrWhiteSpace(version!.FeatureKey));
        Assert.Equal(
            VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow,
            version.WorkflowKey);
    }

    [Fact]
    public void Os등록계획은_공공데이터수집만포함하고_커뮤니티글쓰기작업을소유하지않는다()
    {
        var plan = 공동구매수요모집Os배치등록계획.생성(
            new AgriculturalFisheriesBatchOptions
            {
                Enabled = true,
                PublishCommunityPriceBriefs = true,
                KamisDailyEnabled = true,
                UsdaMonthlyEnabled = true
            });

        Assert.True(plan.조회(
            공동구매수요모집Os배치작업코드.Kamis일별가격수집).등록여부);
        Assert.Throws<InvalidOperationException>(() =>
            plan.조회("CommunityKamisPriceBrief"));
        Assert.Throws<InvalidOperationException>(() =>
            plan.조회("CommunityUsdaNassPriceBrief"));
    }

    [Fact]
    public void Catalog_1점0내부작업과가격근거파이프라인을활성상태로노출한다()
    {
        var plan = 공동구매수요모집Os배치등록계획.생성(
            new AgriculturalFisheriesBatchOptions
            {
                Enabled = true,
                PublishCommunityPriceBriefs = true,
                KamisDailyEnabled = true,
                KamisMonthlyEnabled = true,
                UsdaMonthlyEnabled = true,
                IngredientCompanyResearchEnabled = false
            });
        var catalog = new 공동구매수요모집Os배치Catalog(
            plan,
            new StaticOptionsMonitor<GroupPurchaseDemandOsOptions>(new GroupPurchaseDemandOsOptions
            {
                Enabled = true,
                ScanIntervalSeconds = 60
            }),
            new StubFeatureFlagService(enabled: true),
            new StubExecutionModePolicy(SsalddelExecutionMode.Simulation));

        var result = catalog.조회();

        Assert.True(result.기능활성여부);
        Assert.True(result.OsWorker활성여부);
        Assert.True(result.시뮬레이션여부);
        Assert.Equal(5, result.작업목록.Count);
        Assert.Equal(
            공동구매수요모집Os배치상태코드.Os활성,
            result.작업목록.Single(item =>
                item.작업코드 == 공동구매수요모집Os배치작업코드.Kamis일별가격수집).상태코드);
        Assert.All(result.작업목록, item => Assert.False(item.게시글작성여부));
        Assert.Equal(
            공동구매수요모집Os배치상태코드.설정비활성,
            result.작업목록.Single(item =>
                item.작업코드 == 공동구매수요모집Os배치작업코드.공식재료기업근거수집).상태코드);
    }

    [Fact]
    public void Catalog_기본설정에서는공유외부배치를활성화하지않는다()
    {
        var catalog = new 공동구매수요모집Os배치Catalog(
            공동구매수요모집Os배치등록계획.빈계획(),
            new StaticOptionsMonitor<GroupPurchaseDemandOsOptions>(new GroupPurchaseDemandOsOptions()),
            new StubFeatureFlagService(enabled: false),
            new StubExecutionModePolicy(SsalddelExecutionMode.Simulation));

        var result = catalog.조회();

        Assert.False(result.기능활성여부);
        Assert.False(result.OsWorker활성여부);
        Assert.All(
            result.작업목록.Where(item => item.공유인프라여부),
            item => Assert.False(item.Os사용활성여부));
        Assert.All(
            result.작업목록.Where(item => item.공유인프라여부),
            item => Assert.Equal(
                공동구매수요모집Os배치상태코드.설정비활성,
                item.상태코드));
    }

    private sealed class StubFeatureFlagService(bool enabled) : IVersionFeatureFlagService
    {
        public bool IsEnabled(string featureKey)
            => featureKey == VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow && enabled;

        public IReadOnlyDictionary<string, bool> GetAll()
            => new Dictionary<string, bool>
            {
                [VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow] = enabled
            };
    }

    private sealed class StubExecutionModePolicy(SsalddelExecutionMode mode)
        : ISsalddelExecutionModePolicy
    {
        public SsalddelExecutionMode Mode { get; } = mode;
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
