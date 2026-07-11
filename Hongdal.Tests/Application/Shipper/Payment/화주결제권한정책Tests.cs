using Hongdal.Application.Shipper.Payment;
using 홍달.도메인.결제;
using 홍달.도메인.공통;
using 홍달.도메인.화주;

namespace Hongdal.Tests.Application.Shipper.Payment;

public sealed class 화주결제권한정책Tests
{
    [Fact]
    public void 의뢰결제권한있음_주문자이면_true()
    {
        var request = new 화주운송의뢰
        {
            화주Id = "shipper-1",
            주문자UserId = "orderer-1"
        };

        Assert.True(화주결제권한정책.의뢰결제권한있음(request, "orderer-1", "화주"));
    }

    [Fact]
    public void 의뢰결제권한있음_다른화주이면_false()
    {
        var request = new 화주운송의뢰
        {
            화주Id = "shipper-1",
            주문자UserId = "orderer-1"
        };

        Assert.False(화주결제권한정책.의뢰결제권한있음(request, "shipper-2", "화주"));
    }

    [Fact]
    public void 결제승인권한있음_의뢰가없어도_결제화주이면_true()
    {
        var payment = new 결제
        {
            화주Id = "shipper-1"
        };

        Assert.True(화주결제권한정책.결제승인권한있음(payment, request: null, "shipper-1", "화주"));
    }

    [Fact]
    public void 결제승인권한있음_서버관리자이면_true()
    {
        var payment = new 결제
        {
            화주Id = "shipper-1"
        };
        var request = new 화주운송의뢰
        {
            화주Id = "shipper-2",
            주문자UserId = "orderer-2"
        };

        Assert.True(화주결제권한정책.결제승인권한있음(payment, request, "admin-1", "서버관리자"));
    }

    [Fact]
    public void 결제준비요청검증_권한과상태가맞으면_통과()
    {
        var request = new 화주운송의뢰
        {
            화주Id = "shipper-1",
            배차상태 = 상태값.배차상태.상차완료,
            결제상태 = 상태값.결제상태.결제대기
        };

        var result = 화주운송결제진행정책.결제준비요청검증(request, "shipper-1", "화주");

        Assert.True(result.통과);
    }

    [Fact]
    public void 결제준비요청검증_상차완료전이면_실패()
    {
        var request = new 화주운송의뢰
        {
            화주Id = "shipper-1",
            배차상태 = "배차확정",
            결제상태 = 상태값.결제상태.결제대기
        };

        var result = 화주운송결제진행정책.결제준비요청검증(request, "shipper-1", "화주");

        Assert.False(result.통과);
        Assert.Equal("상차완료 이후에만 결제를 진행할 수 있습니다.", result.실패사유);
    }

    [Fact]
    public void 결제승인요청검증_완료결제이면_멱등완료로_반환()
    {
        var payment = new 결제
        {
            화주Id = "shipper-1",
            결제금액 = 10000,
            결제상태 = 상태값.결제상태.결제완료
        };

        var result = 화주운송결제진행정책.결제승인요청검증(payment, request: null, "shipper-1", "화주", 10000);

        Assert.True(result.통과);
        Assert.True(result.이미완료됨);
    }
}
