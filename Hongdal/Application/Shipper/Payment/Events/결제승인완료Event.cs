namespace Hongdal.Application.Shipper.Payment.Events;

public sealed record 결제승인완료Event(
    long 결제레코드Id,
    string 결제Id,
    int 결제대상유형,
    string 대상Id,
    int 결제제공자,
    int 결제금액,
    string 통화,
    DateTime 승인일시Utc) : INotification;
