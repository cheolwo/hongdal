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
            await using var content = new MemoryStream(운송인수증문서Factory.Create상차인수확인서Bytes(notification));
            await _문서관리Service.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = notification.운송번호,
                운송원장Id = notification.운송Id,
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

}
