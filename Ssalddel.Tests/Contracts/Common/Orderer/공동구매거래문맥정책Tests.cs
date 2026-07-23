using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 공동구매거래문맥정책Tests
{
    [Fact]
    public void B2B집단은_구매조직과세금계산서요청을식별정보없이집계한다()
    {
        var group = new 공동구매자동집단응답
        {
            거래유형 = 공동구매거래유형코드.B2B,
            가격표시기준 = 공동구매가격표시기준코드.부가세별도,
            수요목록 =
            [
                Demand("organization-1", taxInvoiceRequired: true),
                Demand("organization-1", taxInvoiceRequired: false),
                Demand("organization-2", taxInvoiceRequired: true)
            ]
        };

        var context = 공동구매거래문맥정책.생성(group, "group-order-1");

        Assert.Equal(공동구매거래유형코드.B2B, context.거래유형);
        Assert.Equal(공동구매가격표시기준코드.부가세별도, context.가격표시기준);
        Assert.Equal("group-order-1", context.원천거래문맥원장Id);
        Assert.Equal(2, context.구매조직수);
        Assert.Equal(2, context.세금계산서요청수);
    }

    [Theory]
    [InlineData("B2C", "VatIncluded", "B2C", "VatIncluded", true)]
    [InlineData("B2B", "VatExcluded", "b2b", "VatExcluded", true)]
    [InlineData("B2B", "VatExcluded", "B2C", "VatIncluded", false)]
    [InlineData("B2B", "VatExcluded", "B2B", "VatIncluded", false)]
    public void 원장결합호환성은_거래유형과가격기준을함께비교한다(
        string leftType,
        string leftPriceBasis,
        string rightType,
        string rightPriceBasis,
        bool expected)
        => Assert.Equal(
            expected,
            공동구매거래문맥정책.호환됨(
                leftType,
                leftPriceBasis,
                rightType,
                rightPriceBasis));

    private static 공동구매자동수요응답 Demand(string organizationReference, bool taxInvoiceRequired)
        => new()
        {
            구매조직참조키 = organizationReference,
            세금계산서필요 = taxInvoiceRequired
        };
}
