using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 공동구매자동집단화계획기Tests
{
    [Fact]
    public void 자동집단키생성_같은상품과배송권이면_같은키를반환한다()
    {
        var first = 공동구매자동집단화계획기.자동집단키생성("Pork Belly", "APT-101", "Frozen", "LCL");
        var second = 공동구매자동집단화계획기.자동집단키생성("pork-belly", "apt 101", "frozen", "lcl");

        Assert.Equal(first, second);
    }

    [Fact]
    public void 상태제안_예약결제가충분하면_확정대기를반환한다()
    {
        var status = 공동구매자동집단화계획기.상태제안(
            수요건수: 2,
            예약결제건수: 2,
            총희망수량: 6);

        Assert.Equal(공동구매자동집단상태코드.확정대기, status);
    }

    [Fact]
    public void 상태제안_관심표시만있고수요가작으면_수요수집중을반환한다()
    {
        var status = 공동구매자동집단화계획기.상태제안(
            수요건수: 1,
            예약결제건수: 0,
            총희망수량: 3);

        Assert.Equal(공동구매자동집단상태코드.수요수집중, status);
    }

    [Fact]
    public void 상태제안_투표에서전달한참여자와수량목표를모두충족하면_확정대기를반환한다()
    {
        var status = 공동구매자동집단화계획기.상태제안(
            수요건수: 3,
            예약결제건수: 0,
            총희망수량: 12,
            목표참여자수: 3,
            목표수량: 10);

        Assert.Equal(공동구매자동집단상태코드.확정대기, status);
    }

    [Fact]
    public void 상태제안_투표의참여자목표만충족하면_수요수집중을유지한다()
    {
        var status = 공동구매자동집단화계획기.상태제안(
            수요건수: 3,
            예약결제건수: 0,
            총희망수량: 8,
            목표참여자수: 3,
            목표수량: 10);

        Assert.Equal(공동구매자동집단상태코드.수요수집중, status);
    }
}
