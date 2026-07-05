namespace Hongdal.Contracts.Driver.Notification;

public sealed class 기사푸시토큰등록요청
{
    public string PushToken { get; set; } = string.Empty;
}

public sealed class 기사푸시토큰응답
{
    public string DriverId { get; set; } = string.Empty;
    public bool HasToken { get; set; }
    public string PushToken { get; set; } = string.Empty;
}

public sealed class 기사알림설정응답
{
    public string DriverId { get; set; } = string.Empty;
    public bool 배차추천알림사용 { get; set; }
    public bool 운전중푸시만사용 { get; set; }
    public bool 소리사용 { get; set; }
    public bool 진동사용 { get; set; }
    public bool 야간알림제한 { get; set; }
    public bool 정차후모아보기 { get; set; }
}

public sealed class 기사알림설정수정요청
{
    public bool 배차추천알림사용 { get; set; }
    public bool 운전중푸시만사용 { get; set; }
    public bool 소리사용 { get; set; }
    public bool 진동사용 { get; set; }
    public bool 야간알림제한 { get; set; }
    public bool 정차후모아보기 { get; set; }
}