using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Food;

public sealed class EfSsalddelFoodOrderStoreTests
{
    [Fact]
    public async Task 같은주문자와클라이언트요청Id는_Rdb에한건만저장한다()
    {
        await using var db = CreateContext();
        var store = new EfSsalddelFoodOrderStore(db);
        var requestId = Guid.NewGuid();
        var request = CreateRequest(requestId);

        var first = store.AddOrder(request);
        var retried = store.AddOrder(CreateRequest(requestId));

        Assert.Equal(first.주문번호, retried.주문번호);
        Assert.Equal(requestId, retried.클라이언트요청Id);
        Assert.Equal(1, await db.음식주문.CountAsync());
        Assert.Equal(1001, (await db.음식주문상품.SingleAsync()).메뉴Id);
    }

    [Fact]
    public async Task 음식점진행요청Id와처리자는_Rdb상태이력에한번만저장한다()
    {
        await using var db = CreateContext();
        var store = new EfSsalddelFoodOrderStore(db);
        var order = store.AddOrder(CreateRequest(Guid.NewGuid()));
        var request = new 음식점주문진행변경요청
        {
            클라이언트요청Id = Guid.NewGuid(),
            작업 = 음식점주문진행작업코드.거절,
            사유 = "재료 품절"
        };

        var first = store.음식점진행변경(order.주문번호, request, "restaurant-user");
        var retried = store.음식점진행변경(order.주문번호, request, "restaurant-user");

        Assert.True(first?.새로변경됨);
        Assert.False(retried?.새로변경됨);
        var history = Assert.Single(
            await db.음식주문상태이력
                .Where(item => item.클라이언트요청Id == request.클라이언트요청Id)
                .ToListAsync());
        Assert.Equal("restaurant-user", history.처리UserId);
        Assert.Equal(음식주문상태코드.거절, history.다음상태);
    }

    [Fact]
    public async Task 주문자수령확인은_전달완료와소유권을확인하고_요청Id를한번만저장한다()
    {
        await using var db = CreateContext();
        var store = new EfSsalddelFoodOrderStore(db);
        var order = store.AddOrder(CreateRequest(Guid.NewGuid()));
        var entity = await db.음식주문
            .Include(item => item.상태이력)
            .SingleAsync(item => item.주문번호 == order.주문번호);
        entity.상태 = 음식주문상태코드.전달완료;
        entity.배차상태 = 음식주문배차상태코드.배달완료;
        await db.SaveChangesAsync();
        var request = new 주문자음식주문수령확인요청
        {
            클라이언트요청Id = Guid.NewGuid(),
            확인메모 = "정상 수령"
        };

        var otherUser = store.주문자수령확인(order.주문번호, request, "other-orderer");
        var first = store.주문자수령확인(order.주문번호, request, "orderer-1");
        var retried = store.주문자수령확인(order.주문번호, request, "orderer-1");

        Assert.Null(otherUser);
        Assert.True(first?.새로변경됨);
        Assert.False(retried?.새로변경됨);
        Assert.Equal(음식주문상태코드.수령확인, retried?.주문.상태);
        var history = Assert.Single(
            await db.음식주문상태이력
                .Where(item => item.클라이언트요청Id == request.클라이언트요청Id)
                .ToListAsync());
        Assert.Equal("orderer-1", history.처리UserId);
        Assert.Equal(음식주문상태코드.전달완료, history.이전상태);
        Assert.Equal(음식주문상태코드.수령확인, history.다음상태);
        Assert.Contains("정상 수령", history.사유);
    }

    [Fact]
    public void 주문자수령확인은_기사전달완료전에는거부한다()
    {
        using var db = CreateContext();
        var store = new EfSsalddelFoodOrderStore(db);
        var order = store.AddOrder(CreateRequest(Guid.NewGuid()));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            store.주문자수령확인(
                order.주문번호,
                new 주문자음식주문수령확인요청
                {
                    클라이언트요청Id = Guid.NewGuid()
                },
                "orderer-1"));

        Assert.Contains("기사 전달 완료", exception.Message);
    }

    private static 음식주문등록요청 CreateRequest(Guid requestId)
        => new()
        {
            클라이언트요청Id = requestId,
            음식점Id = 101,
            주문자UserId = "orderer-1",
            수령인정보 = new 음식주문수령인정보Dto
            {
                수령인명 = "주문자",
                연락처 = "010-1234-5678",
                주소 = "서울특별시 중구 세종대로 1"
            },
            상품목록 =
            [
                new 음식주문상품Dto
                {
                    메뉴Id = 1001,
                    상품명 = "살뜰김밥",
                    수량 = 2,
                    단가 = 4_500
                }
            ]
        };

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"food-order-idempotency-{Guid.NewGuid():N}")
                .Options,
            new PassThroughEncryptionService());

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
