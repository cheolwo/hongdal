using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 공동구매결제단계계획기Tests
{
    [Fact]
    public void Plan_PickupCompleted_RequestsFirstPaymentOnly()
    {
        var plan = 공동구매결제단계계획기.계획(new 공동구매결제단계초안(
            공동구매Id: "gp-1",
            주문자Id: "orderer-1",
            총금액: 100000m,
            상차완료여부: true,
            하차완료여부: false,
            분배확인율: 0m));

        Assert.Equal(40000m, plan.요청가능금액);
        Assert.Contains(plan.라인목록, x =>
            x.단계코드 == 공동구매결제단계코드.상차1차지급 &&
            x.상태 == 공동구매결제단계상태코드.요청가능);
        Assert.Contains(plan.라인목록, x =>
            x.단계코드 == 공동구매결제단계코드.하차2차지급 &&
            x.상태 == 공동구매결제단계상태코드.차단);
    }

    [Fact]
    public void Plan_DropoffCompletedAfterFirstPayment_RequestsSecondPayment()
    {
        var plan = 공동구매결제단계계획기.계획(new 공동구매결제단계초안(
            공동구매Id: "gp-1",
            주문자Id: "orderer-1",
            총금액: 100000m,
            상차완료여부: true,
            하차완료여부: true,
            분배확인율: 0.3m,
            지급완료단계코드목록: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                공동구매결제단계코드.상차1차지급
            }));

        Assert.Equal(40000m, plan.지급완료금액);
        Assert.Equal(40000m, plan.요청가능금액);
        Assert.True(plan.최종지급차단여부);
    }

    [Fact]
    public void Plan_DistributionConfirmed_RequestsFinalPayment()
    {
        var plan = 공동구매결제단계계획기.계획(new 공동구매결제단계초안(
            공동구매Id: "gp-1",
            주문자Id: "orderer-1",
            총금액: 100000m,
            상차완료여부: true,
            하차완료여부: true,
            분배확인율: 0.82m,
            지급완료단계코드목록: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                공동구매결제단계코드.상차1차지급,
                공동구매결제단계코드.하차2차지급
            }));

        Assert.Equal(80000m, plan.지급완료금액);
        Assert.Equal(20000m, plan.요청가능금액);
        Assert.False(plan.최종지급차단여부);
        Assert.Contains(plan.라인목록, x =>
            x.단계코드 == 공동구매결제단계코드.분배확인최종지급 &&
            x.상태 == 공동구매결제단계상태코드.요청가능);
    }

    [Fact]
    public void Plan_Rejects비율sThatDoNotSumToOne()
    {
        var draft = new 공동구매결제단계초안(
            공동구매Id: "gp-1",
            주문자Id: "orderer-1",
            총금액: 100000m,
            상차완료여부: true,
            하차완료여부: true,
            분배확인율: 1m,
            정책: new 공동구매결제단계정책(
                상차1차지급비율: 0.5m,
                하차2차지급비율: 0.4m,
                분배최종지급비율: 0.2m));

        Assert.Throws<ArgumentException>(() => 공동구매결제단계계획기.계획(draft));
    }
}
