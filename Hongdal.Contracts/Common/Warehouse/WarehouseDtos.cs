namespace Hongdal.Contracts.Common.Warehouse;

public sealed class 창고요약응답
{
    public long Id { get; set; }
    public string 창고명 { get; set; } = string.Empty;
    public string 소유자UserId { get; set; } = string.Empty;
    public string 소유자유형 { get; set; } = string.Empty;
    public string 창고유형 { get; set; } = string.Empty;
    public string 물류대행지분류 { get; set; } = LogisticsProxySiteTypes.DeliveryAgency;
    public string 주소 { get; set; } = string.Empty;
    public string 담당자명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public decimal? 위도 { get; set; }
    public decimal? 경도 { get; set; }
    public bool 기본창고여부 { get; set; }
    public bool IsActive { get; set; }
}

public sealed class 창고목록응답
{
    public IReadOnlyList<창고요약응답> Items { get; set; } = [];
}

public sealed class 창고저장요청
{
    public string 창고명 { get; set; } = string.Empty;
    public string 소유자유형 { get; set; } = string.Empty;
    public string 창고유형 { get; set; } = string.Empty;
    public string 물류대행지분류 { get; set; } = LogisticsProxySiteTypes.DeliveryAgency;
    public string 주소 { get; set; } = string.Empty;
    public string 담당자명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public decimal? 위도 { get; set; }
    public decimal? 경도 { get; set; }
    public bool 기본창고여부 { get; set; } = true;
}

public sealed class 창고사용자항목응답
{
    public long Id { get; set; }
    public long 창고Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 역할명 { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class 창고사용자목록응답
{
    public IReadOnlyList<창고사용자항목응답> Items { get; set; } = [];
}

public sealed class 창고사용자저장요청
{
    public string UserId { get; set; } = string.Empty;
    public string 역할명 { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
