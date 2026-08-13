using System.Text;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData.Korea;
using Ssalddel.Infrastructure.Persistence.PublicData;
using Ssalddel.Startup;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 지방행정인허가사업장Tests
{
    private const string SourceHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void 공개Source는_상호와주소를제공하지만_개인정보전수명부가아님을명시한다()
    {
        var source = Assert.Single(new 지방행정인허가사업장SourceRegistration().GetDefinitions());

        Assert.Equal("mois-localdata", source.SourceId);
        Assert.False(source.DefaultCollectionEnabled);
        Assert.Contains("대표자명", source.UsageLimitations, StringComparison.Ordinal);
        Assert.Contains("전화번호", source.UsageLimitations, StringComparison.Ordinal);
        Assert.Contains("전수명부가 아닙니다", source.UsageLimitations, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CsvImport는_상호업종상태주소를저장하고_대표자와전화번호를투영하지않는다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        var service = new 지방행정인허가사업장ImportService(db);
        await using var csv = Csv(
            "개방서비스아이디,개방서비스명,관리번호,사업장명,업태구분명,영업상태명,상세영업상태명,도로명전체주소,소재지전체주소,인허가일자,좌표정보(X),좌표정보(Y),대표자명,소재지전화\n" +
            "07_22_09_P,식품운반업,PC-001,진부물류,일반운송,영업,정상,\"강원특별자치도 평창군 진부면 진부중앙로 45, 1층 101호 (하진부리)\",강원특별자치도 평창군 진부면 하진부리 1,20250102,200000.1,500000.2,홍길동,033-000-0000\n" +
            "07_22_09_P,식품운반업,,누락관리번호,,,,,,,,,,\n");

        var result = await service.ImportCsvAsync(csv, Request());
        var record = await db.공개인허가사업장Records.SingleAsync();

        Assert.Equal(new 지방행정인허가사업장ImportResult(2, 1, 0, 1), result);
        Assert.Equal("진부물류", record.BusinessName);
        Assert.Equal("일반운송", record.BusinessTypeName);
        Assert.Equal("영업", record.BusinessStatusName);
        Assert.Equal("강원특별자치도 평창군 진부면 진부중앙로 45", record.NormalizedRoadAddressKey);
        var storedProperties = typeof(공개인허가사업장Record).GetProperties().Select(item => item.Name).ToArray();
        Assert.DoesNotContain(storedProperties, name => name.Contains("Representative", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(storedProperties, name => name.Contains("Phone", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(storedProperties, name => name.Contains("RegistrationNumber", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task 같은원본을다시읽으면_사업장행을중복하지않는다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        var service = new 지방행정인허가사업장ImportService(db);
        var content = "개방서비스아이디,관리번호,사업장명,도로명전체주소\n" +
                      "01_01,PC-001,대관령상점,강원특별자치도 평창군 대관령면 횡계길 10\n";

        await using var firstCsv = Csv(content);
        var first = await service.ImportCsvAsync(firstCsv, Request());
        await using var secondCsv = Csv(content);
        var second = await service.ImportCsvAsync(secondCsv, Request());

        Assert.Equal(1, first.InsertedCount);
        Assert.Equal(0, second.InsertedCount);
        Assert.Equal(1, second.ExistingCount);
        Assert.Equal(1, await db.공개인허가사업장Records.CountAsync());
    }

    [Fact]
    public async Task 정확한정규화주소의건물이하나일때만_사업장을자동연결한다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        var building = Building("building-1", "강원특별자치도 평창군 진부면 진부중앙로 45");
        db.BuildingRegisterTitles.Add(building);
        db.공개인허가사업장Records.Add(Business(
            "business-1",
            "진부물류",
            "강원특별자치도 평창군 진부면 진부중앙로 45, 1층 101호",
            "영업"));
        await db.SaveChangesAsync();
        var service = new 공개사업장건축물연결Service(db);

        var result = await service.정확한도로명주소로연결Async("localdata-v1", "building-v1");
        await service.건물별집계생성Async("localdata-v1");
        var assignment = await db.공개사업장건축물Assignments.SingleAsync();
        var aggregate = await db.건축물공개사업장Aggregates.SingleAsync();

        Assert.Equal(1, result.연결수);
        Assert.Equal(building.Id, assignment.BuildingRecordId);
        Assert.Equal(공개사업장연결방법Codes.정확한정규화도로명주소, assignment.AssignmentMethodCode);
        Assert.Equal(1, aggregate.TotalBusinessCount);
        Assert.Equal(1, aggregate.OpenBusinessCount);
    }

    [Fact]
    public async Task 같은주소의건물이여럿이면_입주를추측하지않고복수후보로남긴다()
    {
        await using var db = CreateDb();
        await db.Database.EnsureCreatedAsync();
        const string address = "강원특별자치도 평창군 평창읍 중앙로 10";
        db.BuildingRegisterTitles.AddRange(
            Building("building-1", address),
            Building("building-2", address));
        db.공개인허가사업장Records.Add(Business("business-1", "평창상점", address, "영업"));
        await db.SaveChangesAsync();
        var service = new 공개사업장건축물연결Service(db);

        var result = await service.정확한도로명주소로연결Async("localdata-v1", "building-v1");
        var assignment = await db.공개사업장건축물Assignments.SingleAsync();

        Assert.Equal(1, result.복수후보수);
        Assert.Equal(2, assignment.CandidateBuildingCount);
        Assert.Null(assignment.BuildingRecordId);
        Assert.Equal(공개사업장연결상태Codes.복수후보, assignment.AssignmentStatusCode);
    }

    [Theory]
    [InlineData("영업", "정상", 공개사업장영업상태Codes.영업)]
    [InlineData("영업", "영업정지", 공개사업장영업상태Codes.휴업)]
    [InlineData("폐업", null, 공개사업장영업상태Codes.폐업)]
    [InlineData(null, null, 공개사업장영업상태Codes.미확인)]
    public void 영업상태는_상호배타적인집계상태로분류한다(
        string? status,
        string? detailedStatus,
        string expected)
    {
        Assert.Equal(expected, 공개사업장영업상태Engine.분류(status, detailedStatus));
    }

    [Fact]
    public void 가져오기명령인자는_파일과원본개정번호를하나의계약으로해석한다()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            var parsed = 대한민국공간공공데이터CommandLine.ParseImportArguments([
                대한민국공간공공데이터CommandLine.공개사업장가져오기Command,
                $"--file={filePath}",
                "--source-revision=localdata-20260813",
                "--building-source-revision=building-202608",
            ]);

            Assert.Equal(Path.GetFullPath(filePath), parsed.FilePath);
            Assert.Equal("localdata-20260813", parsed.SourceRevision);
            Assert.Equal("utf-8", parsed.EncodingName);
            Assert.Equal("building-202608", parsed.BuildingSourceRevision);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void 가져오기명령은_원본개정번호가없으면실행전에거부한다()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                대한민국공간공공데이터CommandLine.ParseImportArguments([
                    대한민국공간공공데이터CommandLine.공개사업장가져오기Command,
                    $"--file={filePath}",
                ]));

            Assert.Contains("--source-revision", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static PublicDataIngestionDbContext CreateDb() => new(
        new DbContextOptionsBuilder<PublicDataIngestionDbContext>()
            .UseInMemoryDatabase($"localdata-business-{Guid.NewGuid():N}")
            .Options);

    private static MemoryStream Csv(string value) => new(Encoding.UTF8.GetBytes(value));

    private static 지방행정인허가사업장ImportRequest Request() => new(
        "localdata-v1",
        SourceHash,
        DateTimeOffset.Parse("2026-08-13T00:00:00Z"));

    private static 건축물대장표제부Record Building(string key, string address) => new()
    {
        Id = Guid.NewGuid(),
        RegisterManagementPk = key,
        RegisterKindCode = "title",
        SigunguCode = "51760",
        LegalDongCode = "5176036000",
        RoadAddress = address,
        SourceRevision = "building-v1",
        EvidenceSnapshotId = 1,
        ObservedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
    };

    private static 공개인허가사업장Record Business(
        string key,
        string name,
        string address,
        string status) => new()
    {
        Id = Guid.NewGuid(),
        SourceId = 지방행정인허가사업장ImportService.SourceId,
        SourceDatasetId = 지방행정인허가사업장ImportService.DatasetId,
        OpenServiceId = "test-service",
        ManagementNumber = key,
        BusinessName = name,
        BusinessStatusName = status,
        RoadAddress = address,
        NormalizedRoadAddressKey = 공개사업장주소정규화Engine.NormalizeRoadAddress(address),
        SourceRevision = "localdata-v1",
        SourceHashSha256 = SourceHash,
        ObservedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
    };
}
