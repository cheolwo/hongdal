using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record Nongsaro공공데이터Item(
    IReadOnlyDictionary<string, string> Fields)
{
    public string Get(string fieldName)
        => Fields.TryGetValue(fieldName, out var value) ? value : string.Empty;
}

public sealed record Nongsaro공공데이터Response(
    string ServiceName,
    string OperationName,
    string ResultCode,
    string ResultMessage,
    DateTimeOffset RetrievedAtUtc,
    string SourceDocumentationUrl,
    IReadOnlyList<Nongsaro공공데이터Item> Items);

public interface INongsaroOpenApiClient
{
    Task<Nongsaro공공데이터Response> QueryAsync(
        string serviceName,
        string operationName,
        IReadOnlyDictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default);
}

public interface I농사로작목기술Module
{
    Task<Nongsaro공공데이터Response> 주분류조회Async(
        CancellationToken cancellationToken = default);
}

public interface I농사로농작업일정Module
{
    Task<Nongsaro공공데이터Response> 작업군조회Async(
        CancellationToken cancellationToken = default);

    Task<Nongsaro공공데이터Response> 일정조회Async(
        string 품목구분Code,
        CancellationToken cancellationToken = default);

    Task<Nongsaro공공데이터Response> 시기정보조회Async(
        string 콘텐츠번호,
        CancellationToken cancellationToken = default);

    Task<Nongsaro공공데이터Response> 상세조회Async(
        string 콘텐츠번호,
        CancellationToken cancellationToken = default);
}

public interface I농사로농작물재해예방Module
{
    Task<Nongsaro공공데이터Response> 연도조회Async(
        CancellationToken cancellationToken = default);
}

public interface I농사로품종정보Module
{
    Task<Nongsaro공공데이터Response> 기관조회Async(
        CancellationToken cancellationToken = default);
}

public interface I농사로지역문화Module
{
    Task<Nongsaro공공데이터Response> 지역특산물시도조회Async(
        CancellationToken cancellationToken = default);

    Task<Nongsaro공공데이터Response> 이달음식연도조회Async(
        CancellationToken cancellationToken = default);
}

public interface I농사로표준사료Module
{
    Task<Nongsaro공공데이터Response> 사료목록조회Async(
        CancellationToken cancellationToken = default);
}

public sealed record Nongsaro공공데이터ModuleDescriptor(
    string StableKey,
    string DisplayName,
    string ServiceName,
    string EntryOperationName,
    int Priority,
    string WorldUse,
    string Boundary,
    bool Executable);

public static class Nongsaro공공데이터Catalog
{
    public const string DocumentationUrl =
        "https://www.nongsaro.go.kr/portal/ps/psn/psnj/openApiLst.ps?menuId=PS65428";

    public const string 작목기술Service = "cropEbook";
    public const string 작목기술주분류Operation = "mainCategoryList";

    public const string 농작업일정Service = "farmWorkingPlanNew";
    public const string 농작업일정작업군Operation = "workScheduleGrpList";
    public const string 농작업일정목록Operation = "workScheduleLst";
    public const string 농작업일정시기Operation = "workScheduleEraInfoLst";
    public const string 농작업일정상세Operation = "workScheduleDtl";

    public const string 농작물재해예방Service = "frcDsstrPrevnt";
    public const string 농작물재해예방연도Operation = "frcDsstrPrevntYear";

    public const string 품종정보Service = "varietyInfo";
    public const string 품종정보기관Operation = "insttList";

    public const string 지역특산물Service = "localSpcprd";
    public const string 지역특산물시도Operation = "selectAreaSidoLst";

    public const string 이달음식Service = "monthFd";
    public const string 이달음식연도Operation = "monthFdYearLst";

    public const string 표준사료Service = "feedSearch";
    public const string 표준사료목록Operation = "feedSearchList";

