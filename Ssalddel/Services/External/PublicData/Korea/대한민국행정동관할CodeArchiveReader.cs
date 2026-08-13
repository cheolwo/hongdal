using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace 살뜰.Services.External.PublicData.Korea;

internal sealed record 대한민국행정기관CodeRow(
    string 행정기관Code,
    string 전체명,
    string 행정계층Code,
    string? 상위행정기관Code,
    DateOnly 생성일,
    DateOnly? 말소일);

internal sealed record 대한민국행정동법정동관할Row(
    string 행정기관Code,
    string 행정기관명,
    string 법정동Code,
    string 법정동명,
    DateOnly 생성일,
    DateOnly? 말소일);

internal sealed record 대한민국행정동관할Archive(
    string 기준일,
    IReadOnlyList<대한민국행정기관CodeRow> 행정기관들,
    IReadOnlyList<대한민국행정동법정동관할Row> 관할관계들)
{
    public int RecordCount => 행정기관들.Count + 관할관계들.Count;
}

internal static class 대한민국행정동관할CodeArchiveReader
{
    internal static 대한민국행정동관할Archive Read(
        byte[] archiveBytes,
        int maxExpandedBytes,
        int maxRecordCount)
    {
        ArgumentNullException.ThrowIfNull(archiveBytes);
        if (archiveBytes.Length < 4 || archiveBytes[0] != 0x50 || archiveBytes[1] != 0x4B)
            throw new InvalidDataException("AdministrativeJurisdictionArchiveZipRequired");
        if (maxExpandedBytes < 1024)
            throw new ArgumentOutOfRangeException(nameof(maxExpandedBytes));
        if (maxRecordCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxRecordCount));

        using var archiveStream = new MemoryStream(archiveBytes, writable: false);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        var administrativeEntry = FindEntry(archive, "KIKcd_H.");
        var jurisdictionEntry = FindEntry(archive, "KIKmix.");
        var expandedBytes = administrativeEntry.Length + jurisdictionEntry.Length;
        if (expandedBytes <= 0 || expandedBytes > maxExpandedBytes)
            throw new InvalidDataException("AdministrativeJurisdictionArchiveExpandedSizeInvalid");

