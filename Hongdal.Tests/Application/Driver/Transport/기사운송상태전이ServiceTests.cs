using Hongdal.Application.Driver.Transport;
using 홍달.도메인.운송;

namespace Hongdal.Tests.Application.Driver.Transport;

public class 기사운송상태전이ServiceTests
{
    private readonly 기사운송상태전이Service _service = new();

    [Fact]
    public void 상태변경_상차완료로_전이하면_출발픽업시각을_설정한다()
    {
        var 변경시각 = new DateTime(2026, 7, 1, 9, 30, 0, DateTimeKind.Utc);
        var 운송 = new 배송_운송
        {
            상태 = "상차지도착",
            UpdatedAt = 변경시각.AddMinutes(-5)
        };

        var result = _service.상태변경(운송, "상차완료", 변경시각);

        Assert.True(result.IsSuccess);
        Assert.Equal("상차완료", 운송.상태);
        Assert.Equal(변경시각, 운송.UpdatedAt);
        Assert.Equal(변경시각, 운송.출발_픽업);
    }

    [Fact]
    public void 상태변경_같은상태로_요청하면_UpdatedAt만_갱신한다()
    {
        var 기존출발시각 = new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc);
        var 변경시각 = 기존출발시각.AddMinutes(15);
        var 운송 = new 배송_운송
        {
            상태 = "상차완료",
            출발_픽업 = 기존출발시각,
            UpdatedAt = 기존출발시각
        };

        var result = _service.상태변경(운송, "상차완료", 변경시각);

        Assert.True(result.IsSuccess);
        Assert.Equal("상차완료", 운송.상태);
        Assert.Equal(변경시각, 운송.UpdatedAt);
        Assert.Equal(기존출발시각, 운송.출발_픽업);
    }

    [Fact]
    public void 상태변경_완료된운송은_다른상태로_변경할수없다()
    {
        var 운송 = new 배송_운송
        {
            상태 = "인수완료"
        };

        var result = _service.상태변경(운송, "하차지도착", DateTime.UtcNow);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, error => error.Message == "이미 완료된 운송입니다.");
        Assert.Equal("인수완료", 운송.상태);
    }

    [Fact]
    public void 상태변경_허용되지않은전이는_실패한다()
    {
        var 운송 = new 배송_운송
        {
            상태 = "배차대기"
        };

        var result = _service.상태변경(운송, "인수완료", DateTime.UtcNow);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, error => error.Message == "현재 상태(배차대기)에서는 인수완료 처리할 수 없습니다.");
        Assert.Equal("배차대기", 운송.상태);
    }
}
