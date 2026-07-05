namespace Hongdal.Contracts.Driver.Home;

public sealed class 기사홈요약응답
{
    public string DriverId { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 홈상태문구 { get; set; } = string.Empty;
    public string 주요행동코드 { get; set; } = string.Empty;
    public string 주요행동문구 { get; set; } = string.Empty;
    public bool 운행중 { get; set; }
    public long? 현재근무Id { get; set; }
    public DateTime? 운행시작시각 { get; set; }
    public bool 진행중운송있음 { get; set; }
    public long? 현재운송Id { get; set; }
    public string? 현재운송단계 { get; set; }
    public int 추천콜수 { get; set; }
    public int 적합추천콜수 { get; set; }
    public int 오늘예약수 { get; set; }
    public DateTime? 다음예약시각 { get; set; }
    public int 진행중운송수 { get; set; }
    public int 이번달배차건수 { get; set; }
    public decimal 이번달이용료 { get; set; }
    public decimal 이번달이용료상한 { get; set; }
    public decimal 남은이용료 { get; set; }
    public bool 정산결제완료 { get; set; }
    public bool 푸시토큰등록됨 { get; set; }
    public bool 알림정상 { get; set; }
    public bool 전국콜사용가능 { get; set; }
    public IReadOnlyList<기사홈할일항목> 오늘할일 { get; set; } = [];
}

public sealed class 기사홈할일항목
{
    public string 종류 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string 이동경로 { get; set; } = string.Empty;
    public int 우선순위 { get; set; }
}
