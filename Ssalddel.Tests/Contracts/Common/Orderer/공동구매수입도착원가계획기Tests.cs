using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 공동구매수입도착원가계획기Tests
{
    [Fact]
    public void Plan_SeparatesCifTaxAndDomestic3plInboundCosts()
    {
        var plan = 공동구매수입도착원가계획기.계획(new 수입도착원가초안(
            수량Kg: 100m,
            상품매입단가Krw: 5000m,
            해외처리단가Krw: 500m,
            국제운임보험단가Krw: 1000m,
            관세율: 0.08m,
            수입부가세율: 0.1m,
            보세창고단가Krw: 200m,
            관세사수수료단가Krw: 100m,
            국내3PL운송단가Krw: 300m,
            물류대행입고단가Krw: 400m,
            통관검토필요: true));

        Assert.Equal(6500m, plan.예상Cif단가Krw);
        Assert.Equal(7020m, plan.단계목록.Single(x => x.단계코드 == 수입도착원가단계코드.관세).Accumulated단가Krw);
        Assert.Equal(7722m, plan.예상세후단가Krw);
        Assert.Equal(8722m, plan.예상도착단가Krw);
        Assert.Equal(872200m, plan.예상도착총액Krw);
        Assert.Equal(수입도착원가단계상태코드.검토필요, plan.단계목록.Single(x => x.단계코드 == 수입도착원가단계코드.통관검토).상태코드);
    }

    [Fact]
    public void Plan_통관반려_MarksReviewBlockedAndWarns()
    {
        var plan = 공동구매수입도착원가계획기.계획(new 수입도착원가초안(
            수량Kg: 10m,
            상품매입단가Krw: 3000m,
            통관반려: true));

        Assert.Equal(수입도착원가단계상태코드.차단, plan.단계목록.Single(x => x.단계코드 == 수입도착원가단계코드.통관검토).상태코드);
        Assert.NotEmpty(plan.경고목록);
    }
}
