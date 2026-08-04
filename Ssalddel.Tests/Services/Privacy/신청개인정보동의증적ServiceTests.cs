using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Privacy;

namespace Ssalddel.Tests.Services.Privacy;

public sealed class 신청개인정보동의증적ServiceTests
{
    private const string UserId = "map-applicant-1";

    [Fact]
    public async Task 현재문안의두필수확인을_계정업무출처시각Hash와함께기록한다()
    {
        var service = CreateService();
        var request = CreateRequest(신청개인정보업무Codes.운송대행);

        var evidence = await service.동의기록Async(request, UserId);

        Assert.Equal(request.증적Id, evidence.증적Id);
        Assert.Equal(신청개인정보업무Codes.운송대행, evidence.업무Code);
        Assert.Equal(신청개인정보출처Codes.커뮤니티지도, evidence.출처Code);
        Assert.Equal(신청개인정보동의정책.현재버전, evidence.동의문버전);
        Assert.Equal(신청개인정보동의상태Codes.유효, evidence.상태Code);
        Assert.Equal(64, evidence.동의문Hash.Length);
        Assert.NotEqual(default, evidence.동의일시Utc);
        Assert.Null(evidence.철회일시Utc);

        await service.유효한동의요구Async(
            evidence.증적Id,
            신청개인정보업무Codes.운송대행,
            신청개인정보출처Codes.커뮤니티지도,
            UserId);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task 필수확인하나라도없으면_증적을기록하지않는다(bool collectionUse, bool age)
    {
        var service = CreateService();
        var request = CreateRequest(신청개인정보업무Codes.개별주문);
        request.수집이용동의 = collectionUse;
        request.연령요건확인 = age;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.동의기록Async(request, UserId));
        Assert.Null(await service.내증적조회Async(request.증적Id, UserId));
    }

    [Fact]
    public async Task 증적은_다른계정이나업무에서재사용할수없다()
    {
        var service = CreateService();
        var evidence = await service.동의기록Async(
            CreateRequest(신청개인정보업무Codes.물류대행),
            UserId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.유효한동의요구Async(
            evidence.증적Id,
            신청개인정보업무Codes.운송대행,
            신청개인정보출처Codes.커뮤니티지도,
            UserId));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.유효한동의요구Async(
            evidence.증적Id,
            신청개인정보업무Codes.물류대행,
            신청개인정보출처Codes.커뮤니티지도,
            "other-user"));
    }

    [Fact]
    public async Task 철회하면_시각과상태를남기고_이후신청에사용할수없다()
    {
        var service = CreateService();
        var evidence = await service.동의기록Async(
            CreateRequest(신청개인정보업무Codes.개별주문),
            UserId);

        var withdrawn = await service.철회Async(
            evidence.증적Id,
            new 신청개인정보동의철회Request { 철회사유 = "신청 중단" },
            UserId);

        Assert.Equal(신청개인정보동의상태Codes.철회, withdrawn.상태Code);
        Assert.NotNull(withdrawn.철회일시Utc);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.유효한동의요구Async(
            evidence.증적Id,
            신청개인정보업무Codes.개별주문,
            신청개인정보출처Codes.커뮤니티지도,
            UserId));
    }

    [Fact]
    public async Task 지도외기존신청은_새동의증적요구로막지않는다()
    {
        var service = CreateService();

        await service.유효한동의요구Async(
            null,
            신청개인정보업무Codes.운송대행,
            string.Empty,
            string.Empty);
    }

    private static 신청개인정보동의증적Service CreateService()
        => new(new InMemory신청개인정보동의증적Store());

    private static 신청개인정보동의기록Request CreateRequest(string workCode)
        => new()
        {
            증적Id = Guid.NewGuid(),
            업무Code = workCode,
            출처Code = 신청개인정보출처Codes.커뮤니티지도,
            동의문버전 = 신청개인정보동의정책.현재버전,
            수집이용동의 = true,
            연령요건확인 = true
        };
}
