using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;

namespace 살뜰.Services.External.PublicData.Korea;

public sealed record VWorld건물통합정보ImportRequest(
    string SourceRevision,
    string SourceVintage,
    DateTimeOffset ObservedAtUtc,
    string PrivateStorageLocation);

public sealed record VWorld건물통합정보ImportResult(
    int ParsedCount,
    int InsertedCount,
    int ExistingCount,
    int RejectedCount,
    long RawSnapshotId,
    string SourceHashSha256);

/// <summary>
/// VWorld 건물통합정보 ZIP의 DBF 속성을 공공데이터 건축물 원장으로 정규화합니다.
/// SHP 도형 원본은 ZIP 그대로 보존하고 이 단계에서는 업무 사실과 시각 배치를 확정하지 않습니다.
/// </summary>
public sealed class VWorld건물통합정보ImportService
{
    public const string SourceId = "vworld";
    public const string DatasetId = "building-integrated-master";
    private const int BatchSize = 1_000;
    private const int MaximumRecordCount = 2_000_000;
    private const long MaximumDbfBytes = 2L * 1024 * 1024 * 1024;
    private const string RegionRuleRevision = "vworld-building-pnu-legal-region-v1";

    private readonly PublicDataIngestionDbContext _db;

    public VWorld건물통합정보ImportService(PublicDataIngestionDbContext db)
        => _db = db;

