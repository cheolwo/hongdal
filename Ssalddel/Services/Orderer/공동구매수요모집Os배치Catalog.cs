using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Services.Orderer;

public interface I공동구매수요모집Os배치Catalog
{
    공동구매수요모집Os배치Catalog응답 조회();
}

internal sealed record 공동구매수요모집Os배치등록항목(
    string 작업코드,
    bool 등록여부,
    bool Quartz등록여부,
    string 실행방식,
    string 스케줄,
    string 시간대);

internal sealed class 공동구매수요모집Os배치등록계획
{
    private readonly IReadOnlyDictionary<string, 공동구매수요모집Os배치등록항목> _항목;

    private 공동구매수요모집Os배치등록계획(
        IReadOnlyDictionary<string, 공동구매수요모집Os배치등록항목> 항목)
    {
        _항목 = 항목;
    }

    public 공동구매수요모집Os배치등록항목 조회(string 작업코드)
        => _항목.TryGetValue(작업코드, out var 항목)
            ? 항목
            : throw new InvalidOperationException($"등록되지 않은 OS 배치 코드입니다. WorkloadCode={작업코드}");

    public bool Quartz등록여부(string 작업코드)
        => 조회(작업코드).Quartz등록여부;

    public static 공동구매수요모집Os배치등록계획 생성(
        AgriculturalFisheriesBatchOptions 농수축산배치)
    {
        ArgumentNullException.ThrowIfNull(농수축산배치);

        var kamis일별등록 = 농수축산배치.Enabled && 농수축산배치.KamisDailyEnabled;
        var kamis월별등록 = 농수축산배치.Enabled && 농수축산배치.KamisMonthlyEnabled;
        var usda등록 = 농수축산배치.Enabled && 농수축산배치.UsdaMonthlyEnabled;
        var 기업근거등록 = 농수축산배치.Enabled
                        && 농수축산배치.IngredientCompanyResearchEnabled;

        var 항목 = new[]
        {
            Quartz항목(
                공동구매수요모집Os배치작업코드.Kamis일별가격수집,
                kamis일별등록,
                농수축산배치.KamisDailyCronExpression,
                농수축산배치.TimeZoneId),
            Quartz항목(
                공동구매수요모집Os배치작업코드.Kamis월별가격이력수집,
                kamis월별등록,
                농수축산배치.KamisMonthlyCronExpression,
                농수축산배치.TimeZoneId),
            Quartz항목(
                공동구매수요모집Os배치작업코드.UsdaNass월별가격수집,
                usda등록,
                농수축산배치.UsdaMonthlyCronExpression,
                농수축산배치.TimeZoneId),
            Quartz항목(
                공동구매수요모집Os배치작업코드.공식재료기업근거수집,
                기업근거등록,
                농수축산배치.IngredientCompanyResearchCronExpression,
                농수축산배치.TimeZoneId)
        }.ToDictionary(item => item.작업코드, StringComparer.Ordinal);

        return new 공동구매수요모집Os배치등록계획(항목);
    }

    public static 공동구매수요모집Os배치등록계획 빈계획()
        => 생성(new AgriculturalFisheriesBatchOptions());

    private static 공동구매수요모집Os배치등록항목 Quartz항목(
        string 작업코드,
        bool 등록여부,
        string 스케줄,
        string 시간대)
        => new(
            작업코드,
            등록여부,
            Quartz등록여부: 등록여부,
            공동구매수요모집Os배치실행방식코드.Quartz,
            스케줄,
            시간대);

}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Application,
    "1.0 내부 점검과 공동구매 판단에 필요한 공유 공공데이터 수집 상태·출처·실행 경계를 OS 카탈로그로 조립합니다.",
    ContractType = typeof(I공동구매수요모집Os배치Catalog),
    FlowOrder = 35,
    Effects = SsalddelCodeEffect.None,
    Boundary = "카탈로그 조회는 작업을 즉시 실행하거나 설정을 변경하지 않으며, 외부 API key와 개인정보를 반환하지 않습니다.")]
internal sealed class 공동구매수요모집Os배치Catalog : I공동구매수요모집Os배치Catalog
{
    private const string QuartzCron안내 = "Quartz cron";