    public static IReadOnlyList<Nongsaro공공데이터ModuleDescriptor> Modules { get; } =
    [
        Descriptor("crop-tech", "작목별 농업기술정보", 작목기술Service, 작목기술주분류Operation, 0, "작물 정의와 농업기술 기준", true),
        Descriptor("work-schedule", "농작업일정정보", 농작업일정Service, 농작업일정작업군Operation, 0, "작물별 월드 작업 시기", true),
        Descriptor("crop-disaster", "농작물재해예방정보", 농작물재해예방Service, 농작물재해예방연도Operation, 0, "재해예방 정보와 월드 경고 후보", true),
        Descriptor("variety", "품종정보", 품종정보Service, 품종정보기관Operation, 0, "품종 식별과 연구기관 근거", true),
        Descriptor("local-specialty", "지역특산물", 지역특산물Service, 지역특산물시도Operation, 0, "지역 지도 콘텐츠 후보", true),
        Descriptor("monthly-food", "이달의 음식정보", 이달음식Service, 이달음식연도Operation, 0, "계절 음식과 지역문화 콘텐츠", true),
        Descriptor("standard-feed", "표준사료", 표준사료Service, 표준사료목록Operation, 0, "축산 사료 기준 정보", true),
        Descriptor("common-code", "농사로 공통코드", "commonCode", "commonTopCodeLst", 1, "여러 농사로 모듈의 코드 해석", false),
        Descriptor("agriculture-dictionary", "농업용어사전", "farmDic", "searchEqualWord", 1, "사용자 설명과 용어 도움말", false),
        Descriptor("pesticide-price", "농약판매가격", "pesticideSalePrice", "yearGubunList", 1, "가격 관측과 비용 참고", false),
        Descriptor("agriculture-curation", "농업기술 더하기 나누기", "mlrdCuration", "areaGubunList", 1, "지역 농업기술 큐레이션", false),
        Descriptor("herb", "약초정보", "prvateTherpy", "prvateTherpyList", 1, "약초 지식 콘텐츠", false),
        Descriptor("livestock-tech", "축산실용기술모음", "stkrsPractialTech", "stkrsPractialTechList", 1, "축산 기술 콘텐츠", false),
        Descriptor("livestock-dictionary", "축산용어사전", "stockbreedingDic", "stockbreedingDicList", 1, "축산 용어 도움말", false),
        Descriptor("traditional-liquor", "전통주 제조법", "trditAchlqrMnfcturLaw", "trditAchlqrMnfcturLawList", 2, "전통 식문화 콘텐츠", false),
        Descriptor("regional-brand", "지역브랜드", "areaBrand", "selectSclCodeLst", 2, "지역 브랜드 지도 콘텐츠 후보", false),
        Descriptor("legacy-item-manual", "품목별관리매뉴얼", "farmWorkingPlan", "workScheduleGrpList", 2, "구형 농작업일정 호환 참고", false)
    ];

    private static readonly HashSet<string> ApprovedOperations =
    [
        Key(작목기술Service, 작목기술주분류Operation),
        Key(농작업일정Service, 농작업일정작업군Operation),
        Key(농작업일정Service, 농작업일정목록Operation),
        Key(농작업일정Service, 농작업일정시기Operation),
        Key(농작업일정Service, 농작업일정상세Operation),
        Key(농작물재해예방Service, 농작물재해예방연도Operation),
        Key(품종정보Service, 품종정보기관Operation),
        Key(지역특산물Service, 지역특산물시도Operation),
        Key(이달음식Service, 이달음식연도Operation),
        Key(표준사료Service, 표준사료목록Operation)
    ];

    public static bool IsApproved(string serviceName, string operationName)
        => ApprovedOperations.Contains(Key(serviceName, operationName));

    private static string Key(string serviceName, string operationName)
        => $"{serviceName}/{operationName}";

