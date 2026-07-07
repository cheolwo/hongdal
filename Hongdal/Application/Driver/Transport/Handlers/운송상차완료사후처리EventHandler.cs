using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using 홍달.Services.Documents;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송상차완료사후처리EventHandler : INotificationHandler<운송상차완료됨Event>
{
    private readonly I문서관리Service _문서관리Service;
    private readonly ILogger<운송상차완료사후처리EventHandler> _logger;

    public 운송상차완료사후처리EventHandler(
        I문서관리Service 문서관리Service,
        ILogger<운송상차완료사후처리EventHandler> logger)
    {
        _문서관리Service = 문서관리Service;
        _logger = logger;
    }

    public async Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
    {
        if (notification.인수증증빙 is null)
        {
            return;
        }

        try
        {
            await using var content = new MemoryStream(CreateReceiptPickupEvidenceBytes(notification));
            await _문서관리Service.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = notification.운송번호,
                배송운송Id = notification.운송Id,
                문서코드 = "상차인수확인서",
                문서명 = "상차 인수 확인서",
                파일명 = $"상차인수확인서-{notification.운송번호}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.기사Id
            }, content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Pickup receipt evidence document generation failed for TransportId={TransportId} RequestId={RequestId}",
                notification.운송Id,
                notification.운송번호);
        }
    }

    private static byte[] CreateReceiptPickupEvidenceBytes(운송상차완료됨Event notification)
    {
        var evidence = notification.인수증증빙!;
        var lines = new[]
        {
            "홍달 상차 인수 확인서",
            $"운송번호: {notification.운송번호}",
            $"기사ID: {notification.기사Id}",
            $"출발지: {notification.출발지}",
            $"도착지: {notification.도착지}",
            $"상태: {notification.현재상태}",
            $"상차확인시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
            $"서명필수여부: {(evidence.서명필수여부 ? "필수" : "선택")}",
            $"서명확보여부: {(evidence.서명확보됨 ? "확보" : "생략")}",
            $"증빙방식: {evidence.증빙방식}",
            $"인수자명: {evidence.인수자명 ?? string.Empty}",
            $"인수자소속: {evidence.인수자소속 ?? string.Empty}",
            $"인수자서명: {evidence.인수자서명 ?? string.Empty}",
            $"기사서명: {evidence.기사서명 ?? string.Empty}",
            $"서명생략사유: {evidence.서명생략사유 ?? string.Empty}",
            $"상차사진ObjectName: {evidence.상차사진ObjectName ?? string.Empty}",
            $"상차사진Url: {evidence.상차사진Url ?? string.Empty}",
            $"TraceId: {notification.TraceId}"
        };

        return Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
    }
}
