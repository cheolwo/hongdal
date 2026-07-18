using System.Globalization;
using System.Text.RegularExpressions;
using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Contracts.Common.PublicData;
using Microsoft.Extensions.Options;
using 홍달.Services.External.PublicData;
using 홍달.Services.Options;

namespace Hongdal.Services.AgriculturalFisheries.Information;

public sealed class AgriculturalFisheriesInformationService : IAgriculturalFisheriesInformationService
{
    private static readonly TimeSpan KoreaOffset = TimeSpan.FromHours(9);

    private static readonly IReadOnlyDictionary<string, string> CategoryLabels =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["100"] = "식량작물",
            ["200"] = "채소류",
            ["300"] = "특용작물",
            ["400"] = "과일류",
            ["500"] = "축산물",
            ["600"] = "수산물"
        };

    private readonly IFoodPriceCrosswalkCatalog _crosswalkCatalog;
    private readonly IAtDomesticFoodPriceLookupService _domesticPriceLookupService;
    private readonly PublicDataOptions _options;

    public AgriculturalFisheriesInformationService(
        IFoodPriceCrosswalkCatalog crosswalkCatalog,
        IAtDomesticFoodPriceLookupService domesticPriceLookupService,
        IOptions<PublicDataOptions> options)
    {
        _crosswalkCatalog = crosswalkCatalog;
        _domesticPriceLookupService = domesticPriceLookupService;
        _options = options.Value;
    }

    public AgriculturalFisheriesInformationOverviewResponse GetOverview()
    {
        var dataGoKrConfigured = !string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey)
            || !string.IsNullOrWhiteSpace(_options.ServiceKey);
        var atConfigured = dataGoKrConfigured || !string.IsNullOrWhiteSpace(_options.AtFoodPrices.ServiceKey);
        var customsConfigured = dataGoKrConfigured || !string.IsNullOrWhiteSpace(_options.CustomsTradeStatistics.ServiceKey);
        var nassConfigured = !string.IsNullOrWhiteSpace(_options.UsdaNassQuickStats.ApiKey);
        var australiaCatalog = 호주농수산식품가격Catalog.Build();

        return new AgriculturalFisheriesInformationOverviewResponse
        {
            SupportedMarketCodes = ["KR", "US", "AU"],
            Positioning = "공공데이터를 읽고 비교하고 수입 준비 절차의 확인 기록을 함께 관리하는 정보 기반입니다. 주문·계약·배차를 만들지 않습니다.",
            AllowsReadinessRecordWrites = true,
            AllowsTransactionExecution = false,
            BrokerageBoundaryNote = "현재 단계에서는 화물 주선, 운송계약 체결, 운임 중개, 기사 배정과 수수료 정산을 제공하지 않습니다.",
            ReadinessRecordBoundaryNote = "읽기 전용 공공정보와 별도로 참여자는 육류 수입 준비 상태·증빙 메타데이터·질문·양측 확인을 기록할 수 있지만, 이 기록은 거래 실행이나 정부기관의 공식 결정을 의미하지 않습니다.",
            DataSources =
            [
                Source(
                    "at-daily-wholesale-retail-food-price",
                    "한국농수산식품유통공사(aT)",
                    "일별 도·소매 가격정보",
                    "농축수산물의 국내 중도매·소매 가격",
                    "일별 조사",
                    atConfigured,
                    "https://www.data.go.kr/data/15156057/openapi.do",
                    "가격은 kg 기준으로 정규화하며 품질·등급·포장 차이를 함께 안내합니다."),
                Source(
                    "customs-hs-country-import-statistics",
                    "관세청",
                    "품목별 국가별 수출입실적",
                    "HS 코드·국가·월별 수입금액과 순중량",
                    "월별 통계",
                    customsConfigured,
                    "https://www.data.go.kr/data/15100475/openapi.do",
                    "국내 가격의 비교 맥락으로만 사용하며 실제 매입가나 운송 견적으로 보지 않습니다."),
                Source(
                    미국농수산가격출처Keys.UsdaNassQuickStats,
                    "미국 농무부 농업통계청(USDA NASS)",
                    "Quick Stats 농수산물 가격·판매 통계",
                    "미국 농작물·축산물·양식 수산물의 공식 집계 가격과 판매 통계",
                    "품목·조사 프로그램별 상이",
                    nassConfigured,
                    "https://quickstats.nass.usda.gov/api",
                    "미국 공식 품목명과 조사 단위를 유지하며 국내 aT 가격과 직접 같은 값으로 보지 않습니다."),
                .. australiaCatalog.Sources.Select(AustraliaSource)
            ],
            Capabilities =
            [
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "SupportedItemCatalog",
                    Label = "지원 품목 찾기",
                    Description = "검토된 HS-aT 연결표에서 농축수산물과 매칭 품질을 검색합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/items"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "DomesticPriceInformation",
                    Label = "국내 가격 정보",
                    Description = "기준일 주변의 aT 중도매·소매 가격과 최신 조사일을 제공합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/items/{hsCode}/domestic-price"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "ImportPriceContext",
                    Label = "수입 통계 비교",
                    Description = "기존 HS 가격 비교 기능에서 관세청 CIF 통계단가와 국내가격을 나란히 봅니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/customs/hs-codes/{hsCode}/food-price-comparison"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "MeatImportReadinessCollaboration",
                    Label = "육류 수입 준비도 협업",
                    Description = "한국 수입업자와 해외 작업장이 같은 절차도에서 상태, 증빙 메타데이터, 질문·이의와 양측 확인을 관리합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/import-readiness/diagram"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "UnitedStatesPriceInformation",
                    Label = "미국 농수산물 가격 정보",
                    Description = "USDA NASS의 농산물과 양식 수산물 가격·판매 집계 통계를 조회합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/us-prices"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "UnitedStatesOperatorInformationSources",
                    Label = "미국 농어업경영체 정보 원천",
                    Description = "개별 기록의 비공개 경계와 인증·검사·자발적 등재·지역 허가 목적별 공개 명부를 구분해 제공합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/us-operator-information-sources"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "AustraliaFoodPriceIndexes",
                    Label = "호주 식품 가격지수",
                    Description = "ABS의 8개 주도시 가중평균과 도시별 월별 식품·육류·수산물·유제품·과일·채소 소비자 가격지수를 조회합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/au-food-price-indexes"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "AustraliaFoodPriceSourceCatalog",
                    Label = "호주 농수산물 가격 원천 카탈로그",
                    Description = "ABS 자동 조회와 ABARES 농축산·원예·수산물 파일·참고 원천의 수집 경계를 구분합니다.",
                    AvailableNow = true,
                    Endpoint = "GET /api/v1/agricultural-fisheries/au-food-price-indexes/catalog"
                },
                new AgriculturalFisheriesCapabilityResponse
                {
                    Code = "FreightBrokerage",
                    Label = "화물 주선·중개",
                    Description = "업계 이해와 운영 요건이 충분히 축적된 뒤 별도 모듈로 검토합니다.",
                    AvailableNow = false
                }
            ],
            NextDataPriorities =
            [
                "ABARES 수산·양식 통계 XLSX를 원본 해시·회계연도·어종·단위와 함께 적재하는 연간 수집기 구현",
                "ABARES 주간 농축산·원예 가격의 민간 원자료 이용조건과 안정적인 기계 판독 계약 확인",
                "미국 농어업경영체 공개 원천 중 CSV·API 제공 명부를 개인정보 최소화 규칙과 함께 순차 연동",
                "미국 NOAA 수산물 양륙·생산 자료의 안정적인 공식 제공 방식과 NASS 품목 코드 연결 검증",
                "축산물 등급·도매 유통가격과 aT 가격의 역할 구분",
                "소비자 체감가격·온라인 가격의 조사 기준과 수집 허용 범위 정리",
                "지역·시장·품질·등급·포장단위별 시계열 품질지표 축적"
            ],
            BrokerageReadinessRequirements =
            [
                "데이터 누락률·갱신 지연·품목 매칭 정확도를 기간별로 측정할 것",
                "화주·기사·주선사·시장 운영자 인터뷰로 실제 업무와 책임 경계를 확인할 것",
                "화물자동차 운수사업 관련 등록·허가·약관·보험·정산 요건을 전문가와 검토할 것",
                "분쟁·사고·취소·과적·품질 훼손의 책임과 증빙 절차를 먼저 설계할 것",
                "정보 제공과 주선 거래를 별도 모듈·권한·감사기록으로 분리할 것"
            ]
        };
    }

    private static AgriculturalFisheriesDataSourceResponse AustraliaSource(
        호주농수산식품가격원천응답 source)
        => new()
        {
            Key = source.Key,
            Provider = source.Provider,
            DisplayName = source.DisplayName,
            Coverage = source.Coverage,
            UpdateCycle = source.UpdateCycle,
            StatusCode = source.IntegrationStatusCode,
            StatusLabel = source.IntegrationStatusCode switch
            {
                "IntegratedApi" => "자동 조회 가능",
                "DownloadAvailable" => "공식 파일 수집 준비",
                _ => "참고 원천 확인됨"
            },
            IsConfigured = source.AutomatedQueryAvailable,
            DocumentationUrl = source.DocumentationUrl,
            UsageNote = source.UsageNote
        };

    public AgriculturalFisheriesItemSearchResponse SearchItems(
        string? query,
        string? categoryCode,
        int page,
        int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var term = query?.Trim() ?? string.Empty;
        var normalizedTerm = NormalizeHsCode(term);
        var normalizedCategory = categoryCode?.Trim() ?? string.Empty;

        IEnumerable<FoodPriceCrosswalk> entries = _crosswalkCatalog.GetAll();
        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            entries = entries.Where(entry => string.Equals(
                entry.AtCategoryCode,
                normalizedCategory,
                StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            entries = entries.Where(entry =>
                entry.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entry.AtItemName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(normalizedTerm)
                    && entry.HsPrefix.Contains(normalizedTerm, StringComparison.Ordinal)));
        }

        var filtered = entries
            .OrderBy(entry => entry.AtCategoryCode, StringComparer.Ordinal)
            .ThenBy(entry => entry.ProductName, StringComparer.Ordinal)
            .ThenBy(entry => entry.HsPrefix, StringComparer.Ordinal)
            .ToArray();

        return new AgriculturalFisheriesItemSearchResponse
        {
            Items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapItem)
                .ToArray(),
            TotalCount = filtered.Length,
            Page = page,
            PageSize = pageSize
        };
    }

    public AgriculturalFisheriesItemResponse? FindItem(string? hsCode)
    {
        var crosswalk = _crosswalkCatalog.Find(hsCode);
        return crosswalk is null ? null : MapItem(crosswalk);
    }

    public async Task<AgriculturalFisheriesDomesticPriceResponse> GetDomesticPriceAsync(
        AgriculturalFisheriesDomesticPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var hsCode = NormalizeHsCode(request.HsCode);
        if (hsCode.Length < 4)
        {
            return Fail("InvalidRequest", hsCode, "HS 코드를 4자리 이상 입력해 주세요.");
        }

        if (!TryResolveReferenceDate(request.ReferenceDate, out var referenceDate))
        {
            return Fail("InvalidRequest", hsCode, "국내가격 기준일을 yyyyMMdd 형식으로 확인해 주세요.");
        }

        var crosswalk = _crosswalkCatalog.Find(hsCode);
        if (crosswalk is null)
        {
            return Fail(
                "MappingRequired",
                hsCode,
                "현재 정보 카탈로그에 연결되지 않은 HS 코드입니다.",
                "HS 분류와 국내 가격 조사 품목의 검토된 연결이 필요합니다.");
        }

        var item = MapItem(crosswalk);
        var lookbackDays = Math.Clamp(request.LookbackDays <= 0 ? 14 : request.LookbackDays, 1, 31);
        var startDate = referenceDate.AddDays(-(lookbackDays - 1));
        var lookupRequest = new AtDomesticFoodPriceRequest
        {
            CategoryCode = crosswalk.AtCategoryCode,
            ItemCode = crosswalk.AtItemCode,
            StartDate = startDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            EndDate = referenceDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            VarietyCodes = crosswalk.AtVarietyCodes,
            ExcludedNameTokens = crosswalk.ExcludedNameTokens
        };

        AtDomesticFoodPriceLookupResult price;
        try
        {
            price = await _domesticPriceLookupService.LookupAsync(lookupRequest, cancellationToken);
        }
        catch (Exception ex) when (
            !cancellationToken.IsCancellationRequested
            && ex is InvalidOperationException or HttpRequestException or TaskCanceledException)
        {
            price = new AtDomesticFoodPriceLookupResult
            {
                Success = false,
                ErrorMessage = ex is TaskCanceledException
                    ? "aT 국내가격 조회시간이 초과되었습니다."
                    : "aT 국내가격을 불러오지 못했습니다.",
                CategoryCode = lookupRequest.CategoryCode,
                ItemCode = lookupRequest.ItemCode,
                StartDate = lookupRequest.StartDate,
                EndDate = lookupRequest.EndDate
            };
        }

        var notices = BuildPriceNotices(crosswalk);
        return new AgriculturalFisheriesDomesticPriceResponse
        {
            Success = price.Success,
            StatusCode = price.Success ? "Complete" : "DataUnavailable",
            ErrorMessage = price.ErrorMessage,
            HsCode = hsCode,
            Item = item,
            Price = price,
            Summary = price.Success
                ? $"{item.ProductName}의 최근 국내 중도매·소매 조사 가격입니다."
                : $"{item.ProductName}의 국내가격 자료를 현재 확인하지 못했습니다.",
            Notices = notices
        };
    }

    private static AgriculturalFisheriesDataSourceResponse Source(
        string key,
        string provider,
        string displayName,
        string coverage,
        string updateCycle,
        bool isConfigured,
        string documentationUrl,
        string usageNote)
        => new()
        {
            Key = key,
            Provider = provider,
            DisplayName = displayName,
            Coverage = coverage,
            UpdateCycle = updateCycle,
            StatusCode = isConfigured ? "Ready" : "NeedsServiceKey",
            StatusLabel = isConfigured ? "조회 준비됨" : "공공데이터 인증키 필요",
            IsConfigured = isConfigured,
            DocumentationUrl = documentationUrl,
            UsageNote = usageNote
        };

    private static AgriculturalFisheriesItemResponse MapItem(FoodPriceCrosswalk crosswalk)
        => new()
        {
            HsPrefix = crosswalk.HsPrefix,
            ProductName = crosswalk.ProductName,
            CategoryCode = crosswalk.AtCategoryCode,
            CategoryLabel = CategoryLabels.GetValueOrDefault(crosswalk.AtCategoryCode, "기타"),
            AtItemCode = crosswalk.AtItemCode,
            AtItemName = crosswalk.AtItemName,
            AtVarietyCodes = crosswalk.AtVarietyCodes,
            MatchQualityCode = crosswalk.MatchQualityCode,
            MatchQualityLabel = crosswalk.MatchQualityLabel,
            DomesticOriginStatusCode = crosswalk.DomesticOriginStatusCode,
            DomesticOriginStatusLabel = crosswalk.DomesticOriginStatusLabel,
            Note = crosswalk.Note
        };

    private static IReadOnlyList<string> BuildPriceNotices(FoodPriceCrosswalk crosswalk)
    {
        var notices = new List<string>
        {
            "한국농수산식품유통공사(aT)의 조사 가격이며 주문·매입·운송 견적이 아닙니다.",
            "표시 단가는 kg 기준으로 정규화되어 실제 포장단위 가격과 다를 수 있습니다.",
            "품질·등급·산지·포장·신선도 차이를 확인한 뒤 의사결정에 참고해 주세요."
        };

        notices.Add(crosswalk.DomesticOriginStatusCode == "DomesticVariant"
            ? "aT 품종코드와 명칭을 기준으로 수입산 표본을 제외한 국산 품종 정보입니다."
            : "국내시장 조사값이며 모든 표본에 국산 원산지가 명시된 것은 아닙니다.");
        if (crosswalk.MatchQualityCode == "Representative")
        {
            notices.Add("HS 규격과 국내 조사 규격이 달라 대표 품목 가격을 사용합니다.");
        }

        return notices;
    }

    private static AgriculturalFisheriesDomesticPriceResponse Fail(
        string statusCode,
        string hsCode,
        string errorMessage,
        string? summary = null)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            HsCode = hsCode,
            Summary = summary ?? errorMessage,
            Notices = ["이 API는 정보 제공 전용이며 주문·계약·주선 업무를 실행하지 않습니다."]
        };

    private static bool TryResolveReferenceDate(string? value, out DateTime referenceDate)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            referenceDate = DateTimeOffset.UtcNow.ToOffset(KoreaOffset).Date;
            return true;
        }

        var digits = NormalizeHsCode(value);
        return DateTime.TryParseExact(
            digits,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out referenceDate);
    }

    private static string NormalizeHsCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);
}