    private readonly 공동구매수요모집Os배치등록계획 _등록계획;
    private readonly IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> _osOptions;
    private readonly IVersionFeatureFlagService _기능플래그;
    private readonly ISsalddelExecutionModePolicy _실행모드;

    public 공동구매수요모집Os배치Catalog(
        공동구매수요모집Os배치등록계획 등록계획,
        IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> osOptions,
        IVersionFeatureFlagService 기능플래그,
        ISsalddelExecutionModePolicy 실행모드)
    {
        _등록계획 = 등록계획;
        _osOptions = osOptions;
        _기능플래그 = 기능플래그;
        _실행모드 = 실행모드;
    }

    public 공동구매수요모집Os배치Catalog응답 조회()
    {
        var osOptions = _osOptions.CurrentValue;
        var 기능활성 = _기능플래그.IsEnabled(
            VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow);
        var osWorker활성 = 기능활성 && osOptions.Enabled;

        return new 공동구매수요모집Os배치Catalog응답
        {
            기능활성여부 = 기능활성,
            OsWorker활성여부 = osWorker활성,
            실행모드 = _실행모드.Mode.ToString(),
            시뮬레이션여부 = _실행모드.IsSimulation,
            작업목록 =
            [
                모집마감점검(osOptions, osWorker활성),
                공유작업(
                    공동구매수요모집Os배치작업코드.Kamis일별가격수집,
                    "KAMIS 일별 가격 근거 수집",
                    "한국 농수산물의 조사일·품목·등급·단위가 있는 관측값을 보관합니다.",
                    "공공가격수집",
                    "KAMIS 농산물유통정보",
                    osWorker활성: osWorker활성,
                    필요설정목록:
                    [
                        "AgriculturalFisheriesBatch:Enabled",
                        "AgriculturalFisheriesBatch:KamisDailyEnabled",
                        "PublicData:Kamis:CertificationKey",
                        "PublicData:Kamis:RequesterId"
                    ],
                    실행경계: "관측값은 판매 권고나 공동구매 확정가가 아니며 원문 규격과 조사일을 보존합니다."),
                공유작업(
                    공동구매수요모집Os배치작업코드.Kamis월별가격이력수집,
                    "KAMIS 월별 가격 이력 보강",
                    "최근 완료 월의 도매·소매 월평균 이력을 보관해 가격 추세 검토를 돕습니다.",
                    "공공가격수집",
                    "KAMIS 농산물유통정보",
                    osWorker활성: osWorker활성,
                    필요설정목록:
                    [
                        "AgriculturalFisheriesBatch:Enabled",
                        "AgriculturalFisheriesBatch:KamisMonthlyEnabled",
                        "PublicData:Kamis:CertificationKey",
                        "PublicData:Kamis:RequesterId"
                    ],
                    실행경계: "월평균과 일별 관측값을 같은 가격처럼 합치지 않고 빈 기간은 추정값으로 채우지 않습니다."),
                공유작업(
                    공동구매수요모집Os배치작업코드.UsdaNass월별가격수집,
                    "USDA NASS 월별 가격 근거 수집",
                    "미국 전국 농축수산물 생산자 수취가격의 기준월·단위를 보관합니다.",
                    "공공가격수집",
                    "USDA NASS Quick Stats",
                    osWorker활성: osWorker활성,
                    필요설정목록:
                    [
                        "AgriculturalFisheriesBatch:Enabled",
                        "AgriculturalFisheriesBatch:UsdaMonthlyEnabled",
                        "PublicData:UsdaNassQuickStats:ApiKey"
                    ],
                    실행경계: "미국 생산자 수취가격이며 소매가·한국 유통가·개별 공급 견적으로 해석하지 않습니다."),
                공유작업(
                    공동구매수요모집Os배치작업코드.공식재료기업근거수집,
                    "공식 재료 기업 근거 갱신",
                    "음식·재료 탐색에서 공동구매 후보를 이해할 수 있도록 공식 기업 관측 근거를 갱신합니다.",
                    "검토자료수집",
                    "공식 식품·사업자 공개 원천",
                    osWorker활성: osWorker활성,
                    필요설정목록:
                    [
                        "AgriculturalFisheriesBatch:Enabled",
                        "AgriculturalFisheriesBatch:IngredientCompanyResearchEnabled",
                        "관련 공식 공공데이터 API key"
                    ],
                    실행경계: "후보 근거만 수집하며 공급자 선정·연락·계약·추천 순위 확정은 하지 않습니다.")
            ]
        };
    }

