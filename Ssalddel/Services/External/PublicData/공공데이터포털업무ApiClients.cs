using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public interface I수협유통공공데이터Client
{
    IReadOnlyList<공공데이터포털업무ApiDefinition> Apis { get; }

    Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default);
}

public interface I공동주택운영공공데이터Client
{
    IReadOnlyList<공공데이터포털업무ApiDefinition> Apis { get; }

    Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    공공데이터포털업무ApiFeature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "해양수산부 수협 유통 11개 공개 API를 하나의 인증 및 호출 경계로 제공",
    ContractType = typeof(공공데이터포털업무ApiResponse),
    FlowOrder = 3,
    Boundary = "재고와 입출고 원문은 공개 관측값이며 현재 가용성, 소유권 또는 거래 권한을 확정하지 않음")]
public sealed class 수협유통공공데이터Client : 공공데이터포털업무ApiClientBase, I수협유통공공데이터Client
{
    private static readonly IReadOnlyList<공공데이터포털업무ApiDefinition> Definitions =
    [
        Api("landing-place-status", "수협 산지조합 위판장 현황", "/1192000/select0060List/getselect0060List"),
        Api("landing-place", "수협 산지조합 위판장 정보", "/1192000/select0020List/getselect0020List"),
        Api("local-cooperative", "수협 산지조합 정보", "/1192000/select0010List/getselect0010List"),
        Api("distribution-inout", "수협 물류센터/공판장 품목별 입출고 현황", "/1192000/select0160List/getselect0160List"),
        Api("distribution-inventory", "수협 물류센터/공판장 품목별 재고 현황", "/1192000/select0170List/getselect0170List"),
        Api("warehouse-inout", "수협 산지조합 창고 품목별 입출고 현황", "/1192000/select0140List/getselect0140List"),
        Api("warehouse", "수협 산지조합 창고 정보", "/1192000/select0120List/getselect0120List"),
        Api("warehouse-inventory", "수협 산지조합 창고 품목별 재고 현황", "/1192000/select0150List/getselect0150List"),
        Api("warehouse-customer", "수협 산지조합 창고별 매출처 정보", "/1192000/select0130List/getselect0130List"),
        Api("landing-place-consignment-sale", "위판장별 위탁판매 현황", "/1192000/select0040List/getselect0040List"),
        Api("daily-consignment-sale", "일자별 위탁판매 현황", "/1192000/select0030List/getselect0030List")
    ];

    public 수협유통공공데이터Client(HttpClient httpClient, IOptions<PublicDataOptions> options)
        : base(httpClient, options, "해양수산부", Definitions)
    {
    }

    public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => Definitions;

    public Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default)
        => QueryCoreAsync(request, cancellationToken);

    private static 공공데이터포털업무ApiDefinition Api(string key, string displayName, string path)
        => new(key, displayName, path, [path]);
}

[SsalddelCodeMetadata(
    공공데이터포털업무ApiFeature.Key,
    SsalddelCodeLayer.ExternalAdapter,
    "국토교통부 공동주택 10개 공개 API를 기존 단지 및 관리비 client와 병행 가능한 호출 경계로 제공",
    ContractType = typeof(공공데이터포털업무ApiResponse),
    FlowOrder = 3,
    Boundary = "단지 단위 공개 정보만 조회하며 세대 및 거주자 식별, 계약 체결 또는 관리비 반영을 수행하지 않음")]
