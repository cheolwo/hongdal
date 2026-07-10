using 홍달.Services.Dispatch.Queue;

namespace Hongdal.Tests.Services.Dispatch.Queue;

public class 배차공개전환시점정책Tests
{
    [Fact]
    public void 당일의뢰는_배차대기생성후_설정분이_지나면_공개전환대상이다()
    {
        var createdAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);
        var now = createdAt.AddMinutes(31);
        var pickupStart = createdAt.AddHours(3);

        var result = 배차공개전환시점정책.공개전환대상(
            now,
            createdAt,
            pickupStart,
            당일미배정공개전환분: 30,
            예약상차전공개전환시간: 24,
            예약최소추천유지분: 60);

        Assert.True(result);
    }

    [Fact]
    public void 예약의뢰는_상차하루전까지_공개전환하지_않는다()
    {
        var createdAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);
        var pickupStart = createdAt.AddDays(3);
        var now = createdAt.AddMinutes(31);

        var result = 배차공개전환시점정책.공개전환대상(
            now,
            createdAt,
            pickupStart,
            당일미배정공개전환분: 30,
            예약상차전공개전환시간: 24,
            예약최소추천유지분: 60);

        Assert.False(result);
    }

    [Fact]
    public void 예약의뢰는_상차하루전이_되면_공개전환대상이다()
    {
        var createdAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);
        var pickupStart = createdAt.AddDays(3);
        var now = pickupStart.AddHours(-24);

        var result = 배차공개전환시점정책.공개전환대상(
            now,
            createdAt,
            pickupStart,
            당일미배정공개전환분: 30,
            예약상차전공개전환시간: 24,
            예약최소추천유지분: 60);

        Assert.True(result);
    }

    [Fact]
    public void 예약의뢰는_상차까지_24시간30분_남아도_최소1시간은_추천큐에_머문다()
    {
        var createdAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);
        var pickupStart = createdAt.AddHours(24).AddMinutes(30);
        var now = createdAt.AddMinutes(45);

        var result = 배차공개전환시점정책.공개전환대상(
            now,
            createdAt,
            pickupStart,
            당일미배정공개전환분: 30,
            예약상차전공개전환시간: 24,
            예약최소추천유지분: 60);

        Assert.False(result);
    }

    [Fact]
    public void 예약의뢰는_상차까지_24시간30분_남았으면_1시간후_공개전환대상이다()
    {
        var createdAt = new DateTime(2026, 7, 11, 9, 0, 0, DateTimeKind.Utc);
        var pickupStart = createdAt.AddHours(24).AddMinutes(30);
        var now = createdAt.AddHours(1);

        var result = 배차공개전환시점정책.공개전환대상(
            now,
            createdAt,
            pickupStart,
            당일미배정공개전환분: 30,
            예약상차전공개전환시간: 24,
            예약최소추천유지분: 60);

        Assert.True(result);
    }
}
