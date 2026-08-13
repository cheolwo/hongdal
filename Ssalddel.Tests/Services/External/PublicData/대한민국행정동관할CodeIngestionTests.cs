using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 대한민국행정동관할CodeIngestionTests
{
    [Fact]
    public void 출처등록은_행정동과법정동경계를섞지않는다()
    {
        var source = Assert.Single(
            new 대한민국행정동관할CodeSourceRegistration().GetDefinitions());

        Assert.Equal("mois-resident-registration-codes", source.SourceId);
        Assert.Equal("korea-administrative-legal-jurisdictions", source.DatasetId);
        Assert.False(source.RequiresCredential);
        Assert.False(source.DefaultCollectionEnabled);
        Assert.Contains("경계", source.UsageLimitations, StringComparison.Ordinal);
        Assert.Contains("건물", source.UsageLimitations, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 정규화는_행정기관과관할법정동관계를_서로다른Record로보존한다()
    {
        var raw = Raw();
        var normalizer = new 대한민국행정동관할CodeNormalizer(
            Options.Create(new 대한민국행정동관할CodeOptions()));
        var archive = CreateArchive(
            [
                AdministrativeLine("5100000000", "강원특별자치도", "", "", "20230611"),
                AdministrativeLine("5176038000", "강원특별자치도", "평창군", "대관령면", "20230611"),
            ],
            [
                JurisdictionLine("5100000000", "강원특별자치도", "", "",
                    "5100000000", "강원특별자치도", "20230611"),
                JurisdictionLine("5176038000", "강원특별자치도", "평창군", "대관령면",
                    "5176038024", "횡계리", "20230611"),
            ]);

        var result = await normalizer.NormalizeAsync(
            Source(), raw, new InMemoryRawStorage(archive));

        Assert.Equal(4, result.Records.Count);
        Assert.Equal(2, result.Records.Count(item =>
            item.MetricCode == 대한민국행정동관할CodeDataset.AdministrativeMetricCode));
        Assert.Equal(2, result.Records.Count(item =>
            item.MetricCode == 대한민국행정동관할CodeDataset.JurisdictionMetricCode));
        var daegwallyeong = result.Records.Single(item =>
            item.StableId == "region:kr:hjd:5176038000");
        Assert.Equal("강원특별자치도 평창군 대관령면", daegwallyeong.TextValue);
        Assert.Contains("parent=region:kr:hjd:5176000000", daegwallyeong.DimensionKey,
            StringComparison.Ordinal);
        var relation = result.Records.Single(item =>
            item.StableId == "region:kr:hjd-bjd:5176038000-5176038024");
        Assert.Equal("region:kr:hjd:5176038000", relation.RegionStableId);
        Assert.Equal("횡계리", relation.TextValue);
        Assert.Contains("legal=region:kr:bjd:5176038024", relation.DimensionKey,
            StringComparison.Ordinal);
        Assert.Contains("assignment=official-jurisdiction-crosswalk", relation.DimensionKey,
            StringComparison.Ordinal);
        ExternalDataNormalizationValidator.Validate(Source(), raw, result);
    }

    [Fact]
    public async Task 정규화RecordKey는_재수집에도_행정기관과관할관계별로유지된다()
    {
        var archive = CreateArchive(
            [AdministrativeLine("1100000000", "서울특별시", "", "", "19880423")],
            [JurisdictionLine("1100000000", "서울특별시", "", "",
                "1100000000", "서울특별시", "19880423")]);
        var normalizer = new 대한민국행정동관할CodeNormalizer(
            Options.Create(new 대한민국행정동관할CodeOptions()));

        var first = await normalizer.NormalizeAsync(
            Source(), Raw(1, "2026-03-01T00:00:00Z"), new InMemoryRawStorage(archive));
        var second = await normalizer.NormalizeAsync(
            Source(), Raw(2, "2026-08-13T00:00:00Z"), new InMemoryRawStorage(archive));

        Assert.Equal(
            first.Records.Select(item => item.RecordKey).Order().ToArray(),
            second.Records.Select(item => item.RecordKey).Order().ToArray());
    }

    private static ExternalDataSourceDefinition Source()
        => Assert.Single(new 대한민국행정동관할CodeSourceRegistration().GetDefinitions());

    private static 외부데이터RawSnapshot Raw(
        long id = 17,
        string collectedAt = "2026-08-13T00:00:00Z") => new()
    {
        Id = id,
        SourceId = 대한민국행정동관할CodeDataset.SourceId,
        DatasetId = 대한민국행정동관할CodeDataset.DatasetId,
        SourceVersion = "mois-jscode:20260301:retrieved:2026-08-13",
        ContentHashSha256 = new string('a', 64),
        CollectedAtUtc = DateTimeOffset.Parse(collectedAt),
        EvidenceAsOfUtc = DateTimeOffset.Parse("2026-03-01T00:00:00Z"),
    };

    private static byte[] CreateArchive(
        IReadOnlyCollection<string> administrativeLines,
        IReadOnlyCollection<string> jurisdictionLines)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(
                archive,
                "jscode20260301/KIKcd_H.20260301",
                "행정동코드 시도명                         시군구명                       읍면동명                       생성일자 말소일자\n"
                + string.Join('\n', administrativeLines));
            WriteEntry(
                archive,
                "jscode20260301/KIKmix.20260301",
                "행정동코드 시도명                         시군구명                       읍면동명                       법정동코드 동리명                         생성일자 말소일자\n"
                + string.Join('\n', jurisdictionLines));
        }
        return output.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string name, string text)
    {
        var entry = archive.CreateEntry(name);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.GetEncoding(949));
        writer.Write(text);
    }

    private static string AdministrativeLine(
        string code, string province, string cityCounty, string administrative,
        string created, string abolished = "")
        => $"{code} {PadCp949(province, 30)} {PadCp949(cityCounty, 30)} {PadCp949(administrative, 30)} {created} {abolished}";

    private static string JurisdictionLine(
        string administrativeCode, string province, string cityCounty, string administrative,
        string legalCode, string legalName, string created, string abolished = "")
        => $"{administrativeCode} {PadCp949(province, 30)} {PadCp949(cityCounty, 30)} {PadCp949(administrative, 30)} {legalCode} {PadCp949(legalName, 30)} {created} {abolished}";

    private static string PadCp949(string value, int byteWidth)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var byteCount = Encoding.GetEncoding(949).GetByteCount(value);
        if (byteCount > byteWidth) throw new ArgumentOutOfRangeException(nameof(value));
        return value + new string(' ', byteWidth - byteCount);
    }

    private sealed class InMemoryRawStorage(byte[] bytes) : IExternalDataRawStorage
    {
        public Task<ExternalDataRawStorageResult> StoreAsync(
            ExternalDataSourceDefinition source,
            ExternalDataCollectedPayload payload,
            DateTimeOffset collectedAtUtc,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream> OpenReadAsync(
            외부데이터RawSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
    }
}
