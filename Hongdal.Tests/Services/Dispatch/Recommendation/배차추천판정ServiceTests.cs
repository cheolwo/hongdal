using 홍달.Services.Dispatch.Recommendation;
using 홍달.도메인.화주;

namespace Hongdal.Tests.Services.Dispatch.Recommendation;

public class 배차추천판정ServiceTests
{
    private readonly 배차추천판정Service _service = new();

    [Fact]
    public void 판정_조건이맞으면_묶음삽입추천을_반환한다()
    {
        var request = CreateRequest();

        var result = _service.판정(request, 추가지연분: 8m, 픽업시간창여유분: 12m, 경로기준거리Km: 4m);

        Assert.Equal("bundle_insert", result.추천유형);
        Assert.True(result.묶음삽입가능);
        Assert.True(result.도착후추천가능);
        Assert.True(result.차량적합여부);
        Assert.False(result.단독배송여부);
    }

    [Fact]
    public void 판정_묶음불가요청이면_단독추천만_반환한다()
    {
        var request = CreateRequest(운송방식: "단독", 요청사항: "묶음불가");

        var result = _service.판정(request, 추가지연분: 0m, 픽업시간창여유분: 10m, 경로기준거리Km: 2m);

        Assert.Equal("next_after_dropoff", result.추천유형);
        Assert.True(result.단독배송여부);
        Assert.False(result.묶음삽입가능);
        Assert.True(result.도착후추천가능);
        Assert.Equal(0m, result.허용지연분);
    }

    [Fact]
    public void 판정_차량부적합이면_즉시_single을_반환한다()
    {
        var request = CreateRequest();
        var 적합성결과 = new 차량화물적합성결과(false, ["높이 주의"], ["냉동 차량 필요"]);

        var result = _service.판정(request, 추가지연분: 3m, 픽업시간창여유분: 15m, 경로기준거리Km: 1m, 적합성결과);

        Assert.Equal("single", result.추천유형);
        Assert.False(result.차량적합여부);
        Assert.Contains("냉동 차량 필요", result.차량부적합사유);
        Assert.Contains("높이 주의", result.차량경고);
        Assert.False(result.묶음삽입가능);
        Assert.False(result.도착후추천가능);
    }

    [Fact]
    public void 판정_묶음조건이안되지만_가까우면_도착후추천을_반환한다()
    {
        var request = CreateRequest(서비스레벨: "긴급");

        var result = _service.판정(request, 추가지연분: 12m, 픽업시간창여유분: 2m, 경로기준거리Km: 3m);

        Assert.Equal("next_after_dropoff", result.추천유형);
        Assert.False(result.묶음삽입가능);
        Assert.True(result.도착후추천가능);
        Assert.Equal(5m, result.허용지연분);
        Assert.True(result.화물민감여부);
    }

    private static 화주운송의뢰 CreateRequest(
        string 운송방식 = "혼적",
        string 서비스레벨 = "일반",
        string 요청사항 = "",
        bool 화물파손주의여부 = false,
        string 화물온도조건 = "상온")
    {
        return new 화주운송의뢰
        {
            운송방식 = 운송방식,
            서비스레벨 = 서비스레벨,
            요청사항 = 요청사항,
            화물종류 = "생활용품",
            화물설명 = "테스트 화물",
            화물파손주의여부 = 화물파손주의여부,
            화물온도조건 = 화물온도조건
        };
    }
}
