namespace Hongdal.Contracts.Shipper.Payment;

public sealed class 결제목록응답
{
    public string 결제Id { get; set; } = string.Empty;
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public int 결제금액 { get; set; }
    public string 결제수단 { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string? PaymentKey { get; set; }
    public string? Toss응답Json { get; set; }
    public DateTime 생성일시Utc { get; set; }
    public DateTime? 승인일시Utc { get; set; }
}

public sealed class 토스결제준비요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class 토스결제준비응답
{
    public string 결제Id { get; set; } = string.Empty;
    public string 의뢰Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string ClientKey { get; set; } = string.Empty;
}

public sealed class 토스결제환경응답
{
    public string ClientKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}

public sealed class 토스결제승인요청
{
    public string PaymentKey { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class 토스결제승인응답
{
    public string 결제Id { get; set; } = string.Empty;
    public string 의뢰Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentKey { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 결제응답 { get; set; } = string.Empty;
}