using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

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
}