    private static Nongsaro공공데이터ModuleDescriptor Descriptor(
        string stableKey,
        string displayName,
        string serviceName,
        string entryOperationName,
        int priority,
        string worldUse,
        bool executable)
        => new(
            stableKey,
            displayName,
            serviceName,
            entryOperationName,
            priority,
            worldUse,
            "공개 정보 근거이며 판매 가능성, 재고, 계약, 개인 위치를 의미하지 않음",
            executable);
}

public sealed class NongsaroOpenApiClient : INongsaroOpenApiClient
{
    private readonly HttpClient _httpClient;
    private readonly NongsaroOpenApiOptions _options;
    private readonly TimeProvider _timeProvider;

    public NongsaroOpenApiClient(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options,
        TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _options = options.Value.Nongsaro;
        _timeProvider = timeProvider;
    }

    public async Task<Nongsaro공공데이터Response> QueryAsync(
        string serviceName,
        string operationName,
        IReadOnlyDictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "PublicData:Nongsaro:ApiKey가 필요합니다. 키는 .NET User Secrets 또는 운영 비밀 저장소에만 설정하세요.");
        }

        if (!Nongsaro공공데이터Catalog.IsApproved(serviceName, operationName))
        {
            throw new ArgumentException(
                $"승인된 농사로 operation이 아닙니다: {serviceName}/{operationName}",
                nameof(operationName));
        }

        var path = BuildPath(serviceName, operationName, parameters);
        using var response = await SendAsync(
            serviceName,
            operationName,
            path,
            cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await LoadXmlAsync(stream, cancellationToken);
        return Parse(document, serviceName, operationName, _timeProvider.GetUtcNow());
    }

    internal static Nongsaro공공데이터Response Parse(
        XDocument document,
        string serviceName,
        string operationName,
        DateTimeOffset retrievedAtUtc)
    {
        var resultCode = Read(document.Root, "resultCode");
        var resultMessage = Read(document.Root, "resultMsg");
        if (!string.IsNullOrWhiteSpace(resultCode) && resultCode is not "00" and not "0")
        {
            throw new InvalidOperationException(
                $"농사로 API 응답 오류입니다. Service={serviceName}, Operation={operationName}, Code={resultCode}, Message={resultMessage}");
        }

        var items = Descendants(document.Root, "item")
            .Select(item => new Nongsaro공공데이터Item(
                item.Elements()
                    .GroupBy(element => element.Name.LocalName, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => string.Join(" | ", group.Select(element => element.Value.Trim())),
                        StringComparer.Ordinal)))
            .ToArray();

        return new Nongsaro공공데이터Response(
            serviceName,
            operationName,
            resultCode,
            resultMessage,
            retrievedAtUtc,
            Nongsaro공공데이터Catalog.DocumentationUrl,
            items);
    }

    private string BuildPath(
        string serviceName,
        string operationName,
        IReadOnlyDictionary<string, string>? parameters)
    {
        var query = new List<KeyValuePair<string, string>>
        {
            new("apiKey", _options.ApiKey.Trim())
        };
        if (parameters is not null)
        {
            query.AddRange(parameters.Where(item => !string.IsNullOrWhiteSpace(item.Value)));
        }

        return $"/service/{Uri.EscapeDataString(serviceName)}/{Uri.EscapeDataString(operationName)}?"
               + string.Join(
                   '&',
                   query.Select(item =>
                       $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));
    }

    private async Task<HttpResponseMessage> SendAsync(
        string serviceName,
        string operationName,
        string path,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await _httpClient.GetAsync(
                path,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (attempt >= 3 || !IsTransient(response.StatusCode))
            {
                var statusCode = (int)response.StatusCode;
                response.Dispose();
                throw new InvalidOperationException(
                    $"농사로 API 호출에 실패했습니다. Service={serviceName}, Operation={operationName}, Status={statusCode}");
            }

            response.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
        }
    }

    private static async Task<XDocument> LoadXmlAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        using var reader = XmlReader.Create(stream, settings);
        return await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
    }

    private static IEnumerable<XElement> Descendants(XContainer? container, string name)
        => container?.Descendants().Where(element => element.Name.LocalName == name)
           ?? [];

    private static string Read(XContainer? container, string name)
        => container?.Descendants()
               .FirstOrDefault(element => element.Name.LocalName == name)
               ?.Value
               .Trim()
           ?? string.Empty;

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;
}

