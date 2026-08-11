using System.IO.Compression;
using System.Text;

namespace 살뜰.Services.External.PublicData.Korea;

internal sealed record 대한민국법정동CodeRow(
    string 법정동Code,
    string 전체명,
    string 상태Code,
    string 행정계층Code,
    string? 상위법정동Code);

internal static class 대한민국법정동CodeArchiveReader
{
    private const string ExpectedHeader = "법정동코드\t법정동명\t폐지여부";

    internal static async Task<byte[]> ReadAllBytesAsync(
        Stream source,
        int maxArchiveBytes,
        CancellationToken cancellationToken)
    {
        if (maxArchiveBytes < 1024)
            throw new ArgumentOutOfRangeException(nameof(maxArchiveBytes));

        using var output = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            if (output.Length + read > maxArchiveBytes)
                throw new InvalidDataException("LegalDongArchiveTooLarge");
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return output.ToArray();
    }

    internal static IReadOnlyList<대한민국법정동CodeRow> Read(
        byte[] archiveBytes,
        int maxExpandedBytes,
        int maxRecordCount)
    {
        ArgumentNullException.ThrowIfNull(archiveBytes);
        if (archiveBytes.Length < 4
            || archiveBytes[0] != 0x50
            || archiveBytes[1] != 0x4B)
            throw new InvalidDataException("LegalDongArchiveZipRequired");
        if (maxExpandedBytes < 1024)
            throw new ArgumentOutOfRangeException(nameof(maxExpandedBytes));
        if (maxRecordCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRecordCount));

        using var archiveStream = new MemoryStream(archiveBytes, writable: false);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        var textEntries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (textEntries.Length != 1)
            throw new InvalidDataException("LegalDongArchiveTextEntryInvalid");

        var entry = textEntries[0];
        if (entry.Length <= 0 || entry.Length > maxExpandedBytes)
            throw new InvalidDataException("LegalDongArchiveExpandedSizeInvalid");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var entryStream = entry.Open();
        using var reader = new StreamReader(
            entryStream,
            Encoding.GetEncoding(949),
            detectEncodingFromByteOrderMarks: true);
        var header = reader.ReadLine()?.TrimStart('\uFEFF');
        if (!string.Equals(header, ExpectedHeader, StringComparison.Ordinal))
            throw new InvalidDataException("LegalDongArchiveHeaderInvalid");

        var rows = new List<대한민국법정동CodeRow>();
        var codes = new HashSet<string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (rows.Count >= maxRecordCount)
                throw new InvalidDataException("LegalDongRecordLimitExceeded");

            var fields = line.Split('\t');
            if (fields.Length != 3)
                throw new InvalidDataException("LegalDongRowShapeInvalid");
            var code = fields[0].Trim();
            var name = fields[1].Trim();
            var status = fields[2].Trim() switch
            {
                "존재" => "active",
                "폐지" => "abolished",
                _ => string.Empty,
            };
            if (code.Length != 10
                || code.Any(character => character is < '0' or > '9')
                || name.Length == 0
                || status.Length == 0
                || !codes.Add(code))
                throw new InvalidDataException("LegalDongRowValueInvalid");

            rows.Add(new 대한민국법정동CodeRow(
                code,
                name,
                status,
                ResolveLevel(code),
                ResolveParent(code)));
        }

        if (rows.Count == 0)
            throw new InvalidDataException("LegalDongArchiveEmpty");
        return rows;
    }

    private static string ResolveLevel(string code)
    {
        // 세종특별자치시는 법정동코드 자릿수상 시군구처럼 보이지만
        // 현행 전체자료에서 별도 시도 상위행 없이 최상위 광역자치단체로 제공됩니다.
        if (code == "3611000000") return "province";
        if (code.AsSpan(2).IndexOfAnyExcept('0') < 0) return "province";
        if (code.AsSpan(5).IndexOfAnyExcept('0') < 0) return "city-county-district";
        if (code.AsSpan(8).IndexOfAnyExcept('0') < 0) return "town-neighborhood";
        return "village";
    }

    private static string? ResolveParent(string code)
        => ResolveLevel(code) switch
        {
            "province" => null,
            "city-county-district" => code[..2] + "00000000",
            "town-neighborhood" => code[..5] + "00000",
            _ => code[..8] + "00",
        };
}
