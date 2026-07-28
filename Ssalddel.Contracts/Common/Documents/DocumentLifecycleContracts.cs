namespace Ssalddel.Contracts.Common.Documents;

public static class 문서StableId종류코드
{
    public const string 커뮤니티원장 = "CommunityLedger";
    public const string 주문참조 = "OrderReference";
    public const string 입고요청 = "WarehouseInboundRequest";
    public const string 입고상품 = "WarehouseInventory";
    public const string 출고예정 = "WarehouseOutboundPlan";
    public const string 운송의뢰 = "TransportRequest";
    public const string 운송실행 = "TransportExecution";
    public const string 문서초안 = "DocumentDraft";

    public static string 표시명(string? code)
        => code switch
        {
            커뮤니티원장 => "공동 원장",
            주문참조 => "주문",
            입고요청 => "입고 요청",
            입고상품 => "입고 상품",
            출고예정 => "출고 예정",
            운송의뢰 => "운송 의뢰",
            운송실행 => "운송 실행",
            문서초안 => "문서 초안",
            _ => code?.Trim() ?? string.Empty
        };
}

public static class 문서StableId
{
    public static string 만들기(string 종류코드, string 값)
    {
        var normalizedKind = 종류코드?.Trim() ?? string.Empty;
        var normalizedValue = 값?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedKind)
            || normalizedKind.Contains(':', StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ArgumentException("stable ID에는 콜론이 없는 종류코드와 값이 필요합니다.");
        }

        return $"{normalizedKind}:{normalizedValue}";
    }

    public static string 만들기(string 종류코드, long 값)
        => 만들기(종류코드, 값.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static bool 분석(string? stableId, out string 종류코드, out string 값)
    {
        종류코드 = string.Empty;
        값 = string.Empty;
        if (string.IsNullOrWhiteSpace(stableId))
        {
            return false;
        }

        var normalized = stableId.Trim();
        var separator = normalized.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0 || separator >= normalized.Length - 1)
        {
            return false;
        }

        종류코드 = normalized[..separator].Trim();
        값 = normalized[(separator + 1)..].Trim();
        return !string.IsNullOrWhiteSpace(종류코드)
               && !string.IsNullOrWhiteSpace(값);
    }

    public static string 표시명(string? stableId)
        => 분석(stableId, out var kind, out var value)
            ? $"{문서StableId종류코드.표시명(kind)} · {value}"
            : stableId?.Trim() ?? string.Empty;

    public static int 흐름순서(string? stableId)
        => 분석(stableId, out var kind, out _)
            ? kind switch
            {
                문서StableId종류코드.커뮤니티원장 => 0,
                문서StableId종류코드.주문참조 => 1,
                문서StableId종류코드.입고요청 => 2,
                문서StableId종류코드.입고상품 => 3,
                문서StableId종류코드.출고예정 => 4,
                문서StableId종류코드.운송의뢰 => 5,
                문서StableId종류코드.운송실행 => 6,
                문서StableId종류코드.문서초안 => 7,
                _ => 99
            }
            : 100;
}

public sealed class 문서관계그래프노드응답
{
    public long 문서Id { get; set; }
    public string 문서코드 { get; set; } = string.Empty;
    public string 문서명 { get; set; } = string.Empty;
    public string 문서분류코드 { get; set; } = string.Empty;
    public string 생명주기상태코드 { get; set; } = string.Empty;
    public string 원천원장Id { get; set; } = string.Empty;
    public string 원천원장종류코드 { get; set; } = string.Empty;
    public long? 원천원장Revision { get; set; }
    public string 내용Sha256 { get; set; } = string.Empty;
    public DateTime 생성일시 { get; set; }
    public IReadOnlyList<string> 연결StableId목록 { get; set; } = [];
}

public sealed class 문서관계그래프응답
{
    public string 기준StableId { get; set; } = string.Empty;
    public IReadOnlyList<string> 발견StableId목록 { get; set; } = [];
    public IReadOnlyList<문서관계그래프노드응답> 문서목록 { get; set; } = [];
}

