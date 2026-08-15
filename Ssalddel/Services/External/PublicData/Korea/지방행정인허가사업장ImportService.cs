using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record 지방행정인허가사업장ImportRequest(
    string SourceRevision,
    string SourceHashSha256,
    DateTimeOffset ObservedAtUtc,
    long? EvidenceSnapshotId = null,
    string EncodingName = "utf-8",
    string SourceId = 지방행정인허가사업장ImportService.SourceId,
    string DatasetId = 지방행정인허가사업장ImportService.DatasetId,
    string? DefaultOpenServiceId = null,
    string? DefaultOpenServiceName = null);

public sealed record 지방행정인허가사업장ImportResult(
    int ParsedCount,
    int InsertedCount,
    int ExistingCount,
    int RejectedCount);

public sealed class 지방행정인허가사업장ImportService
{
    public const string SourceId = "mois-localdata";
    public const string DatasetId = "korea-local-government-licensed-businesses";
    private const int BatchSize = 1_000;

    private readonly PublicDataIngestionDbContext _db;

    public 지방행정인허가사업장ImportService(PublicDataIngestionDbContext db)
    {
        _db = db;
    }

    public async Task<지방행정인허가사업장ImportResult> ImportCsvAsync(
        Stream csv,
        지방행정인허가사업장ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(csv);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DatasetId);
        if (request.SourceHashSha256.Length != 64)
            throw new ArgumentException("원본 SHA-256은 64자리여야 합니다.", nameof(request));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding(request.EncodingName);
        using var reader = new StreamReader(csv, encoding, true, leaveOpen: true);
        using var rows = CsvRows(reader).GetEnumerator();
        if (!rows.MoveNext())
            return new 지방행정인허가사업장ImportResult(0, 0, 0, 0);