    private static 공동구매수요모집Os배치작업응답 모집마감점검(
        GroupPurchaseDemandProcessManagerOptions options,
        bool os활성)
    {
        var interval = Math.Clamp(options.ScanIntervalSeconds, 10, 3600);
        return new 공동구매수요모집Os배치작업응답
        {
            작업코드 = 공동구매수요모집Os배치작업코드.모집마감장기정체점검,
            작업명 = "모집 마감·장기 정체 점검",
            목적 = "마감 또는 다음 점검 시각이 지난 모집 원장을 EDF·Aging 정책으로 재조율합니다.",
            작업유형 = "모집원장조율",
            실행방식 = 공동구매수요모집Os배치실행방식코드.HostedWorker,
            스케줄 = $"{interval}초 간격",
            시간대 = "UTC 원장 시각",
            등록여부 = true,
            Os사용활성여부 = os활성,
            공유인프라여부 = false,
            게시글작성여부 = false,
            상태코드 = 상태코드(등록여부: true, os활성: os활성),
            데이터출처 = "공동구매 수요·모집 Mongo 원장",
            필요설정목록 =
            [
                "VersionFeatureFlags:GroupPurchaseDemandWorkflow",
                "GroupPurchaseDemandOS:Enabled"
            ],
            상태안내 = 상태안내(
                등록여부: true,
                os활성: os활성,
                공유인프라여부: false),
            실행경계 = "모집 상태와 검토 큐만 재계산하며 주문·결제·공급자 선정·1.5 원장을 자동 생성하지 않습니다."
        };
    }

    private 공동구매수요모집Os배치작업응답 공유작업(
        string 작업코드,
        string 작업명,
        string 목적,
        string 작업유형,
        string 데이터출처,
        bool osWorker활성,
        IReadOnlyList<string> 필요설정목록,
        string 실행경계,
        IReadOnlyList<string>? 선행작업코드목록 = null)
    {
        var 등록 = _등록계획.조회(작업코드);
        var os활성 = osWorker활성 && 등록.등록여부;
        return new 공동구매수요모집Os배치작업응답
        {
            작업코드 = 작업코드,
            작업명 = 작업명,
            목적 = 목적,
            작업유형 = 작업유형,
            실행방식 = 등록.실행방식,
            스케줄 = $"{QuartzCron안내}: {등록.스케줄}",
            시간대 = 등록.시간대,
            등록여부 = 등록.등록여부,
            Os사용활성여부 = os활성,
            공유인프라여부 = true,
            게시글작성여부 = false,
            상태코드 = 상태코드(등록.등록여부, os활성),
            데이터출처 = 데이터출처,
            선행작업코드목록 = 선행작업코드목록 ?? [],
            필요설정목록 = 필요설정목록,
            상태안내 = 상태안내(등록.등록여부, os활성, 공유인프라여부: true),
            실행경계 = 실행경계
        };
    }

    private static string 상태코드(bool 등록여부, bool os활성)
        => !등록여부
            ? 공동구매수요모집Os배치상태코드.설정비활성
            : os활성
                ? 공동구매수요모집Os배치상태코드.Os활성
                : 공동구매수요모집Os배치상태코드.등록됨Os비활성;

    private static string 상태안내(
        bool 등록여부,
        bool os활성,
        bool 공유인프라여부)
    {
        if (!등록여부)
        {
            return "기본 비활성입니다. 필요 설정과 자격 증명을 검토해 켠 뒤 서버를 재시작해야 스케줄에 등록됩니다.";
        }

        if (os활성)
        {
            return "1.0 공동구매 수요·모집 OS가 이 작업을 사용할 수 있는 상태입니다.";
        }

        return 공유인프라여부
            ? "공유 배치에는 등록되어 있지만 1.0 OS 기능 플래그 또는 OS worker가 꺼져 있어 OS 사용 상태는 아닙니다."
            : "worker는 등록되어 있지만 1.0 OS 기능 플래그 또는 OS 설정이 꺼져 있습니다.";
    }
}
