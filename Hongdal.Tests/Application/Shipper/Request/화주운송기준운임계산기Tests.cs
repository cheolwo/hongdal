using Hongdal.Application.Shipper.Request;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Tests.Application.Shipper.Request;

public class 화주운송기준운임계산기Tests
{
    [Fact]
    public void FindDefaultRate_다마스는_km당_1200원을_적용한다()
    {
        var rate = 화주운송기준운임계산기.FindDefaultRate("다마스");

        Assert.NotNull(rate);
        Assert.Equal(1200m, rate.Value.Km당단가);
    }

    [Fact]
    public void FindRate_일반_1톤은_가장_낮은_1톤_기본후보를_선택한다()
    {
        var rates = new[]
        {
            new 화주운송기준운임단가("1톤 냉동탑", 35000m, 1500m, 35000m, "test"),
            new 화주운송기준운임단가("1톤 카고", 35000m, 1300m, 35000m, "test")
        };

        var rate = 화주운송기준운임계산기.FindRate("1톤", rates);

        Assert.NotNull(rate);
        Assert.Equal("1톤 카고", rate.Value.차량종류);
        Assert.Equal(1300m, rate.Value.Km당단가);
    }

    [Fact]
    public void Calculate_차량별_기본운임과_거리운임을_더해_최종운임을_계산한다()
    {
        var request = new 화주운송기준운임견적요청
        {
            차량종류 = "다마스",
            예상거리Km = 10m
        };
        var rate = new 화주운송기준운임단가("다마스", 15000m, 1200m, 15000m, "test");

        var result = 화주운송기준운임계산기.Calculate(request, rate, 10m);

        Assert.Equal(10m, result.예상거리Km);
        Assert.Equal(12000m, result.거리운임);
        Assert.Equal(27000m, result.최종운임);
        Assert.True(result.직선거리기준);
        Assert.Equal("직선거리", result.거리계산방식);
    }

    [Fact]
    public void Calculate_Directions5_거리이면_직선거리기준을_끄고_실제경로방식을_남긴다()
    {
        var request = new 화주운송기준운임견적요청
        {
            차량종류 = "다마스"
        };
        var rate = new 화주운송기준운임단가("다마스", 15000m, 1200m, 15000m, "test");

        var result = 화주운송기준운임계산기.Calculate(
            request,
            rate,
            12.34m,
            직선거리기준: false,
            거리계산방식: "Directions5");

        Assert.False(result.직선거리기준);
        Assert.Equal("Directions5", result.거리계산방식);
        Assert.Equal(12.34m, result.예상거리Km);
        Assert.Equal(14808m, result.거리운임);
    }

    [Fact]
    public void Calculate_기본운임과_거리운임이_최소운임보다_낮으면_최소운임을_적용한다()
    {
        var request = new 화주운송기준운임견적요청
        {
            차량종류 = "테스트차량",
            예상거리Km = 2m
        };
        var rate = new 화주운송기준운임단가("테스트차량", 10000m, 1000m, 15000m, "test");

        var result = 화주운송기준운임계산기.Calculate(request, rate, 2m);

        Assert.Equal(2000m, result.거리운임);
        Assert.Equal(15000m, result.최종운임);
    }

    [Fact]
    public void ResolveDistanceKm_상차하차_좌표가_있으면_직선거리를_계산한다()
    {
        var request = new 화주운송기준운임견적요청
        {
            상차위도 = 37.5665m,
            상차경도 = 126.9780m,
            하차위도 = 37.5512m,
            하차경도 = 126.9882m
        };

        var distanceKm = 화주운송기준운임계산기.ResolveDistanceKm(request);

        Assert.NotNull(distanceKm);
        Assert.InRange(distanceKm.Value, 1.8m, 2.1m);
    }

    [Fact]
    public void ResolveStraightLineDistanceKm_예상거리보다_좌표_직선거리만_계산한다()
    {
        var request = new 화주운송기준운임견적요청
        {
            예상거리Km = 99m,
            상차위도 = 37.5665m,
            상차경도 = 126.9780m,
            하차위도 = 37.5512m,
            하차경도 = 126.9882m
        };

        var distanceKm = 화주운송기준운임계산기.ResolveStraightLineDistanceKm(request);

        Assert.NotNull(distanceKm);
        Assert.InRange(distanceKm.Value, 1.8m, 2.1m);
    }
}
