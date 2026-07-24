using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매체험ServiceTests
{
    [Fact]
    public void 첫라운드는_나만포함하고_아무것도저장하거나실행하지않는다()
    {
        var service = CreateService();
        var scenario = service.시나리오목록()[0];

        var result = service.시뮬레이션(new 공동구매체험요청
        {
            시나리오Id = scenario.시나리오Id,
            내희망수량 = scenario.기본희망수량,
            라운드 = 0
        });

        Assert.True(result.시뮬레이션여부);
        Assert.False(result.서버저장여부);
        Assert.False(result.외부효과발생여부);
        Assert.False(result.완료여부);
        var participant = Assert.Single(result.참여자목록);
        Assert.Equal("나", participant.표시명);
        Assert.False(participant.가상참여자여부);
        Assert.Contains(result.안전경계안내, item => item.Contains("저장하지 않습니다", StringComparison.Ordinal));
        Assert.Contains(result.안전경계안내, item => item.Contains("주문·결제·계약", StringComparison.Ordinal));
    }

    [Fact]
    public void 마지막라운드는_가상이웃을명시하고_기존집단화진행계산으로목표를달성한다()
    {
        var service = CreateService();
        var scenario = service.시나리오목록()[0];

        var result = service.시뮬레이션(new 공동구매체험요청
        {
            세션Id = "practice-session-17",
            시나리오Id = scenario.시나리오Id,
            내희망수량 = scenario.기본희망수량,
            라운드 = 3,
            대화주제코드 = 공동구매체험대화주제코드.요리이야기
        });

        Assert.Equal("practice-session-17", result.세션Id);
        Assert.True(result.완료여부);
        Assert.True(result.진행.모집조건충족여부);
        Assert.Equal(scenario.목표참여자수, result.진행.참여자수);
        Assert.Equal(5, result.참여자목록.Count(item => item.가상참여자여부));
        Assert.All(
            result.참여자목록.Where(item => item.가상참여자여부),
            item => Assert.StartsWith("practice-", item.참여자키, StringComparison.Ordinal));
        Assert.All(
            result.대화목록.Where(item => item.발화자 != "나"),
            item => Assert.True(item.가상대화여부));
        Assert.True(result.연습예상단가 < scenario.연습기준단가);
        Assert.Contains("다시 확인", result.실제수요전환안내, StringComparison.Ordinal);
    }

    [Fact]
    public void 같은세션과라운드는_참여자와연습단가가결정적으로같다()
    {
        var service = CreateService();
        var scenario = service.시나리오목록()[1];
        var request = new 공동구매체험요청
        {
            세션Id = "practice-repeat",
            시나리오Id = scenario.시나리오Id,
            내희망수량 = scenario.기본희망수량,
            라운드 = 2
        };

        var first = service.시뮬레이션(request);
        var second = service.시뮬레이션(request);

        Assert.Equal(first.연습예상단가, second.연습예상단가);
        Assert.Equal(
            first.참여자목록.Select(item => item.참여자키),
            second.참여자목록.Select(item => item.참여자키));
    }

    [Fact]
    public void 알수없는시나리오와잘못된수량을거절한다()
    {
        var service = CreateService();
        var scenario = service.시나리오목록()[0];

        Assert.Throws<InvalidOperationException>(() => service.시뮬레이션(new 공동구매체험요청
        {
            시나리오Id = "missing",
            내희망수량 = 1
        }));
        Assert.Throws<InvalidOperationException>(() => service.시뮬레이션(new 공동구매체험요청
        {
            시나리오Id = scenario.시나리오Id,
            내희망수량 = 0
        }));
    }

    private static 공동구매체험Service CreateService()
        => new(
            new 공동구매주문자집단화Engine(),
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 23, 9, 0, 0, TimeSpan.Zero)));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
