using System.Globalization;
using System.Text;

namespace Ssalddel.Application.Driver.Transport;

public static class 운송인수증문서Factory
{
    public static byte[] Create상차인수확인서Bytes(운송상차완료됨Event notification)
    {
        var evidence = notification.인수증증빙!;
        var lines = new[]
        {
            "살뜰 상차 인수 확인서",
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

    public static byte[] Create운송인수증PdfBytes(운송인수완료됨Event notification)
    {
        var lines = new[]
        {
            "살뜰 운송 인수증",
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
