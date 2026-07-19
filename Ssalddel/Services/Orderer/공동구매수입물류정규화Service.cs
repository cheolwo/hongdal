using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Services.Orderer;

public interface I공동구매수입물류정규화Service
{
    IReadOnlyList<수입물류참조항목> SearchReferences(수입물류참조조회요청 request);

    수입물류정규화시뮬레이션결과 Simulate(
        수입물류정규화시뮬레이션요청 request);
}

public sealed class 공동구매수입물류정규화Service : I공동구매수입물류정규화Service
{
    private static readonly IReadOnlyList<수입물류참조항목> ReferenceCatalog =
    [
        CreatePort("KRPUS", "Busan Port", "Busan", "Busan Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRINC", "Incheon Port", "Incheon", "Incheon Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRPTK", "Pyeongtaek-Dangjin Port", "Gyeonggi/Chungnam", "Pyeongtaek Customs", "Korea Customs Service / MOF public data"),
        CreatePort("KRKUV", "Gunsan Port", "Jeonbuk", "Gunsan Customs", "Korea Customs Service / MOF public data"),
        CreateAirport("ICN", "Incheon International Airport", "Incheon", "Incheon Airport Customs", "Korea Customs Service / MOLIT public data"),
        CreateAirport("GMP", "Gimpo International Airport", "Seoul", "Gimpo Airport Customs", "Korea Customs Service / MOLIT public data"),
        CreateCustomsOffice("INCHEON", "Incheon Customs", "Incheon", "KRINC"),
        CreateCustomsOffice("PYEONGTAEK", "Pyeongtaek Customs", "Gyeonggi", "KRPTK"),
        CreateCustomsOffice("BUSAN", "Busan Customs", "Busan", "KRPUS"),
        CreateCustomsOffice("INCHEON_AIRPORT", "Incheon Airport Customs", "Incheon", "ICN")
    ];

    private static readonly IReadOnlyList<수입물류출처Dto> 출처목록 =
    [
        new()
        {
            Name = "Korea Customs Service Cargo Clearance Progress OpenAPI",
            Url = "https://www.data.go.kr/data/15126268/openapi.do",
            용도 = "B/L, H B/L, or cargo management number based cargo status and bonded location lookup"
        },
        new()
        {
            Name = "Korea Customs Service port/airport trade statistics",
            Url = "https://www.data.go.kr/data/15101636/openapi.do",
            용도 = "Port and airport import/export flow statistics"
        },
        new()
        {
            Name = "Ministry of Oceans and Fisheries port facility public data",
            Url = "https://www.data.go.kr/data/3082243/openapi.do",
            용도 = "Port facility and handling capacity reference"
        },
        new()
        {
            Name = "UNIPASS bonded area and registered business references",
            Url = "https://unipass.customs.go.kr/per/index.do",
            용도 = "Official bonded area code, bonded warehouse, and customs logistics reference verification"
        }
    ];

