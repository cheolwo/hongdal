using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Community;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Tests.Application.WorkflowRules;

public sealed class 업무흐름규칙ParityTests
{
    [Fact]
    public void Catalog는_네_업무와_운영효과_제외경계를_보존한다()
    {
        var rules = 업무흐름규칙Catalog.전체조회();

        Assert.Equal(
            [업무흐름코드.음식배달, 업무흐름코드.화물운송, 업무흐름코드.같이주문, 업무흐름코드.개별주문],
            rules.Select(x => x.업무흐름코드).OrderBy(x => x, StringComparer.Ordinal));
        Assert.All(rules, rule =>
        {
            Assert.NotEmpty(rule.SourceCapabilityKey);
            Assert.NotEmpty(rule.SourceContractRevision);
            Assert.Equal("workflow-rules.v1", rule.RuleRevision);
            Assert.NotEmpty(rule.SourceStableIds);
            Assert.NotEmpty(rule.Simulation제외운영효과코드목록);
        });
    }

    [Fact]
    public void 음식배달_상태코드는_현재_운영Contract와_일치한다()
    {
        var rule = 업무흐름규칙Catalog.조회(업무흐름코드.음식배달);

        Assert.Equal(음식주문상태코드.전체, rule.상태코드목록);
        Assert.True(업무상태전이Policy.판정(
            업무흐름코드.음식배달,
            음식주문상태코드.조리중,
            음식주문상태코드.픽업대기).허용여부);
        Assert.False(업무상태전이Policy.판정(
            업무흐름코드.음식배달,
            음식주문상태코드.수령확인,
            음식주문상태코드.조리중).허용여부);
    }

    [Fact]
    public void 화물운송_상태전이는_현재_운영Policy와_일치한다()
    {
        var rule = 업무흐름규칙Catalog.조회(업무흐름코드.화물운송);

        foreach (var current in rule.상태코드목록)
        {
            foreach (var target in rule.상태코드목록)
            {
                var expected = 기사운송상태전이Policy.가능한가(current, target);
                var actual = 업무상태전이Policy.판정(
                    업무흐름코드.화물운송,
                    current,
                    target).허용여부;
                Assert.Equal(expected, actual);
            }
        }
    }

    [Fact]
    public void 같이주문_상태코드는_현재_집단화Contract와_일치한다()
    {
        var rule = 업무흐름규칙Catalog.조회(업무흐름코드.같이주문);

        Assert.Contains(공동구매자동집단상태코드.수요수집중, rule.상태코드목록);
        Assert.Contains(공동구매자동집단상태코드.확정대기, rule.상태코드목록);
        Assert.Contains(공동구매자동집단상태코드.확정, rule.상태코드목록);
        Assert.Contains(공동구매자동집단상태코드.모집종료목표미달, rule.상태코드목록);
    }

    [Theory]
    [InlineData(1, 20, 3, 60)]
    [InlineData(2, 30, 3, 60)]
    [InlineData(3, 60, 3, 60)]
    public void 같이주문_목표상태판정은_예약결제없는_운영계획과_일치한다(
        int 참여자수,
        int 총희망수량,
        int 목표참여자수,
        int 목표수량)
    {
        var expected = 공동구매자동집단화계획기.상태제안(
            참여자수,
            0,
            총희망수량,
            목표참여자수,
            목표수량);

        var actual = 같이주문상태Policy.판정(new 같이주문상태판정요청
        {
            참여자수 = 참여자수,
            총희망수량 = 총희망수량,
            목표참여자수 = 목표참여자수,
            목표수량 = 목표수량,
        });

        Assert.Equal(expected, actual.제안상태코드);
        Assert.Equal(
            expected == 같이주문상태코드.확정대기
                ? 같이주문상태코드.확정
                : 같이주문상태코드.모집종료목표미달,
            actual.모집종료결과상태코드);
    }

    [Fact]
    public void 개별주문_상태코드는_현재_커뮤니티원장Contract와_일치한다()
    {
        var rule = 업무흐름규칙Catalog.조회(업무흐름코드.개별주문);

        Assert.Equal(
            [커뮤니티원장상태.초안, 커뮤니티원장상태.진행중, 커뮤니티원장상태.완료],
            rule.상태코드목록);
    }

    [Fact]
    public void 같은상태_재시도는_멱등으로_허용한다()
    {
        var result = 업무상태전이Policy.판정(
            업무흐름코드.개별주문,
            개별주문상태코드.진행중,
            개별주문상태코드.진행중);

        Assert.True(result.허용여부);
        Assert.True(result.멱등재시도여부);
    }

    [Fact]
    public void 수량은_결과와_명시적손실의_합으로_보존한다()
    {
        var conserved = 업무수량보존Policy.판정(new 업무수량보존요청
        {
            입력수량 = 300m,
            결과수량 = 288m,
            손실수량 = 12m,
            단위코드 = "kg",
        });
        var broken = 업무수량보존Policy.판정(new 업무수량보존요청
        {
            입력수량 = 300m,
            결과수량 = 280m,
            손실수량 = 12m,
            단위코드 = "kg",
        });

        Assert.True(conserved.보존여부);
        Assert.Empty(conserved.차단사유코드목록);
        Assert.False(broken.보존여부);
        Assert.Equal(8m, broken.차이수량);
        Assert.Equal([업무규칙차단사유코드.수량불일치], broken.차단사유코드목록);
    }
}
