namespace Ssalddel.Contracts.Common.TraditionalMarkets;

public sealed class 전통시장생활권협의체생성요청
{
    public string 시장Code { get; set; } = string.Empty;
    public string 협의체명 { get; set; } = string.Empty;
    public string 아파트단지명 { get; set; } = string.Empty;
    public string 아파트주소 { get; set; } = string.Empty;
    public string 상인회명 { get; set; } = string.Empty;
    public string 요청자역할 { get; set; } = string.Empty;
    public string 요청자대표명 { get; set; } = string.Empty;
    public string 상대대표UserId { get; set; } = string.Empty;
    public string 상대대표명 { get; set; } = string.Empty;
    public string 협의목적 { get; set; } = string.Empty;
}

public sealed class 전통시장생활권협의체참여수락요청
{
    public long? 예상Revision { get; set; }
}

public sealed class 전통시장교역안건생성요청
{
    public string 교역방향 { get; set; } = string.Empty;
    public string 품목명 { get; set; } = string.Empty;
    public string 품목설명 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 원산지국가 { get; set; } = string.Empty;
    public string 목적지국가 { get; set; } = string.Empty;
    public DateOnly? 희망시작일 { get; set; }
    public DateOnly? 희망종료일 { get; set; }
    public string 물류조건 { get; set; } = string.Empty;
    public decimal? 예상금액 { get; set; }
    public string 통화Code { get; set; } = "KRW";
    public bool 통관검토필요여부 { get; set; }
    public string 제안내용 { get; set; } = string.Empty;
}

public sealed class 전통시장교역안건결정요청
{
    public string 결정 { get; set; } = string.Empty;
    public string 의견 { get; set; } = string.Empty;
    public long? 예상Revision { get; set; }
}

public sealed class 전통시장생활권협의체목록응답
{
    public IReadOnlyList<전통시장생활권협의체요약응답> 항목 { get; set; } = [];
}

public class 전통시장생활권협의체요약응답
{
    public Guid 협의체Id { get; set; }
    public string 협의체명 { get; set; } = string.Empty;
    public string 시장Code { get; set; } = string.Empty;
    public string 시장명 { get; set; } = string.Empty;
    public string 아파트단지명 { get; set; } = string.Empty;
    public string 상인회명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 내역할 { get; set; } = string.Empty;
    public int 안건수 { get; set; }
    public int 합의안건수 { get; set; }
    public long Revision { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class 전통시장생활권협의체상세응답 : 전통시장생활권협의체요약응답
{
    public string 아파트주소 { get; set; } = string.Empty;
    public string 아파트대표UserId { get; set; } = string.Empty;
    public string 아파트대표명 { get; set; } = string.Empty;
    public DateTime? 아파트대표수락AtUtc { get; set; }
    public string 상인회대표UserId { get; set; } = string.Empty;
    public string 상인회대표명 { get; set; } = string.Empty;
    public DateTime? 상인회대표수락AtUtc { get; set; }
    public string 협의목적 { get; set; } = string.Empty;
    public string CommunityScopeKey { get; set; } = string.Empty;
    public string 협의체참조Key { get; set; } = string.Empty;
    public IReadOnlyList<전통시장교역안건응답> 안건 { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class 전통시장교역안건응답
{
    public Guid 안건Id { get; set; }
    public string 안건참조Key { get; set; } = string.Empty;
    public string 교역방향 { get; set; } = string.Empty;
    public string 품목명 { get; set; } = string.Empty;
    public string 품목설명 { get; set; } = string.Empty;
    public decimal 희망수량 { get; set; }
    public string 수량단위 { get; set; } = string.Empty;
    public string 원산지국가 { get; set; } = string.Empty;
    public string 목적지국가 { get; set; } = string.Empty;
    public DateOnly? 희망시작일 { get; set; }
    public DateOnly? 희망종료일 { get; set; }
    public string 물류조건 { get; set; } = string.Empty;
    public decimal? 예상금액 { get; set; }
    public string 통화Code { get; set; } = string.Empty;
    public bool 통관검토필요여부 { get; set; }
    public string 제안내용 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 아파트측결정 { get; set; } = string.Empty;
    public string 아파트측의견 { get; set; } = string.Empty;
    public DateTime? 아파트측결정AtUtc { get; set; }
    public string 상인회측결정 { get; set; } = string.Empty;
    public string 상인회측의견 { get; set; } = string.Empty;
    public DateTime? 상인회측결정AtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public static class 전통시장협의체역할Codes
{
    public const string 아파트대표 = "아파트대표";
    public const string 상인회대표 = "상인회대표";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            아파트대표 => 아파트대표,
            상인회대표 => 상인회대표,
            _ => string.Empty
        };
}

public static class 전통시장협의체상태Codes
{
    public const string 초대중 = "초대중";
    public const string 협의중 = "협의중";
    public const string 종료 = "종료";
}

public static class 전통시장교역방향Codes
{
    public const string 수입 = "수입";
    public const string 수출 = "수출";

    public static string Normalize(string? value)
        => value?.Trim() switch
        {
            수입 => 수입,
            수출 => 수출,
            _ => string.Empty
        };
}

public static class 전통시장협의결정Codes
{
    public const string 대기 = "대기";
    public const string 동의 = "동의";
    public const string 보완요청 = "보완요청";
    public const string 반대 = "반대";

    public static string NormalizeDecision(string? value)
        => value?.Trim() switch
        {
            동의 => 동의,
            보완요청 => 보완요청,
            반대 => 반대,
            _ => string.Empty
        };
}

public static class 전통시장교역안건상태Codes
{
    public const string 검토중 = "검토중";
    public const string 합의 = "합의";
    public const string 보완요청 = "보완요청";
    public const string 반려 = "반려";
    public const string 철회 = "철회";
}

public static class 전통시장생활권협의참조
{
    public static string 협의체(Guid 협의체Id) => $"traditional-market-council:{협의체Id:N}";
    public static string 안건(Guid 안건Id) => $"traditional-market-trade-agenda:{안건Id:N}";
}