public sealed class 공동주택운영공공데이터Client : 공공데이터포털업무ApiClientBase, I공동주택운영공공데이터Client
{
    private static readonly IReadOnlyList<공공데이터포털업무ApiDefinition> Definitions =
    [
        Api("complex-list", "공동주택 단지 목록", "/1613000/AptListService3/getLegaldongAptList3", "/1613000/AptListService3/"),
        Api("maintenance-history", "공동주택 유지관리 이력", "/1613000/ApHusMntMngHistInfoOfferServiceV2/getBuldExtrlMntncHistInfoSearchV2", "/1613000/ApHusMntMngHistInfoOfferServiceV2/"),
        Api("bid-result", "공동주택 입찰결과공지", "/1613000/ApHusBidResultNoticeInfoOfferServiceV2/getHsmpCdSearchV2", "/1613000/ApHusBidResultNoticeInfoOfferServiceV2/"),
        Api("individual-management-cost", "공동주택관리비 개별사용료", "/1613000/AptIndvdlzManageCostServiceV2/getHsmpElectricityCostInfoV2", "/1613000/AptIndvdlzManageCostServiceV2/"),
        Api("private-contract-notice", "공동주택 수의계약 공지", "/1613000/ApHusPrvCntrNoticeInfoOfferServiceV2/getHsmpCdSearchV2", "/1613000/ApHusPrvCntrNoticeInfoOfferServiceV2/"),
        Api("public-management-cost", "공동주택관리비 공용관리비", "/1613000/AptCmnuseManageCostServiceV2/getHsmpLaborCostInfoV2", "/1613000/AptCmnuseManageCostServiceV2/"),
        Api("bid-notice", "공동주택 입찰공고", "/1613000/ApHusBidPblAncInfoOfferServiceV2/getHsmpCdSearchV2", "/1613000/ApHusBidPblAncInfoOfferServiceV2/"),
        Api("energy-use", "공동주택 에너지 사용", "/1613000/ApHusEnergyUseInfoOfferServiceV2/getHsmpApHusUsgQtyInfoSearchV2", "/1613000/ApHusEnergyUseInfoOfferServiceV2/"),
        Api("long-term-repair-reserve", "공동주택관리비 장기수선충당금", "/1613000/AptRepairsCostServiceV2/getHsmpMonthFeeInfoV2", "/1613000/AptRepairsCostServiceV2/"),
        Api("complex-basic", "공동주택 기본 정보", "/1613000/AptBasisInfoServiceV4/getAphusBassInfoV4", "/1613000/AptBasisInfoServiceV4/")
    ];

    public 공동주택운영공공데이터Client(HttpClient httpClient, IOptions<PublicDataOptions> options)
        : base(httpClient, options, "국토교통부", Definitions)
    {
    }

    public IReadOnlyList<공공데이터포털업무ApiDefinition> Apis => Definitions;

    public Task<공공데이터포털업무ApiResponse> QueryAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken = default)
        => QueryCoreAsync(request, cancellationToken);

    private static 공공데이터포털업무ApiDefinition Api(
        string key,
        string displayName,
        string defaultPath,
        string allowedPrefix)
        => new(key, displayName, defaultPath, [allowedPrefix]);
}

public abstract class 공공데이터포털업무ApiClientBase
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;
    private readonly string _provider;
    private readonly IReadOnlyDictionary<string, 공공데이터포털업무ApiDefinition> _definitions;

    protected 공공데이터포털업무ApiClientBase(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        string provider,
        IReadOnlyList<공공데이터포털업무ApiDefinition> definitions)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _provider = provider;
        _definitions = definitions.ToDictionary(item => item.Key, StringComparer.OrdinalIgnoreCase);
    }

    protected async Task<공공데이터포털업무ApiResponse> QueryCoreAsync(
        공공데이터포털업무ApiRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_definitions.TryGetValue(request.ApiKey, out var definition))
        {
            throw new ArgumentException($"허용되지 않은 공공데이터 API key입니다: {request.ApiKey}", nameof(request));
        }

        var operationPath = string.IsNullOrWhiteSpace(request.OperationPath)
            ? definition.DefaultOperationPath
            : request.OperationPath.Trim();
        if (!operationPath.StartsWith("/", StringComparison.Ordinal)
            || !definition.AllowedOperationPrefixes.Any(prefix =>
                operationPath.StartsWith(prefix, StringComparison.Ordinal)))
        {
            throw new ArgumentException("선택한 API에 허용되지 않은 operation path입니다.", nameof(request));
        }

        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new InvalidOperationException(
                "PublicData:DataGoKrServiceKey 또는 PublicData:ServiceKey 설정이 필요합니다.");
        }

        var parameters = request.Parameters
            .Where(item => !string.Equals(item.Key, "serviceKey", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(
                               item.Key,
                               definition.ServiceKeyParameterName,
                               StringComparison.OrdinalIgnoreCase))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        parameters[definition.ServiceKeyParameterName] = serviceKey;
        var relativeUrl = QueryHelpers.AddQueryString(operationPath.TrimStart('/'), parameters);

        using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return new 공공데이터포털업무ApiResponse
        {
            Success = response.IsSuccessStatusCode,
            ApiKey = definition.Key,
            Provider = _provider,
            OperationPath = operationPath,
            HttpStatusCode = (int)response.StatusCode,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            Body = body,
            ObservedAt = DateTimeOffset.UtcNow
        };
    }

    private string ResolveServiceKey()
        => !string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey)
            ? _options.DataGoKrServiceKey
            : _options.ServiceKey;
}
