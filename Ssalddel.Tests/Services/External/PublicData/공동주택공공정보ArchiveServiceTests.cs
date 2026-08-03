using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.PublicData;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 공동주택공공정보ArchiveServiceTests
{
    [Fact]
    public async Task 비활성은_외부호출과DB쓰기를하지않는다()
    {
        await using var db = CreateDb();
        var complex = new FakeComplexLookup();
        var fee = new FakeFeeLookup();
        var service = CreateService(db, complex, fee, enabled: false);

        var result = await service.수집Async("apartment:apt-1", "APT-1", "선택단지", "202607");

        Assert.Equal(공동주택공공정보수집상태Codes.비활성, result.StatusCode);
        Assert.Equal(0, complex.CallCount);
        Assert.Equal(0, fee.CallCount);
        Assert.Empty(db.공동주택공공정보수집Runs);
        Assert.Empty(db.공동주택공공정보Snapshots);
    }

    [Fact]
    public async Task 선택단지한곳의_월별Snapshot을저장한다()
    {
        await using var db = CreateDb();
        var complex = new FakeComplexLookup();
        var fee = new FakeFeeLookup();
        var service = CreateService(db, complex, fee);

        var result = await service.수집Async("apartment:apt-1", "APT-1", "선택단지", "2026-07");

        Assert.Equal(공동주택공공정보수집상태Codes.완료, result.StatusCode);
        Assert.NotNull(result.SnapshotId);
        Assert.Equal(64, result.ContentSha256?.Length);
        var run = Assert.Single(db.공동주택공공정보수집Runs);
        Assert.Equal(5, run.RequestCount);
        var snapshot = Assert.Single(db.공동주택공공정보Snapshots);
        Assert.Equal("kapt:APT-1", snapshot.SpatialKey);
        Assert.Equal("202607", snapshot.TargetMonth);
        Assert.Contains("APT-1", snapshot.NormalizedJson, StringComparison.Ordinal);
        Assert.Equal(1, complex.CallCount);
        Assert.Equal(1, fee.CallCount);
    }

    [Fact]
    public async Task 같은Scope와월은_기존Run을재사용하고재호출하지않는다()
    {
        await using var db = CreateDb();
        var complex = new FakeComplexLookup();
        var fee = new FakeFeeLookup();
        var service = CreateService(db, complex, fee);

        var first = await service.수집Async("apartment:apt-1", "APT-1", "선택단지", "202607");
        var second = await service.수집Async("apartment:apt-1", "APT-1", "선택단지", "202607");

        Assert.Equal(first.RunId, second.RunId);
        Assert.True(second.ReusedExistingSnapshot);
        Assert.Equal(1, complex.CallCount);
        Assert.Equal(1, fee.CallCount);
        Assert.Single(db.공동주택공공정보수집Runs);
    }

    [Fact]
    public async Task 같은내용의다른선택Scope는_월별Snapshot을upsert한다()
    {
        await using var db = CreateDb();
        var complex = new FakeComplexLookup();
        var fee = new FakeFeeLookup();
        var service = CreateService(db, complex, fee);

        var first = await service.수집Async("organization:one", "APT-1", "선택단지", "202607");
        var second = await service.수집Async("organization:two", "APT-1", "선택단지", "202607");

        Assert.NotEqual(first.RunId, second.RunId);
        Assert.Equal(first.SnapshotId, second.SnapshotId);
        Assert.True(second.ReusedExistingSnapshot);
        Assert.Equal(2, db.공동주택공공정보수집Runs.Count());
        Assert.Single(db.공동주택공공정보Snapshots);
    }

    [Fact]
    public async Task 조회실패는_Run에오류를기록한다()
    {
        await using var db = CreateDb();
        var complex = new FakeComplexLookup(success: false);
        var fee = new FakeFeeLookup();
        var service = CreateService(db, complex, fee);

        var result = await service.수집Async("apartment:apt-1", "APT-1", "선택단지", "202607");

        Assert.Equal(공동주택공공정보수집상태Codes.실패, result.StatusCode);
        var run = Assert.Single(db.공동주택공공정보수집Runs);
        Assert.Equal(공동주택공공정보수집상태Codes.실패, run.StatusCode);
        Assert.Equal("fixture failure", run.ErrorMessage);
        Assert.Equal(1, run.RequestCount);
        Assert.Empty(db.공동주택공공정보Snapshots);
    }

    private static 공동주택공공정보ArchiveService CreateService(
        SsalddelContext db,
        IApartmentComplexLookupService complex,
        IApartmentManagementFeeLookupService fee,
        bool enabled = true)
        => new(
            db,
            complex,
            fee,
            Options.Create(new PublicDataOptions
            {
                ApartmentManagementFee = new ApartmentManagementFeeOptions
                {
                    Archive = new ApartmentPublicDataArchiveOptions
                    {
                        Enabled = enabled,
                        MaxComplexesPerRun = 1,
                        MaxRequestsPerComplex = 5
                    }
                }
            }),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero)));

    private static SsalddelContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new PassThroughEncryption());
    }

    private sealed class FakeComplexLookup(bool success = true) : IApartmentComplexLookupService
    {
        public int CallCount { get; private set; }

        public Task<PublicDataLookupResponse<ApartmentComplexItem>> SearchAsync(
            ApartmentComplexSearchRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<PublicDataLookupResponse<ApartmentComplexBasicItem>> GetBasicInfoAsync(
            ApartmentComplexBasicRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new PublicDataLookupResponse<ApartmentComplexBasicItem>
            {
                Success = success,
                ErrorMessage = success ? null : "fixture failure",
                Items = success
                    ?
                    [
                        new ApartmentComplexBasicItem
                        {
                            ComplexCode = request.ComplexCode,
                            ComplexName = "선택단지",
                            HouseholdCount = 120,
                            BuildingCount = 3,
                            RoadAddress = "공개 도로명 주소"
                        }
                    ]
                    : []
            });
        }
    }

    private sealed class FakeFeeLookup : IApartmentManagementFeeLookupService
    {
        public int CallCount { get; private set; }

        public Task<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>> GetSnapshotAsync(
            ApartmentManagementFeeSnapshotRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(new PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>
            {
                Success = true,
                Items =
                [
                    new ApartmentManagementFeeSnapshotItem
                    {
                        ComplexCode = request.ComplexCode,
                        Month = request.Month,
                        HouseholdCount = 120,
                        PublicManagementFeeAmount = 1_000_000m,
                        IndividualUsageFeeAmount = 2_000_000m,
                        LongTermRepairReserveMonthlyAmount = 300_000m,
                        EstimatedTotalMonthlyFeeAmount = 3_300_000m,
                        EstimatedFeePerHousehold = 27_500m,
                        LineItems =
                        [
                            new ApartmentManagementFeeLineItem
                            {
                                Category = "PublicManagementFee",
                                Code = "gnrlMngCost",
                                DisplayName = "일반관리비",
                                Amount = 1_000_000m
                            }
                        ]
                    }
                ]
            });
        }

        public Task<ApartmentGroupCommerceOffsetSimulationResult> SimulateGroupCommerceOffsetAsync(
            ApartmentGroupCommerceOffsetSimulationRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class PassThroughEncryption : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
