using Hongdal.Contracts.Driver.Transport;
using FluentResults;
using Microsoft.Extensions.Logging;
using 홍달.Services.Documents;
using System.Globalization;
using System.Text;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송인수완료CommandHandler : IRequestHandler<운송인수완료Command, Result<기사운송상태변경응답>>
{
    private readonly HongdalContext _db;
    private readonly I기사운송상태전이Service _상태전이Service;
    private readonly I문서관리Service _문서관리Service;
    private readonly ILogger<운송인수완료CommandHandler> _logger;

    public 운송인수완료CommandHandler(HongdalContext db, I기사운송상태전이Service 상태전이Service, I문서관리Service 문서관리Service, ILogger<운송인수완료CommandHandler> logger)
    {
        _db = db;
        _상태전이Service = 상태전이Service;
        _문서관리Service = 문서관리Service;
        _logger = logger;
    }

    public async Task<Result<기사운송상태변경응답>> Handle(운송인수완료Command request, CancellationToken cancellationToken)
    {
        var entity = await _db.배송_운송
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken);
        if (entity is null)
        {
            return Result.Fail<기사운송상태변경응답>("운송을 찾을 수 없습니다.");
        }

        var 이전상태 = entity.상태;
        var now = DateTime.UtcNow;
        var 상태변경 = _상태전이Service.상태변경(entity, "인수완료", now);
        if (상태변경.IsFailed)
        {
            return Result.Fail<기사운송상태변경응답>(상태변경.Errors.Select(x => x.Message));
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Action={Action} DriverId={DriverId} TransportId={TransportId} BeforeStatus={BeforeStatus} AfterStatus={AfterStatus} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}",
            "TransportCompleted",
            request.기사Id,
            entity.Id,
            이전상태,
            entity.상태,
            "Success",
            System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty,
            now);

        await TryCreateReceiptDocumentAsync(entity, now, cancellationToken);

        return Result.Ok(new 기사운송상태변경응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            UpdatedAt = entity.UpdatedAt
        });
    }

    private async Task TryCreateReceiptDocumentAsync(배송_운송 transport, DateTime completedAtUtc, CancellationToken cancellationToken)
    {
        try
        {
            await using var pdf = new MemoryStream(CreateReceiptPdfBytes(transport, completedAtUtc));
            await _문서관리Service.CreateDocumentAsync(new 문서생성요청
            {
                의뢰Id = transport.운송번호,
                배송운송Id = transport.Id,
                문서코드 = "인수증",
                문서명 = "인수증",
                파일명 = $"인수증-{transport.운송번호}.pdf",
                ContentType = "application/pdf",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = transport.기사_운송자
            }, pdf, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Receipt document auto generation failed for TransportId={TransportId} RequestId={RequestId}", transport.Id, transport.운송번호);
        }
    }

    private static byte[] CreateReceiptPdfBytes(배송_운송 transport, DateTime completedAtUtc)
    {
        var lines = new[]
        {
            "홍달 운송 인수증",
            $"운송번호: {transport.운송번호}",
            $"기사ID: {transport.기사_운송자}",
            $"출발지: {transport.출발지}",
            $"도착지: {transport.도착지}",
            $"상태: {transport.상태}",
            $"완료시각: {completedAtUtc:yyyy-MM-dd HH:mm:ss} UTC"
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