public static class 문서분류코드
{
    public const string 업무작업지 = "OperationalWorksheet";
    public const string 당사자합의 = "PartyAgreement";
    public const string 거래명세 = "TransactionStatement";
    public const string 수행증빙 = "PerformanceEvidence";
    public const string 신고준비 = "FilingPreparation";
    public const string 외부발급참조 = "ExternalIssuedReference";
    public const string 거버넌스기록 = "GovernanceRecord";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(
        [
            업무작업지,
            당사자합의,
            거래명세,
            수행증빙,
            신고준비,
            외부발급참조,
            거버넌스기록
        ],
        StringComparer.Ordinal);

    public static string 표시명(string? code)
        => code switch
        {
            업무작업지 => "업무 작업지",
            당사자합의 => "당사자 합의",
            거래명세 => "거래 명세",
            수행증빙 => "수행 증빙",
            신고준비 => "신고 준비",
            외부발급참조 => "외부 발급 참조",
            거버넌스기록 => "거버넌스 기록",
            _ => "미분류"
        };
}

public static class 문서생명주기상태코드
{
    public const string 초안 = "Draft";
    public const string 입력필요 = "NeedsInput";
    public const string 검토준비 = "ReadyForReview";
    public const string 확인완료 = "Confirmed";
    public const string 서명완료 = "Signed";
    public const string 발행완료 = "Issued";
    public const string 외부원본등록 = "ExternalOriginalRegistered";
    public const string 전달완료 = "Delivered";
    public const string 수령확인 = "Acknowledged";
    public const string 보관 = "Archived";
    public const string 대체됨 = "Superseded";
    public const string 취소 = "Cancelled";
    public const string 폐기 = "Disposed";

    public static IReadOnlySet<string> 지원목록 { get; } = new HashSet<string>(
        [
            초안,
            입력필요,
            검토준비,
            확인완료,
            서명완료,
            발행완료,
            외부원본등록,
            전달완료,
            수령확인,
            보관,
            대체됨,
            취소,
            폐기
        ],
        StringComparer.Ordinal);

    public static string 표시명(string? code)
        => code switch
        {
            초안 => "초안",
            입력필요 => "입력 필요",
            검토준비 => "검토 준비",
            확인완료 => "확인 완료",
            서명완료 => "서명 완료",
            발행완료 => "발행 완료",
            외부원본등록 => "외부 원본 등록",
            전달완료 => "전달 완료",
            수령확인 => "수령 확인",
            보관 => "보관",
            대체됨 => "대체됨",
            취소 => "취소",
            폐기 => "폐기",
            _ => "상태 미지정"
        };
}

public static class 문서생성모드코드
{
    public const string 수동업로드 = "ManualUpload";
    public const string 원장초안 = "LedgerDraft";
    public const string 업무이벤트자동생성 = "BusinessEventGenerated";
    public const string 외부발급원본등록 = "ExternalOriginalRegistration";
}

public static class 문서발급주체코드
{
    public const string 플랫폼 = "Platform";
    public const string 플랫폼운영자 = "PlatformOperator";
    public const string 업무담당자 = "BusinessActor";
}

