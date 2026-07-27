using 살뜰.Services.Dispatch.Recommendation;

namespace Ssalddel.Tests.Services.Dispatch.Recommendation;

public sealed class FoodDeliveryDriverAddressDisclosureTests
{
    [Fact]
    public void 추천전에는_전달지_상세주소를_권역으로_줄인다()
    {
        var result = 음식배달기사업무Service.ToApproximateAddress(
            "서울특별시 강남구 테헤란로 123");

        Assert.Equal("서울특별시 강남구 인근", result);
    }

    [Fact]
    public void 추천전에는_전달지_좌표를_약_일킬로미터_단위로_줄인다()
    {
        var result = 음식배달기사업무Service.ToApproximateCoordinate(37.501234m);

        Assert.Equal(37.50m, result);
    }

    [Fact]
    public void 짧은_주소에서도_번지_숫자는_추천전에_숨긴다()
    {
        var result = 음식배달기사업무Service.ToApproximateAddress("테헤란로 123");

        Assert.Equal("테헤란로 인근", result);
    }
}
