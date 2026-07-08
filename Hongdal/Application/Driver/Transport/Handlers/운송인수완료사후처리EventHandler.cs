using System.Globalization;
using System.Text;
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
            await using var pdf = new MemoryStream(CreateReceiptPdfBytes(notification));
            await _문서관리Service.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = notification.운송번호,
                배송운송Id = notification.운송Id,
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

    private static byte[] CreateReceiptPdfBytes(운송인수완료됨Event notification)
    {
        var lines = new[]
        {
            "홍달 운송 인수증",
            $"운송번호: {notification.운송번호}",
            $"기사ID: {notification.기사Id}",
            $"출발지: {notification.출발지}",
            $"도착지: {notification.도착지}",
            $"상태: {notification.상태}",
            $"완료시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
            $"하차사진ObjectName: {notification.하차완료증빙?.하차사진ObjectName ?? string.Empty}",
            $"하차사진Url: {notification.하차완료증빙?.하차사진Url ?? string.Empty}"
        };

        return BuildMinimalPdf(lines);
    }

    private static byte[] BuildMinimalPdf(IReadOnlyList<string> lines)
    {
        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 12 Tf");
        contentBuilder.AppendLine("50 780 Td");

        for (var index = 0; index < lines.Count; index++)
        {
            if (index > 0)
            {
                contentBuilder.AppendLine("0 -18 Td");
            }

            contentBuilder.AppendLine($"({EscapePdfText(lines[index])}) Tj");
        }

        contentBuilder.AppendLine("ET");
        var contentBytes = Encoding.ASCII.GetBytes(contentBuilder.ToString());

        using var stream = new MemoryStream();
        void Write(string text) => stream.Write(Encoding.ASCII.GetBytes(text));

        var offsets = new List<long>();
        Write("%PDF-1.4\n");

        offsets.Add(stream.Position);
        Write("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        Write("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets.Add(stream.Position);
        Write("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>\nendobj\n");

        offsets.Add(stream.Position);
        Write($"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        stream.Write(contentBytes);
        Write("endstream\nendobj\n");

        offsets.Add(stream.Position);
        Write("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

        var xrefStart = stream.Position;
        Write("xref\n0 6\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            Write($"{offset:0000000000} 00000 n \n");
        }

        Write("trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n");
        Write(xrefStart.ToString(CultureInfo.InvariantCulture));
        Write("\n%%EOF");

        return stream.ToArray();
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
