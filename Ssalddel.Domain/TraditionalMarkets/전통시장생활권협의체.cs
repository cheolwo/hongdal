using Ssalddel.Contracts.Common.TraditionalMarkets;

namespace Ssalddel.Domain.TraditionalMarkets;

public sealed class 전통시장생활권협의체
{
    public Guid Id { get; set; }
    public string 시장Code { get; set; } = string.Empty;
    public string 협의체명 { get; set; } = string.Empty;
    public string 아파트단지명 { get; set; } = string.Empty;
    public string 아파트주소 { get; set; } = string.Empty;
    public string 아파트대표UserId { get; set; } = string.Empty;
    public string 아파트대표명 { get; set; } = string.Empty;
    public DateTime? 아파트대표수락AtUtc { get; set; }
    public string 상인회명 { get; set; } = string.Empty;
    public string 상인회대표UserId { get; set; } = string.Empty;
    public string 상인회대표명 { get; set; } = string.Empty;
    public DateTime? 상인회대표수락AtUtc { get; set; }
    public string 협의목적 { get; set; } = string.Empty;
    public string 상태 { get; set; } = 전통시장협의체상태Codes.초대중;
    public string CreatedByUserId { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<전통시장교역안건> 안건 { get; set; } = new List<전통시장교역안건>();
}

public sealed class 전통시장교역안건
{
    public Guid Id { get; set; }
    public Guid 협의체Id { get; set; }
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
    public string 상태 { get; set; } = 전통시장교역안건상태Codes.검토중;
    public string 아파트측결정 { get; set; } = 전통시장협의결정Codes.대기;
    public string 아파트측의견 { get; set; } = string.Empty;
    public DateTime? 아파트측결정AtUtc { get; set; }
    public string 상인회측결정 { get; set; } = 전통시장협의결정Codes.대기;
    public string 상인회측의견 { get; set; } = string.Empty;
    public DateTime? 상인회측결정AtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public long Revision { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public 전통시장생활권협의체 협의체 { get; set; } = null!;
}

public static class 전통시장생활권협의Policy
{
    public static string 참여역할(전통시장생활권협의체 협의체, string userId)
    {
        if (string.Equals(협의체.아파트대표UserId, userId, StringComparison.Ordinal))
        {
            return 전통시장협의체역할Codes.아파트대표;
        }

        return string.Equals(협의체.상인회대표UserId, userId, StringComparison.Ordinal)
            ? 전통시장협의체역할Codes.상인회대표
            : string.Empty;
    }

    public static string 안건상태(string 아파트측결정, string 상인회측결정)
    {
        if (아파트측결정 == 전통시장협의결정Codes.반대
            || 상인회측결정 == 전통시장협의결정Codes.반대)
        {
            return 전통시장교역안건상태Codes.반려;
        }

        if (아파트측결정 == 전통시장협의결정Codes.보완요청
            || 상인회측결정 == 전통시장협의결정Codes.보완요청)
        {
            return 전통시장교역안건상태Codes.보완요청;
        }

        return 아파트측결정 == 전통시장협의결정Codes.동의
               && 상인회측결정 == 전통시장협의결정Codes.동의
            ? 전통시장교역안건상태Codes.합의
            : 전통시장교역안건상태Codes.검토중;
    }
}