        var revision = administrativeEntry.Name["KIKcd_H.".Length..];
        if (revision.Length != 8 || revision.Any(character => character is < '0' or > '9')
            || jurisdictionEntry.Name != $"KIKmix.{revision}")
            throw new InvalidDataException("AdministrativeJurisdictionRevisionInvalid");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var administrativeRows = ReadAdministrativeRows(administrativeEntry, maxRecordCount);
        var remaining = maxRecordCount - administrativeRows.Count;
        var jurisdictionRows = ReadJurisdictionRows(jurisdictionEntry, remaining);
        if (administrativeRows.Count == 0 || jurisdictionRows.Count == 0)
            throw new InvalidDataException("AdministrativeJurisdictionArchiveEmpty");
        return new 대한민국행정동관할Archive(revision, administrativeRows, jurisdictionRows);
    }

    private static ZipArchiveEntry FindEntry(ZipArchive archive, string prefix)
    {
        var matches = archive.Entries
            .Where(entry => entry.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            && !entry.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return matches.Length == 1
            ? matches[0]
            : throw new InvalidDataException("AdministrativeJurisdictionArchiveEntryInvalid");
    }

    private static IReadOnlyList<대한민국행정기관CodeRow> ReadAdministrativeRows(
        ZipArchiveEntry entry,
        int maxRecordCount)
    {
        var lines = ReadLines(entry);
        if (lines.Count == 0
            || !Decode(lines[0], 0, lines[0].Length).StartsWith("행정동코드", StringComparison.Ordinal))
            throw new InvalidDataException("AdministrativeCodeHeaderInvalid");
        var rows = new List<대한민국행정기관CodeRow>();
        var codes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in lines.Skip(1))
        {
            if (line.Length == 0 || line.All(value => value is 0 or 9 or 32)) continue;
            if (rows.Count >= maxRecordCount)
                throw new InvalidDataException("AdministrativeJurisdictionRecordLimitExceeded");
            var code = Decode(line, 0, 10);
            var province = Decode(line, 11, 30);
            var cityCounty = Decode(line, 42, 30);
            var townNeighborhood = Decode(line, 73, 30);
            var created = ParseRequiredDate(Decode(line, 104, 8));
            var abolished = ParseOptionalDate(Decode(line, 113, 8));
            if (!IsCode(code) || !codes.Add(code) || province.Length == 0)
                throw new InvalidDataException("AdministrativeCodeRowInvalid");
            var fullName = string.Join(' ', new[] { province, cityCounty, townNeighborhood }
                .Where(value => value.Length > 0));
            var level = townNeighborhood.Length > 0
                ? "town-neighborhood"
                : cityCounty.Length > 0 ? "city-county-district" : "province";
            var parent = level switch
            {
                "province" => null,
                "city-county-district" => code[..2] + "00000000",
                _ => code[..5] + "00000",
            };
            rows.Add(new 대한민국행정기관CodeRow(code, fullName, level, parent, created, abolished));
        }
        return rows;
    }

    private static IReadOnlyList<대한민국행정동법정동관할Row> ReadJurisdictionRows(
        ZipArchiveEntry entry,
        int maxRecordCount)
    {
        var lines = ReadLines(entry);
        if (lines.Count == 0
            || !Decode(lines[0], 0, lines[0].Length).StartsWith("행정동코드", StringComparison.Ordinal))
            throw new InvalidDataException("AdministrativeJurisdictionHeaderInvalid");
        var rows = new List<대한민국행정동법정동관할Row>();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in lines.Skip(1))
        {
            if (line.Length == 0 || line.All(value => value is 0 or 9 or 32)) continue;
            if (rows.Count >= maxRecordCount)
                throw new InvalidDataException("AdministrativeJurisdictionRecordLimitExceeded");
            var administrativeCode = Decode(line, 0, 10);
            var province = Decode(line, 11, 30);
            var cityCounty = Decode(line, 42, 30);
            var administrativeName = Decode(line, 73, 30);
            var legalCode = Decode(line, 104, 10);
            var legalName = Decode(line, 115, 30);
            var created = ParseRequiredDate(Decode(line, 146, 8));
            var abolished = ParseOptionalDate(Decode(line, 155, 8));
            var key = $"{administrativeCode}:{legalCode}:{created:yyyyMMdd}:{abolished:yyyyMMdd}";
            if (!IsCode(administrativeCode) || !IsCode(legalCode) || !keys.Add(key)
                || province.Length == 0)
                throw new InvalidDataException("AdministrativeJurisdictionRowInvalid");
            var fullAdministrativeName = string.Join(' ',
                new[] { province, cityCounty, administrativeName }.Where(value => value.Length > 0));
            rows.Add(new 대한민국행정동법정동관할Row(
                administrativeCode,
                fullAdministrativeName,
                legalCode,
                legalName,
                created,
                abolished));
        }
        return rows;
    }

    private static IReadOnlyList<byte[]> ReadLines(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var output = new MemoryStream();
        stream.CopyTo(output);
        var bytes = output.ToArray();
        var lines = new List<byte[]>();
        var start = 0;
        for (var index = 0; index <= bytes.Length; index++)
        {
            if (index < bytes.Length && bytes[index] != (byte)'\n') continue;
            var end = index;
            if (end > start && bytes[end - 1] == (byte)'\r') end--;
            lines.Add(end == start ? [] : bytes[start..end]);
            start = index + 1;
        }
        return lines;
    }

    private static string Decode(byte[] value, int start, int length)
        => start >= value.Length
            ? string.Empty
            : Encoding.GetEncoding(949)
                .GetString(value, start, Math.Min(length, value.Length - start))
                .Trim();

    private static bool IsCode(string value)
        => value.Length == 10 && value.All(character => character is >= '0' and <= '9');

    private static DateOnly ParseRequiredDate(string value)
        => DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed)
            ? parsed
            : throw new InvalidDataException("AdministrativeJurisdictionDateInvalid");

    private static DateOnly? ParseOptionalDate(string value)
        => value.Length == 0
            ? null
            : ParseRequiredDate(value);
}
