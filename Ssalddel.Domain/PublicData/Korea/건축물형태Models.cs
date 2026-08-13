namespace Ssalddel.Domain.PublicData.Korea;

public static class 건축물형태근거종류Codes
{
    public const string 관측값우선 = "ObservedPreferred";
    public const string 일부추정 = "PartiallyDerived";
    public const string 추정값 = "Derived";
    public const string 자료부족 = "InsufficientData";
}

public sealed class 건축물형태Profile
{
    public Guid Id { get; set; }
    public Guid 건축물RecordId { get; set; }
    public 건축물대장표제부Record 건축물Record { get; set; } = null!;
    public int? 관측지상층수 { get; set; }
    public int? 추정지상층수 { get; set; }
    public int 표현지상층수 { get; set; }
    public decimal? 공식건폐율Percent { get; set; }
    public decimal? 공식용적률Percent { get; set; }
    public decimal? 단순건폐비율Percent { get; set; }
    public decimal? 단순연면적대지비율Percent { get; set; }
    public decimal? 대지면적SquareMeters { get; set; }
    public decimal? 건축면적SquareMeters { get; set; }
    public decimal? 연면적SquareMeters { get; set; }
    public decimal? 높이Meters { get; set; }
    public decimal? 추정층고Meters { get; set; }
    public string 건물바닥면적등급Code { get; set; } = string.Empty;
    public string 높이등급Code { get; set; } = string.Empty;
    public string 밀도등급Code { get; set; } = string.Empty;
    public string 근거종류Code { get; set; } = string.Empty;
    public string 규칙개정번호 { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset 생성시각Utc { get; set; }
}

public sealed class 건축물시각구성계획
{
    public Guid Id { get; set; }
    public Guid 건축물형태ProfileId { get; set; }
    public 건축물형태Profile 건축물형태Profile { get; set; } = null!;
    public string 시각FamilyCode { get; set; } = string.Empty;
    public int 기준층수 { get; set; }
    public int 중간층반복수 { get; set; }
    public string 대지점유등급Code { get; set; } = string.Empty;
    public string 주변여백등급Code { get; set; } = string.Empty;
    public string LOD등급Code { get; set; } = string.Empty;
    public bool 표현전용 { get; set; } = true;
    public string 규칙개정번호 { get; set; } = string.Empty;
    public string 계획HashSha256 { get; set; } = string.Empty;
    public DateTimeOffset 생성시각Utc { get; set; }
}

public sealed record 건축물형태분석결과(
    int? 관측지상층수,
    int? 추정지상층수,
    int 표현지상층수,
    decimal? 공식건폐율Percent,
    decimal? 공식용적률Percent,
    decimal? 단순건폐비율Percent,
    decimal? 단순연면적대지비율Percent,
    decimal? 추정층고Meters,
    string 건물바닥면적등급Code,
    string 높이등급Code,
    string 밀도등급Code,
    string 근거종류Code);

public sealed record 건축물시각구성결과(
    string 시각FamilyCode,
    int 기준층수,
    int 중간층반복수,
    string 대지점유등급Code,
    string 주변여백등급Code,
    string LOD등급Code);