    public IReadOnlyList<수입물류참조항목> SearchReferences(수입물류참조조회요청 request)
    {
        var query = ReferenceCatalog.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.운송수단))
        {
            var transportMode = Normalize(request.운송수단);
            query = query.Where(x => string.IsNullOrWhiteSpace(x.운송수단)
                                     || Normalize(x.운송수단) == transportMode);
        }

        if (!string.IsNullOrWhiteSpace(request.코드유형))
        {
            var codeType = Normalize(request.코드유형);
            query = query.Where(x => Normalize(x.코드유형) == codeType);
        }

        if (!string.IsNullOrWhiteSpace(request.검색어))
        {
            var keyword = Normalize(request.검색어);
            query = query.Where(x =>
                Normalize(x.Code).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.Name).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.지역명).Contains(keyword, StringComparison.Ordinal)
                || Normalize(x.관련세관코드).Contains(keyword, StringComparison.Ordinal));
        }

        var pageSize = request.페이지크기 <= 0 ? 20 : Math.Min(request.페이지크기, 100);
        return query
            .OrderBy(x => x.코드유형)
            .ThenBy(x => x.Code)
            .Take(pageSize)
            .ToArray();
    }

    public 수입물류정규화시뮬레이션결과 Simulate(
        수입물류정규화시뮬레이션요청 request)
    {
        var warnings = new List<string>();
        var references = ResolveReferences(request, warnings);
        var flow = BuildFlow(request, references);
        var simulation = BuildSimulation(request, references, warnings);

        return new 수입물류정규화시뮬레이션결과
        {
            Success = warnings.Count == 0 || references.Count > 0,
            문서관리번호 = request.문서관리번호.Trim(),
            정규화참조목록 = references,
            제안흐름목록 = flow,
            Simulation = simulation,
            경고목록 = warnings,
            출처목록 = 출처목록
        };
    }

    private static List<수입물류참조항목> ResolveReferences(
        수입물류정규화시뮬레이션요청 request,
        List<string> warnings)
    {
        var references = new List<수입물류참조항목>();
        var destinationCode = Normalize(request.도착항코드);
        var destinationName = Normalize(request.도착항만공항명);
        var location = Normalize(request.현재위치요약);

        var destination = ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(destinationCode) && Normalize(x.Code) == destinationCode);
        destination ??= ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(destinationName) && Normalize(x.Name).Contains(destinationName, StringComparison.Ordinal));
        destination ??= ReferenceCatalog.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(location) && location.Contains(Normalize(x.지역명), StringComparison.Ordinal));

        if (destination is not null)
        {
            references.Add(destination);
            var relatedCustoms = ReferenceCatalog.FirstOrDefault(x =>
                x.코드유형 == 수입물류참조코드유형.세관
                && Normalize(x.관련항만공항코드) == Normalize(destination.Code));
            if (relatedCustoms is not null)
            {
                references.Add(relatedCustoms);
            }
        }
        else
        {
            warnings.Add("Destination port or airport could not be normalized. Keep the raw B/L response and verify the official code.");
        }

        if (!string.IsNullOrWhiteSpace(request.세관코드) || !string.IsNullOrWhiteSpace(request.세관명))
        {
            var customs = ReferenceCatalog.FirstOrDefault(x =>
                x.코드유형 == 수입물류참조코드유형.세관
                && (Normalize(x.Code) == Normalize(request.세관코드)
                    || Normalize(x.Name).Contains(Normalize(request.세관명), StringComparison.Ordinal)));
            if (customs is not null && references.All(x => x.Code != customs.Code))
            {
                references.Add(customs);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.보세구역코드) || !string.IsNullOrWhiteSpace(request.보세구역명))
        {
            references.Add(new 수입물류참조항목
            {
                Code = request.보세구역코드.Trim(),
                코드유형 = 수입물류참조코드유형.보세구역,
                Name = string.IsNullOrWhiteSpace(request.보세구역명) ? request.현재위치요약.Trim() : request.보세구역명.Trim(),
                지역명 = destination?.지역명 ?? string.Empty,
                운송수단 = request.운송수단.Trim(),
                관련항만공항코드 = destination?.Code ?? string.Empty,
                관련세관코드 = references.FirstOrDefault(x => x.코드유형 == 수입물류참조코드유형.세관)?.Code ?? string.Empty,
                출처명 = "UNIPASS cargo tracking response",
                출처Url = "https://www.data.go.kr/data/15126268/openapi.do",
                공식검증필요 = string.IsNullOrWhiteSpace(request.보세구역코드)
            });
        }
        else
        {
            warnings.Add("Bonded area code is not present. Cargo can be tracked by location name, but bonded warehouse settlement should wait for the official bonded area code.");
        }

        return references
            .GroupBy(x => $"{x.코드유형}:{Normalize(x.Code)}:{Normalize(x.Name)}")
            .Select(x => x.First())
            .ToList();
    }

    private static IReadOnlyList<수입물류흐름단계Dto> BuildFlow(
        수입물류정규화시뮬레이션요청 request,
        IReadOnlyList<수입물류참조항목> references)
    {
        var portOrAirport = references.FirstOrDefault(x =>
            x.코드유형 is 수입물류참조코드유형.항만 or 수입물류참조코드유형.공항);
        var customsOffice = references.FirstOrDefault(x => x.코드유형 == 수입물류참조코드유형.세관);
        var bondedArea = references.FirstOrDefault(x => x.코드유형 == 수입물류참조코드유형.보세구역);

        return
        [
            new()
            {
                순서 = 1,
                단계코드 = "TransportDocumentRegistered",
                표시명 = request.운송문서유형 == 공동구매선적문서유형코드.항공화물운송장
                    ? "AWB registered"
                    : "B/L registered",
                책임주체코드 = 공동구매물류워크플로우주체코드.해외판매자,
                참조코드 = request.운송문서번호.Trim(),
                참조명 = request.운송문서유형,
                공식코드확인됨 = !string.IsNullOrWhiteSpace(request.운송문서번호)
            },
            new()
            {
                순서 = 2,
                단계코드 = "ArrivalPortOrAirport",
                표시명 = request.운송수단 == 공동구매선적운송수단코드.항공
                    ? "Arrive at Korean airport"
                    : "Arrive at Korean port",
                책임주체코드 = 공동구매물류워크플로우주체코드.운송주체,
                참조코드 = portOrAirport?.Code ?? request.도착항코드.Trim(),
                참조명 = portOrAirport?.Name ?? request.도착항만공항명.Trim(),
                공식코드확인됨 = portOrAirport is not null
            },
            new()
            {
                순서 = 3,
                단계코드 = "CustomsJurisdiction",
                표시명 = "Customs jurisdiction resolved",
                책임주체코드 = 공동구매물류워크플로우주체코드.관세사,
                참조코드 = customsOffice?.Code ?? request.세관코드.Trim(),
                참조명 = customsOffice?.Name ?? request.세관명.Trim(),
                공식코드확인됨 = customsOffice is not null
            },
            new()
            {
                순서 = 4,
                단계코드 = "BondedAreaStorage",
                표시명 = "Bonded area storage or bonded transport",
                책임주체코드 = 공동구매물류워크플로우주체코드.관세사,
                참조코드 = bondedArea?.Code ?? request.보세구역코드.Trim(),
                참조명 = bondedArea?.Name ?? request.보세구역명.Trim(),
                공식코드확인됨 = bondedArea is not null && !bondedArea.공식검증필요
            },
            new()
            {
                순서 = 5,
                단계코드 = "DomesticLogisticsProxyInbound",
                표시명 = "Inbound to domestic logistics proxy",
                책임주체코드 = 공동구매물류워크플로우주체코드.국내물류대행,
                참조코드 = request.문서관리번호.Trim(),
                참조명 = "Group purchase import ledger",
                공식코드확인됨 = !string.IsNullOrWhiteSpace(request.문서관리번호)
            }
        ];
    }

    private static 수입물류비용위험시뮬레이션Dto BuildSimulation(
        수입물류정규화시뮬레이션요청 request,
        IReadOnlyList<수입물류참조항목> references,
        List<string> warnings)
    {
        var hasDestination = references.Any(x =>
            x.코드유형 is 수입물류참조코드유형.항만 or 수입물류참조코드유형.공항);
        var has보세구역코드 = references.Any(x =>
            x.코드유형 == 수입물류참조코드유형.보세구역 && !string.IsNullOrWhiteSpace(x.Code));
        var invoiceUnitValue = CalculateUnitValue(request.화물인보이스금액Usd, request.화물중량Kg);
        var domesticInboundCost = CalculateUnitValue(request.예상국내입고비용Krw, request.화물중량Kg);

        if (request.화물중량Kg is <= 0)
        {
            warnings.Add("Cargo weight is required to calculate unit value and domestic inbound cost per kg.");
        }

        var risk = hasDestination && has보세구역코드
            ? 수입물류시뮬레이션위험코드.낮음
            : hasDestination
                ? 수입물류시뮬레이션위험코드.중간
                : 수입물류시뮬레이션위험코드.검토필요;

        return new 수입물류비용위험시뮬레이션Dto
        {
            인보이스단가UsdPerKg = invoiceUnitValue,
            예상국내입고비용KrwPerKg = domesticInboundCost,
            통관경로위험코드 = risk,
            신뢰도코드 = hasDestination && has보세구역코드
                ? 수입물류시뮬레이션위험코드.낮음
                : 수입물류시뮬레이션위험코드.검토필요,
            요약 = hasDestination && has보세구역코드
                ? "Arrival point, customs jurisdiction, and bonded area are normalized enough for operational tracking."
                : "Keep the B/L response, then confirm official bonded area and customs codes before settlement or responsibility assignment."
        };
    }

    private static decimal? CalculateUnitValue(decimal? amount, decimal? weight)
        => amount.HasValue && weight is > 0
            ? decimal.Round(amount.Value / weight.Value, 4, MidpointRounding.AwayFromZero)
            : null;

    private static 수입물류참조항목 CreatePort(
        string code,
        string name,
        string regionName,
        string customsOfficeName,
        string sourceName)
        => new()
        {
            Code = code,
            코드유형 = 수입물류참조코드유형.항만,
            Name = name,
            지역명 = regionName,
            운송수단 = 공동구매선적운송수단코드.해상,
            관련세관코드 = customsOfficeName,
            출처명 = sourceName,
            출처Url = "https://www.data.go.kr/data/3082243/openapi.do"
        };

    private static 수입물류참조항목 CreateAirport(
        string code,
        string name,
        string regionName,
        string customsOfficeName,
        string sourceName)
        => new()
        {
            Code = code,
            코드유형 = 수입물류참조코드유형.공항,
            Name = name,
            지역명 = regionName,
            운송수단 = 공동구매선적운송수단코드.항공,
            관련세관코드 = customsOfficeName,
            출처명 = sourceName,
            출처Url = "https://www.data.go.kr/data/15101636/openapi.do"
        };

    private static 수입물류참조항목 CreateCustomsOffice(
        string code,
        string name,
        string regionName,
        string relatedPortOrAirportCode)
        => new()
        {
            Code = code,
            코드유형 = 수입물류참조코드유형.세관,
            Name = name,
            지역명 = regionName,
            관련항만공항코드 = relatedPortOrAirportCode,
            출처명 = "Korea Customs Service",
            출처Url = "https://www.customs.go.kr/"
        };

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
}
