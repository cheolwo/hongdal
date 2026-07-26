using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Driver.Settlement;
using Ssalddel.Contracts.Driver.Settlement;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.기사;

namespace Ssalddel.Tests.Application.Driver.Settlement;

public sealed class 기사정산계좌UseCaseTests
{
    [Fact]
    public async Task 미등록_조회는_본인기사Id와_HasAccount_false를_반환한다()
    {
        await using var db = CreateContext();
        await SeedDriverAsync(db, "driver-a");
        var useCase = new 기사정산계좌UseCase(db);

        var result = await useCase.조회Async("driver-a");

        Assert.True(result.IsSuccess);
        Assert.Equal("driver-a", result.Value.DriverId);
        Assert.False(result.Value.HasAccount);
        Assert.Empty(result.Value.MaskedAccountNumber);
    }

    [Fact]
    public async Task 저장은_동의를_요구하고_응답에는_마스킹된_번호만_포함한다()
    {
        await using var db = CreateContext();
        await SeedDriverAsync(db, "driver-a");
        var useCase = new 기사정산계좌UseCase(db);

        var denied = await useCase.저장Async(
            "driver-a",
            Request(consent: false));
        var saved = await useCase.저장Async(
            "driver-a",
            Request(consent: true));

        Assert.True(denied.IsFailed);
        Assert.Contains(denied.Errors, x => x.Message.Contains("동의", StringComparison.Ordinal));
        Assert.True(saved.IsSuccess);
        Assert.True(saved.Value.HasAccount);
        Assert.Equal("****8901", saved.Value.MaskedAccountNumber);
        Assert.DoesNotContain("123-45-678901", saved.Value.MaskedAccountNumber, StringComparison.Ordinal);
        Assert.Equal(기사정산계좌확인상태.미확인, saved.Value.VerificationStatus);

        var stored = await db.Set<기사정산계좌>().SingleAsync();
        Assert.Equal("driver-a", stored.기사Id);
        Assert.Equal("123-45-678901", stored.계좌번호);
        Assert.Single(db.사용자행위로그);
        Assert.DoesNotContain("123-45-678901", db.사용자행위로그.Single().MetadataJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 다른_기사는_자기_계좌_조회에서_상대_계좌를_볼_수_없다()
    {
        await using var db = CreateContext();
        await SeedDriverAsync(db, "driver-a");
        await SeedDriverAsync(db, "driver-b");
        var useCase = new 기사정산계좌UseCase(db);
        await useCase.저장Async("driver-a", Request(consent: true));

        var otherDriver = await useCase.조회Async("driver-b");

        Assert.True(otherDriver.IsSuccess);
        Assert.False(otherDriver.Value.HasAccount);
        Assert.Empty(otherDriver.Value.BankName);
        Assert.Empty(otherDriver.Value.MaskedAccountNumber);
    }

    [Fact]
    public async Task 변경은_기존_행을_갱신하고_확인상태를_미확인으로_되돌린다()
    {
        await using var db = CreateContext();
        await SeedDriverAsync(db, "driver-a");
        var useCase = new 기사정산계좌UseCase(db);
        await useCase.저장Async("driver-a", Request(consent: true));
        var account = await db.Set<기사정산계좌>().SingleAsync();
        var originalId = account.Id;
        account.확인상태 = 기사정산계좌확인상태.확인완료;
        await db.SaveChangesAsync();

        var updated = await useCase.저장Async(
            "driver-a",
            new 기사정산계좌수정요청
            {
                CountryCode = "us",
                BankName = "Example Bank",
                AccountHolderName = "Driver A",
                AccountNumber = "US-ACCT-7788",
                개인정보저장동의 = true
            });

        Assert.True(updated.IsSuccess);
        Assert.Equal("US", updated.Value.CountryCode);
        Assert.Equal("****7788", updated.Value.MaskedAccountNumber);
        Assert.Equal(기사정산계좌확인상태.미확인, updated.Value.VerificationStatus);
        var stored = await db.Set<기사정산계좌>().SingleAsync();
        Assert.Equal(originalId, stored.Id);
        Assert.Equal("US-ACCT-7788", stored.계좌번호);
    }

    [Fact]
    public async Task 삭제는_본인_계좌를_제거하고_반복_호출에도_성공한다()
    {
        await using var db = CreateContext();
        await SeedDriverAsync(db, "driver-a");
        var useCase = new 기사정산계좌UseCase(db);
        await useCase.저장Async("driver-a", Request(consent: true));

        var deleted = await useCase.삭제Async("driver-a");
        var deletedAgain = await useCase.삭제Async("driver-a");

        Assert.True(deleted.IsSuccess);
        Assert.True(deletedAgain.IsSuccess);
        Assert.Empty(await db.Set<기사정산계좌>().ToListAsync());
        Assert.Contains(db.사용자행위로그, x => x.ActionType == "Delete");
    }

    private static 기사정산계좌수정요청 Request(bool consent)
        => new()
        {
            CountryCode = "KR",
            BankName = "국민은행",
            AccountHolderName = "홍길동",
            AccountNumber = "123-45-678901",
            개인정보저장동의 = consent
        };

    private static async Task SeedDriverAsync(SsalddelContext db, string driverId)
    {
        db.용달기사.Add(new 용달기사
        {
            기사Id = driverId,
            기사명 = driverId,
            연락처 = "010-0000-0000",
            차량 = "1톤 카고",
            주_활동지역 = "서울"
        });
        await db.SaveChangesAsync();
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value is null ? null : $"protected:{value}";

        public string? Unprotect(string? value)
            => value?.StartsWith("protected:", StringComparison.Ordinal) == true
                ? value["protected:".Length..]
                : value;
    }
}