public static class 문서생명주기Planner
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> 허용전이 =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal)
        {
            [문서생명주기상태코드.초안] = 상태목록(
                문서생명주기상태코드.입력필요,
                문서생명주기상태코드.검토준비,
                문서생명주기상태코드.취소),
            [문서생명주기상태코드.입력필요] = 상태목록(
                문서생명주기상태코드.초안,
                문서생명주기상태코드.검토준비,
                문서생명주기상태코드.취소),
            [문서생명주기상태코드.검토준비] = 상태목록(
                문서생명주기상태코드.초안,
                문서생명주기상태코드.확인완료,
                문서생명주기상태코드.서명완료,
                문서생명주기상태코드.발행완료,
                문서생명주기상태코드.외부원본등록,
                문서생명주기상태코드.취소),
            [문서생명주기상태코드.확인완료] = 상태목록(
                문서생명주기상태코드.서명완료,
                문서생명주기상태코드.발행완료,
                문서생명주기상태코드.전달완료,
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨,
                문서생명주기상태코드.취소),
            [문서생명주기상태코드.서명완료] = 상태목록(
                문서생명주기상태코드.발행완료,
                문서생명주기상태코드.전달완료,
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨),
            [문서생명주기상태코드.발행완료] = 상태목록(
                문서생명주기상태코드.전달완료,
                문서생명주기상태코드.수령확인,
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨),
            [문서생명주기상태코드.외부원본등록] = 상태목록(
                문서생명주기상태코드.전달완료,
                문서생명주기상태코드.수령확인,
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨),
            [문서생명주기상태코드.전달완료] = 상태목록(
                문서생명주기상태코드.수령확인,
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨),
            [문서생명주기상태코드.수령확인] = 상태목록(
                문서생명주기상태코드.보관,
                문서생명주기상태코드.대체됨),
            [문서생명주기상태코드.보관] = 상태목록(
                문서생명주기상태코드.대체됨,
                문서생명주기상태코드.폐기),
            [문서생명주기상태코드.취소] = 상태목록(
                문서생명주기상태코드.보관,
                문서생명주기상태코드.폐기),
            [문서생명주기상태코드.대체됨] = 상태목록(),
            [문서생명주기상태코드.폐기] = 상태목록()
        };

    public static bool 전이가능한가(string? 현재상태코드, string? 대상상태코드)
        => !string.IsNullOrWhiteSpace(현재상태코드)
           && !string.IsNullOrWhiteSpace(대상상태코드)
           && 허용전이.TryGetValue(현재상태코드, out var targets)
           && targets.Contains(대상상태코드);

    public static IReadOnlyList<string> 다음상태목록(string? 현재상태코드)
        => !string.IsNullOrWhiteSpace(현재상태코드)
           && 허용전이.TryGetValue(현재상태코드, out var targets)
            ? targets.OrderBy(state => state, StringComparer.Ordinal).ToArray()
            : [];

    public static bool 불변스냅샷인가(string? 상태코드)
        => 상태코드 is 문서생명주기상태코드.확인완료
            or 문서생명주기상태코드.서명완료
            or 문서생명주기상태코드.발행완료
            or 문서생명주기상태코드.외부원본등록
            or 문서생명주기상태코드.전달완료
            or 문서생명주기상태코드.수령확인
            or 문서생명주기상태코드.보관
            or 문서생명주기상태코드.대체됨
            or 문서생명주기상태코드.폐기;

    private static IReadOnlySet<string> 상태목록(params string[] states)
        => new HashSet<string>(states, StringComparer.Ordinal);
}

public static class 문서분류Resolver
{
    public static string Resolve(string? 문서코드, string? 원천문서종류코드 = null)
    {
        var sourceKind = 원천문서종류코드?.Trim();
        if (!string.IsNullOrWhiteSpace(sourceKind))
        {
            var sourceClassification = sourceKind switch
            {
                원장관행문서종류코드.계약검토자료서 => 문서분류코드.당사자합의,
                원장관행문서종류코드.수입통관서류점검표
                    or 원장관행문서종류코드.수입식품서류점검표
                    or 원장관행문서종류코드.원산지증명준비자료서
                    or 원장관행문서종류코드.선적문서참조표
                    or 원장관행문서종류코드.선적인도지시서 => 문서분류코드.신고준비,
                원장관행문서종류코드.같이주문집계표 => 문서분류코드.업무작업지,
                원장관행문서종류코드.견적요청서
                    or 원장관행문서종류코드.구매주문서
                    or 원장관행문서종류코드.프로포마송장자료서
                    or 원장관행문서종류코드.상업송장
                    or 원장관행문서종류코드.포장명세서 => 문서분류코드.거래명세,
                _ => null
            };
            if (sourceClassification is not null)
            {
                return sourceClassification;
            }
        }

        return 문서코드?.Trim() switch
        {
            "인수증" or "상차인수확인서" or "운송확인서" => 문서분류코드.수행증빙,
            "출고인계확인서" => 문서분류코드.수행증빙,
            "정산내역서" or "세금계산서연결정보" or "결제영수증" or "환불확인서" => 문서분류코드.거래명세,
            "배차확정서" or "출고예정목록" => 문서분류코드.업무작업지,
            "사고분쟁기록" => 문서분류코드.거버넌스기록,
            _ => 문서분류코드.업무작업지
        };
    }
}