    public async Task<VWorld건물통합정보ImportResult> ImportZipAsync(
        Stream zipStream,
        string originalFileName,
        VWorld건물통합정보ImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(zipStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceRevision);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SourceVintage);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PrivateStorageLocation);

        var (sourceBytes, sourceHash) = await ReadAndHashAsync(zipStream, cancellationToken);
        var rawSnapshot = await RegisterRawSnapshotAsync(
            sourceBytes,
            sourceHash,
            originalFileName,
            request,
            cancellationToken);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var archiveStream = new MemoryStream(sourceBytes, writable: false);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
        var entry = archive.Entries.SingleOrDefault(item =>
            item.FullName.EndsWith(".dbf", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException("VWorldBuildingDbfMissing");
        if (entry.Length <= 0 || entry.Length > MaximumDbfBytes)
            throw new InvalidDataException("VWorldBuildingDbfSizeInvalid");

        await using var dbf = entry.Open();
        using var reader = new DbfReader(dbf, Encoding.GetEncoding(949), MaximumRecordCount);
        var parsed = 0;
        var inserted = 0;
        var existing = 0;
        var rejected = 0;
        var batch = new List<건축물대장표제부Record>(BatchSize);
        var regionBatch = new List<건축물행정구역Assignment>(BatchSize);

        foreach (var row in reader.ReadRows())
        {
            cancellationToken.ThrowIfCancellationRequested();
            parsed++;
            var ufid = row.Get("UFID");
            var sigunguObjectId = row.Get("SGG_OID");
            var pnu = row.Get("PNU");
            if (string.IsNullOrWhiteSpace(ufid)
                || pnu.Length < 10
                || !pnu[..10].All(char.IsDigit))
            {
                rejected++;
                continue;
            }

            var buildingId = 공공데이터원장식별자.결정적Guid(
                $"{SourceId}|{DatasetId}|{ufid}|{sigunguObjectId}|{request.SourceRevision}");
            var purposeCode = NullIfEmpty(row.Get("USABILITY"));
            batch.Add(new 건축물대장표제부Record
            {
                Id = buildingId,
                RegisterManagementPk = "vworld:" + ufid + ":" + sigunguObjectId,
                RegisterKindCode = "building-integrated-master",
                RegisterTypeCode = NullIfEmpty(row.Get("GB_CD")),
                SigunguCode = pnu[..5],
                LegalDongCode = pnu[..10],
                LandLot = pnu,
                BuildingName = NullIfEmpty(row.Get("BLD_NM")),
                DongName = NullIfEmpty(row.Get("DONG_NM")),
                MainPurposeCode = purposeCode,
                MainPurposeName = PurposeName(purposeCode),
                StructureCode = NullIfEmpty(row.Get("STRCT_CD")),
                StructureName = StructureName(row.Get("STRCT_CD")),
                BuildingAreaSquareMeters = Decimal(row.Get("ARCHAREA")),
                TotalFloorAreaSquareMeters = Decimal(row.Get("TOTALAREA")),
                SiteAreaSquareMeters = Decimal(row.Get("PLATAREA")),
                OfficialBuildingCoveragePercent = Decimal(row.Get("BC_RAT")),
                OfficialFloorAreaRatioPercent = Decimal(row.Get("VL_RAT")),
                HeightMeters = Decimal(row.Get("HEIGHT")),
                AboveGroundFloorCount = Integer(row.Get("GRND_FLR")),
                UndergroundFloorCount = Integer(row.Get("UGRND_FLR")),
                ApprovalDate = Date(row.Get("USEAPR_DAY")),
                SourceRevision = request.SourceRevision,
                EvidenceSnapshotId = rawSnapshot.Id,
                ObservedAtUtc = request.ObservedAtUtc,
            });
            regionBatch.Add(new 건축물행정구역Assignment
            {
                Id = 공공데이터원장식별자.결정적Guid(
                    $"building-region|{buildingId:N}|{RegionRuleRevision}"),
                BuildingRecordId = buildingId,
                LegalRegionStableId = "region:kr:bjd:" + pnu[..10],
                AdministrativeRegionStableId = null,
                AssignmentMethodCode = "ObservedPnuLegalRegion",
                ConfidenceCode = "ObservedHigh",
                SourceVintage = request.SourceVintage,
                RuleRevision = RegionRuleRevision,
                ValidFromUtc = request.ObservedAtUtc,
            });

            if (batch.Count < BatchSize)
                continue;
            var saved = await SaveBatchAsync(batch, regionBatch, cancellationToken);
            inserted += saved.Inserted;
            existing += saved.Existing;
            batch.Clear();
            regionBatch.Clear();
        }

        if (batch.Count > 0)
        {
            var saved = await SaveBatchAsync(batch, regionBatch, cancellationToken);
            inserted += saved.Inserted;
            existing += saved.Existing;
        }

        return new VWorld건물통합정보ImportResult(
            parsed,
            inserted,
            existing,
            rejected,
            rawSnapshot.Id,
            sourceHash);
    }

    private async Task<(int Inserted, int Existing)> SaveBatchAsync(
        IReadOnlyCollection<건축물대장표제부Record> buildings,
        IReadOnlyCollection<건축물행정구역Assignment> regions,
        CancellationToken cancellationToken)
    {
        var ids = buildings.Select(item => item.Id).ToList();
        var existingIds = await _db.BuildingRegisterTitles
            .Where(item => ids.Contains(item.Id))
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
        var existingSet = existingIds.ToHashSet();
        var additions = buildings.Where(item => !existingSet.Contains(item.Id)).ToArray();
        var additionIds = additions.Select(item => item.Id).ToHashSet();
        _db.BuildingRegisterTitles.AddRange(additions);
        _db.BuildingRegionAssignments.AddRange(
            regions.Where(item => additionIds.Contains(item.BuildingRecordId)));
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return (additions.Length, existingSet.Count);
    }

    private async Task<외부데이터RawSnapshot> RegisterRawSnapshotAsync(
        byte[] sourceBytes,
        string sourceHash,
        string originalFileName,
        VWorld건물통합정보ImportRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _db.RawSnapshots.SingleOrDefaultAsync(item =>
            item.SourceId == SourceId
            && item.DatasetId == DatasetId
            && item.ContentHashSha256 == sourceHash,
            cancellationToken);
        if (existing != null)
        {
            existing.LastSeenAtUtc = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var now = DateTimeOffset.UtcNow;
        var run = new 외부데이터수집Run
        {
            RunKey = $"vworld-building-import:{now:yyyyMMddHHmmssfff}:{sourceHash[..12]}",
            SourceId = SourceId,
            DatasetId = DatasetId,
            StatusCode = 외부데이터수집StatusCodes.Success,
            StartedAtUtc = now,
            CompletedAtUtc = now,
            AttemptCount = 1,
            FetchedCount = 1,
            InsertedCount = 1,
            SourceVersion = request.SourceVintage,
            DataRevision = request.SourceRevision,
        };
        _db.IngestionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var snapshot = new 외부데이터RawSnapshot
        {
            FirstCollectionRunId = run.Id,
            SourceId = SourceId,
            DatasetId = DatasetId,
            SourceVersion = request.SourceVintage,
            CollectedAtUtc = now,
            EvidenceAsOfUtc = request.ObservedAtUtc,
            ContentHashSha256 = sourceHash,
            ContentLength = sourceBytes.LongLength,
            ContentType = "application/zip",
            OriginalFileName = Path.GetFileName(originalFileName),
            StorageContainer = "local-private-public-spatial",
            StorageObjectName = request.PrivateStorageLocation.Replace('\\', '/'),
            StorageLocation = "private-file://" + request.PrivateStorageLocation.Replace('\\', '/'),
            FirstSeenAtUtc = now,
            LastSeenAtUtc = now,
        };
        _db.RawSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    private static async Task<(byte[] Bytes, string Hash)> ReadAndHashAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        return (bytes, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }

    private static string? NullIfEmpty(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Decimal(string value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            && result >= 0m
                ? result
                : null;

    private static int? Integer(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            && result >= 0
                ? result
                : null;

    private static DateOnly? Date(string value)
        => DateOnly.TryParseExact(
            value,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result)
                ? result
                : null;

    private static string? PurposeName(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
            return null;
        return code[..2] switch
        {
            "01" => "단독주택",
            "02" => "공동주택",
            "03" or "04" => "근린생활시설",
            "05" => "문화및집회시설",
            "06" => "종교시설",
            "07" => "판매시설",
            "08" => "운수시설",
            "09" => "의료시설",
            "10" => "교육연구시설",
            "11" => "노유자시설",
            "12" => "수련시설",
            "13" => "운동시설",
            "14" => "업무시설",
            "15" => "숙박시설",
            "16" => "위락시설",
            "17" => "공장",
            "18" => "창고시설",
            "19" => "위험물저장및처리시설",
            "20" => "자동차관련시설",
            "21" => "동물및식물관련시설",
            "22" or "30" => "자원순환관련시설",
            "23" or "32" or "33" => "교정및군사시설",
            "24" => "방송통신시설",
            "25" => "발전시설",
            "26" => "묘지관련시설",
            "27" => "관광휴게시설",
            "29" => "장례시설",
            "31" => "야영장시설",
            _ => "기타시설",
        };
    }

    private static string? StructureName(string code) => code switch
    {
        "10" => "조적구조",
        "11" => "벽돌구조",
        "12" => "블록구조",
        "13" => "석구조",
        "20" => "콘크리트구조",
        "21" => "철근콘크리트구조",
        "22" => "프리케스트콘크리트구조",
        "30" => "철골구조",
        "31" => "일반철골구조",
        "32" => "경량철골구조",
        "33" => "강파이프구조",
        "40" or "42" => "철골철근콘크리트구조",
        "41" => "철골콘크리트구조",
        "50" => "목구조",
        "51" => "일반목구조",
        "52" => "통나무구조",
        "19" or "29" or "39" or "49" or "99" => "기타구조",
        _ => null,
    };

    private sealed class DbfReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly Encoding _encoding;
        private readonly int _recordCount;
        private readonly int _recordLength;
        private readonly IReadOnlyList<DbfField> _fields;

        public DbfReader(Stream stream, Encoding encoding, int maximumRecordCount)
        {
            _reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
            _encoding = encoding;
            var header = _reader.ReadBytes(32);
            if (header.Length != 32)
                throw new InvalidDataException("VWorldBuildingDbfHeaderInvalid");
            _recordCount = BitConverter.ToInt32(header, 4);
            var headerLength = BitConverter.ToUInt16(header, 8);
            _recordLength = BitConverter.ToUInt16(header, 10);
            if (_recordCount < 0 || _recordCount > maximumRecordCount || _recordLength <= 1)
                throw new InvalidDataException("VWorldBuildingDbfShapeInvalid");

            var fields = new List<DbfField>();
            var consumedHeaderBytes = 32;
            while (true)
            {
                var marker = _reader.ReadByte();
                consumedHeaderBytes++;
                if (marker == 0x0D)
                    break;
                var descriptor = new byte[32];
                descriptor[0] = marker;
                var tail = _reader.ReadBytes(31);
                if (tail.Length != 31)
                    throw new InvalidDataException("VWorldBuildingDbfFieldInvalid");
                consumedHeaderBytes += tail.Length;
                Buffer.BlockCopy(tail, 0, descriptor, 1, tail.Length);
                var name = Encoding.ASCII.GetString(descriptor, 0, 11).TrimEnd('\0', ' ');
                fields.Add(new DbfField(name, descriptor[16]));
            }
            _fields = fields;
            if (consumedHeaderBytes > headerLength)
                throw new InvalidDataException("VWorldBuildingDbfHeaderLengthInvalid");
            if (consumedHeaderBytes < headerLength
                && _reader.ReadBytes(headerLength - consumedHeaderBytes).Length
                    != headerLength - consumedHeaderBytes)
                throw new InvalidDataException("VWorldBuildingDbfHeaderTruncated");
        }

        public IEnumerable<DbfRow> ReadRows()
        {
            for (var index = 0; index < _recordCount; index++)
            {
                var record = _reader.ReadBytes(_recordLength);
                if (record.Length != _recordLength)
                    throw new InvalidDataException("VWorldBuildingDbfRecordTruncated");
                if (record[0] == (byte)'*')
                    continue;
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var offset = 1;
                foreach (var field in _fields)
                {
                    values[field.Name] = _encoding.GetString(record, offset, field.Length).Trim();
                    offset += field.Length;
                }
                yield return new DbfRow(values);
            }
        }

        public void Dispose() => _reader.Dispose();
        private sealed record DbfField(string Name, int Length);
    }

    private sealed class DbfRow(IReadOnlyDictionary<string, string> values)
    {
        public string Get(string name) => values.GetValueOrDefault(name) ?? string.Empty;
    }
}