public sealed class 농사로작목기술Module(INongsaroOpenApiClient client)
    : I농사로작목기술Module
{
    public Task<Nongsaro공공데이터Response> 주분류조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.작목기술Service,
            Nongsaro공공데이터Catalog.작목기술주분류Operation,
            cancellationToken: cancellationToken);
}

public sealed class 농사로농작업일정Module(INongsaroOpenApiClient client)
    : I농사로농작업일정Module
{
    public Task<Nongsaro공공데이터Response> 작업군조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.농작업일정Service,
            Nongsaro공공데이터Catalog.농작업일정작업군Operation,
            cancellationToken: cancellationToken);

    public Task<Nongsaro공공데이터Response> 일정조회Async(
        string 품목구분Code,
        CancellationToken cancellationToken = default)
        => QueryByRequiredValue(
            Nongsaro공공데이터Catalog.농작업일정목록Operation,
            "kidofcomdtySeCode",
            품목구분Code,
            cancellationToken);

    public Task<Nongsaro공공데이터Response> 시기정보조회Async(
        string 콘텐츠번호,
        CancellationToken cancellationToken = default)
        => QueryByRequiredValue(
            Nongsaro공공데이터Catalog.농작업일정시기Operation,
            "cntntsNo",
            콘텐츠번호,
            cancellationToken);

    public Task<Nongsaro공공데이터Response> 상세조회Async(
        string 콘텐츠번호,
        CancellationToken cancellationToken = default)
        => QueryByRequiredValue(
            Nongsaro공공데이터Catalog.농작업일정상세Operation,
            "cntntsNo",
            콘텐츠번호,
            cancellationToken);

    private Task<Nongsaro공공데이터Response> QueryByRequiredValue(
        string operationName,
        string parameterName,
        string value,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return client.QueryAsync(
            Nongsaro공공데이터Catalog.농작업일정Service,
            operationName,
            new Dictionary<string, string>
            {
                [parameterName] = value.Trim()
            },
            cancellationToken);
    }
}

public sealed class 농사로농작물재해예방Module(INongsaroOpenApiClient client)
    : I농사로농작물재해예방Module
{
    public Task<Nongsaro공공데이터Response> 연도조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.농작물재해예방Service,
            Nongsaro공공데이터Catalog.농작물재해예방연도Operation,
            cancellationToken: cancellationToken);
}

public sealed class 농사로품종정보Module(INongsaroOpenApiClient client)
    : I농사로품종정보Module
{
    public Task<Nongsaro공공데이터Response> 기관조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.품종정보Service,
            Nongsaro공공데이터Catalog.품종정보기관Operation,
            cancellationToken: cancellationToken);
}

public sealed class 농사로지역문화Module(INongsaroOpenApiClient client)
    : I농사로지역문화Module
{
    public Task<Nongsaro공공데이터Response> 지역특산물시도조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.지역특산물Service,
            Nongsaro공공데이터Catalog.지역특산물시도Operation,
            cancellationToken: cancellationToken);

    public Task<Nongsaro공공데이터Response> 이달음식연도조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.이달음식Service,
            Nongsaro공공데이터Catalog.이달음식연도Operation,
            cancellationToken: cancellationToken);
}

public sealed class 농사로표준사료Module(INongsaroOpenApiClient client)
    : I농사로표준사료Module
{
    public Task<Nongsaro공공데이터Response> 사료목록조회Async(
        CancellationToken cancellationToken = default)
        => client.QueryAsync(
            Nongsaro공공데이터Catalog.표준사료Service,
            Nongsaro공공데이터Catalog.표준사료목록Operation,
            cancellationToken: cancellationToken);
}
