namespace Hongdal.Contracts.Common.Payments;

public static class 계약결제대상유형
{
    public const int 음식주문 = 10;
    public const int 용달운송의뢰 = 20;
    public const int 기사이용료 = 30;
    public const int 회원구독 = 40;
    public const int 후원 = 50;
}

public static class 계약결제제공자
{
    public const int TossPayments = 10;
    public const int NaverPay = 20;
    public const int KakaoPay = 30;
    public const int ManualBankTransfer = 90;
}

public sealed class 공통결제준비요청
{
    public int 결제대상유형 { get; set; }
    public string 대상Id { get; set; } = string.Empty;
    public int 결제제공자 { get; set; } = 계약결제제공자.TossPayments;
    public int 금액 { get; set; }
    public string? 주문명 { get; set; }
}

public sealed class 공통결제준비응답
{
    public string 결제요청Id { get; set; } = string.Empty;
    public string 대상Id { get; set; } = string.Empty;
    public int 결제제공자 { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string ClientKey { get; set; } = string.Empty;
}

public sealed class 공통결제승인요청
{
    public int 결제제공자 { get; set; } = 계약결제제공자.TossPayments;
    public string PaymentKey { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public sealed class 공통결제승인응답
{
    public string 결제요청Id { get; set; } = string.Empty;
    public string 대상Id { get; set; } = string.Empty;
    public int 결제제공자 { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string PaymentKey { get; set; } = string.Empty;
    public string 결제상태 { get; set; } = string.Empty;
    public string 결제응답 { get; set; } = string.Empty;
}
