using Hongdal.Contracts.Common.Participants;
using Hongdal.Contracts.Food;
using Hongdal.Services.Food;

namespace Hongdal.Tests.Services.Food;

public sealed class InMemoryHongdalFoodOrderStoreTests
{
    [Fact]
    public void 음식점수락은_주문상태와_음식점정보를_갱신한다()
    {
        var store = new InMemoryHongdalFoodOrderStore();
        var order = store.AddOrder(CreateOrderRequest());

        var accepted = store.음식점수락(
            order.주문번호,
            new 음식점주문수락요청
            {
                음식점명 = "홍달분식",
                음식점주소 = "서울특별시 마포구 월드컵북로 1",
                음식점상세주소 = "1층",
                조리예상분 = 20,
                수락메모 = "바로 조리 시작"
            });

        Assert.NotNull(accepted);
        Assert.Equal(음식주문상태코드.조리중, accepted.상태);
        Assert.Equal("홍달분식", accepted.음식점명);
        Assert.Equal("서울특별시 마포구 월드컵북로 1", accepted.음식점주소);
        Assert.Equal(음식주문배차상태코드.미요청, accepted.배차상태);
        Assert.NotNull(accepted.음식점수락시각Utc);
        Assert.NotNull(accepted.조리예상완료시각Utc);
        Assert.Contains(accepted.상태이력, x => x.다음상태 == 음식주문상태코드.조리중);
    }

    [Fact]
    public void 음식점수락후_배차대기반영은_배차상태와_대기Id를_갱신한다()
    {
        var store = new InMemoryHongdalFoodOrderStore();
        var order = store.AddOrder(CreateOrderRequest());
        store.음식점수락(
            order.주문번호,
            new 음식점주문수락요청
            {
                음식점명 = "홍달분식",
                음식점주소 = "서울특별시 마포구 월드컵북로 1",
                즉시픽업가능여부 = true
            });

        var dispatchRequestedAt = DateTime.UtcNow;
        var updated = store.배차대기반영(order.주문번호, 1234, dispatchRequestedAt);

        Assert.NotNull(updated);
        Assert.Equal(음식주문상태코드.픽업대기, updated.상태);
        Assert.Equal(음식주문배차상태코드.배차대기, updated.배차상태);
        Assert.Equal(1234, updated.배차대기Id);
        Assert.Equal(dispatchRequestedAt, updated.배차요청시각Utc);
    }

    private static 음식주문등록요청 CreateOrderRequest()
        => new()
        {
            음식점Id = 42,
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "홍길동",
                연락처 = "010-0000-0000",
                주소 = "서울특별시 마포구 양화로 10",
                상세주소 = "101호",
                주문자본인수령여부 = true
            },
            상품목록 =
            [
                new 음식주문상품Dto
                {
                    상품명 = "김밥",
                    수량 = 2,
                    단가 = 4500
                }
            ],
            결제수단 = "카드"
        };
}
