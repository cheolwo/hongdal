namespace Hongdal.Contracts.Driver.Reservation;

public sealed class 기사예약요청
{
    public string 시작모드 { get; set; } = "reserved";
    public DateTime? 시작시각 { get; set; }
    public string 시작위치 { get; set; } = string.Empty;
    public string? 복귀지 { get; set; }
}

public class 기사예약응답
{
    public long Id { get; set; }
    public string DriverId { get; set; } = string.Empty;
    public string StartMode { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public string StartLocation { get; set; } = string.Empty;
    public string? ReturnDestination { get; set; }
}

public sealed class 기사예약목록응답 : 기사예약응답
{
    public bool IsFuture { get; set; }
}

public sealed class 기사예약취소응답
{
    public long Id { get; set; }
    public string DriverId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}