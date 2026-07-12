using MediatR;
using Microsoft.Extensions.Logging;
using 홍달.Services.Documents;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송인수완료사후처리EventHandler : INotificationHandler<운송인수완료됨Event>
{
    private readonly I문서관리Service _문서관리Service;
    private readonly ILogger<운송인수완료사후처리EventHandler> _logger;

    public 운송인수완료사후처리EventHandler(I문서관리Service 문서관리Service, ILogger<운송인수완료사후처리EventHandler> logger)
    {
        _문서관리Service = 문서관리Service;
        _logger = logger;
    }

    public async Task Handle(운송인수완료됨Event notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportCompleted",
            notification.기사Id,
            notification.운송Id,
            notification.상태,
            "Success",
            notification.TraceId,
            notification.발생시각Utc);

        await TryCreateReceiptDocumentAsync(notification, cancellationToken);
    }

    private async Task TryCreateReceiptDocumentAsync(운송인수완료됨Event notification, CancellationToken cancellationToken)
    {
        try
        {
            await using var pdf = new MemoryStream(운송인수증문서Factory.Create운송인수증PdfBytes(notification));
            await _문서관리Service.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = notification.운송번호,
                운송원장Id = notification.운송Id,
                문서코드 = "인수증",
                문서명 = "인수증",
                파일명 = $"인수증-{notification.운송번호}.pdf",
                ContentType = "application/pdf",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.기사Id
            }, pdf, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Receipt document auto generation failed for TransportId={TransportId} RequestId={RequestId}", notification.운송Id, notification.운송번호);
        }
    }

}