        var header = rows.Current
            .Select((name, index) => new { Name = NormalizeHeader(name), Index = index })
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Index, StringComparer.OrdinalIgnoreCase);
        var parsed = 0;
        var inserted = 0;
        var existing = 0;
        var rejected = 0;
        var batch = new List<공개인허가사업장Record>(BatchSize);

        while (rows.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            parsed++;
            var values = rows.Current;
            var serviceId = FirstNotEmpty(
                Get(values, header, "개방서비스아이디", "개방서비스ID", "OPNSFTEAMCODE"),
                request.DefaultOpenServiceId);
            var managementNumber = Get(values, header, "관리번호", "인허가번호", "MGTNO");
            var businessName = Get(values, header, "사업장명", "업소명", "BPLCNM");
            if (string.IsNullOrWhiteSpace(serviceId)
                || string.IsNullOrWhiteSpace(managementNumber)
                || string.IsNullOrWhiteSpace(businessName))
            {
                rejected++;
                continue;
            }

            var roadAddress = NullIfEmpty(Get(
                values,
                header,
                "도로명전체주소",
                "도로명주소",
                "소재지(도로명)",
                "RDNWHLADDR"));
            batch.Add(new 공개인허가사업장Record
            {
                Id = 공공데이터원장식별자.결정적Guid(
                    $"{request.SourceId}|{serviceId}|{managementNumber}|{request.SourceRevision}"),
                SourceId = request.SourceId,
                SourceDatasetId = request.DatasetId,
                OpenServiceId = serviceId,
                OpenServiceName = FirstNotEmpty(
                    NullIfEmpty(Get(values, header, "개방서비스명", "OPNSVCNM")),
                    request.DefaultOpenServiceName),
                ManagementNumber = managementNumber,
                BusinessName = businessName.Trim(),
                BusinessTypeName = NullIfEmpty(Get(values, header, "업태구분명", "업태명", "UPTAENM")),
                LicenseCategoryName = NullIfEmpty(Get(values, header, "업종명", "위생업종명", "SNTUPTAENM")),
                BusinessStatusCode = NullIfEmpty(Get(values, header, "영업상태구분코드", "TRDSTATEGBN")),
                BusinessStatusName = NullIfEmpty(Get(values, header, "영업상태명", "TRDSTATENM")),
                DetailedStatusCode = NullIfEmpty(Get(values, header, "상세영업상태코드", "DTLSTATEGBN")),
                DetailedStatusName = NullIfEmpty(Get(values, header, "상세영업상태명", "DTLSTATENM")),
                LotAddress = NullIfEmpty(Get(values, header, "소재지전체주소", "지번주소", "SITEWHLADDR")),
                RoadAddress = roadAddress,
                NormalizedRoadAddressKey = 공개사업장주소정규화Engine.NormalizeRoadAddress(roadAddress),
                SourceCoordinateX = Decimal(Get(values, header, "좌표정보X", "좌표정보(X)", "X")),
                SourceCoordinateY = Decimal(Get(values, header, "좌표정보Y", "좌표정보(Y)", "Y")),
                SourceCoordinateReferenceSystem = "SourceDeclaredOrUnknown",
                LicenseDate = Date(Get(values, header, "인허가일자", "APVPERMYMD")),
                ClosureDate = Date(Get(values, header, "폐업일자", "DcbYmd")),
                SourceLastModifiedAt = DateTimeValue(Get(values, header, "최종수정시점", "UPTAEDATETS")),
                SourceRevision = request.SourceRevision,
                SourceHashSha256 = request.SourceHashSha256.ToLowerInvariant(),
                EvidenceSnapshotId = request.EvidenceSnapshotId,
                ObservedAtUtc = request.ObservedAtUtc,
            });

            if (batch.Count >= BatchSize)
            {
                var result = await SaveBatchAsync(batch, cancellationToken);
                inserted += result.Inserted;
                existing += result.Existing;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            var result = await SaveBatchAsync(batch, cancellationToken);
            inserted += result.Inserted;
            existing += result.Existing;
        }

        return new 지방행정인허가사업장ImportResult(parsed, inserted, existing, rejected);
    }

    private async Task<(int Inserted, int Existing)> SaveBatchAsync(
        IReadOnlyCollection<공개인허가사업장Record> batch,
        CancellationToken cancellationToken)
    {
        var ids = batch.Select(item => item.Id).ToList();
        var existingIds = await _db.공개인허가사업장Records
            .Where(item => ids.Contains(item.Id))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var existingSet = existingIds.ToHashSet();
        var additions = batch.Where(item => !existingSet.Contains(item.Id)).ToArray();
        _db.공개인허가사업장Records.AddRange(additions);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return (additions.Length, existingSet.Count);
    }

    private static IEnumerable<string[]> CsvRows(TextReader reader)
    {
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        while (reader.Read() is var value && value >= 0)
        {
            var character = (char)value;
            if (quoted)
            {
                if (character == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        quoted = false;
                    }
                }
                else
                {
                    field.Append(character);
                }
                continue;
            }

            switch (character)
            {
                case '"' when field.Length == 0:
                    quoted = true;
                    break;
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    yield return row.ToArray();
                    row.Clear();
                    break;
                default:
                    field.Append(character);
                    break;
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            yield return row.ToArray();
        }
    }

    private static string NormalizeHeader(string value) =>
        value.Trim().TrimStart('\uFEFF').Replace(" ", string.Empty, StringComparison.Ordinal);

    private static string Get(string[] values, IReadOnlyDictionary<string, int> header, params string[] names)
    {
        foreach (var name in names.Select(NormalizeHeader))
        {
            if (header.TryGetValue(name, out var index) && index < values.Length)
                return values[index].Trim();
        }
        return string.Empty;
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? FirstNotEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static decimal? Decimal(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : null;

    private static DateOnly? Date(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length >= 8
            && DateOnly.TryParseExact(digits[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                ? result
                : null;
    }

    private static DateTimeOffset? DateTimeValue(string value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
            ? result
            : null;

}
