using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Participants;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Services.Food;

public sealed class 음식주문메뉴검증ServiceTests
{
    [Fact]
    public async Task 공개메뉴Id를기준으로_클라이언트메뉴명과단가를서버값으로교체한다()
    {
        await using var db = CreateContext();
        await SeedRestaurantAsync(db);
        var service = new 음식주문메뉴검증Service(db);

        var result = await service.서버기준요청생성Async(
            CreateRequest(101, 1001, quantity: 2, name: "변조 메뉴", price: 1),
            CancellationToken.None);

        var item = Assert.Single(result.상품목록);
        Assert.Equal(1001, item.메뉴Id);
        Assert.Equal("살뜰김밥", item.상품명);
        Assert.Equal(4_500m, item.단가);
        Assert.Equal(2, item.수량);
    }

    [Fact]
    public async Task 다른음식점메뉴나품절메뉴는_주문스냅샷으로만들지않는다()
    {
        await using var db = CreateContext();
        await SeedRestaurantAsync(db);
        var service = new 음식주문메뉴검증Service(db);

        var wrongRestaurant = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.서버기준요청생성Async(
                CreateRequest(101, 2001, quantity: 1),
                CancellationToken.None));
        var soldOut = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.서버기준요청생성Async(
                CreateRequest(101, 1002, quantity: 1),
                CancellationToken.None));

        Assert.Contains("선택한 음식점", wrongRestaurant.Message);
        Assert.Contains("품절", soldOut.Message);
    }

    [Fact]
    public async Task 서버가격으로계산한금액이최소주문보다작으면_등록을거절한다()
    {
        await using var db = CreateContext();
        await SeedRestaurantAsync(db);
        var service = new 음식주문메뉴검증Service(db);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.서버기준요청생성Async(
                CreateRequest(101, 1001, quantity: 1, price: 99_999),
                CancellationToken.None));

        Assert.Contains("최소 주문 금액", exception.Message);
        Assert.Contains("4,500", exception.Message);
    }

    private static 음식주문등록요청 CreateRequest(
        long restaurantId,
        long menuId,
        int quantity,
        string name = "김밥",
        decimal price = 4_500)
        => new()
        {
            음식점Id = restaurantId,
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
                    메뉴Id = menuId,
                    상품명 = name,
                    수량 = quantity,
                    단가 = price
                }
            ],
            결제수단 = "현장결제"
        };

    private static async Task SeedRestaurantAsync(SsalddelContext db)
    {
        db.음식점공개프로필.AddRange(
            new 음식점공개프로필
            {
                Id = 101,
                상호명 = "살뜰분식",
                공개여부 = true,
                주문가능여부 = true,
                최소주문금액 = 8_000,
                메뉴목록 =
                [
                    new 음식점메뉴
                    {
                        Id = 1001,
                        메뉴명 = "살뜰김밥",
                        판매가 = 4_500,
                        공개여부 = true
                    },
                    new 음식점메뉴
                    {
                        Id = 1002,
                        메뉴명 = "품절라면",
                        판매가 = 5_000,
                        공개여부 = true,
                        품절여부 = true
                    }
                ]
            },
            new 음식점공개프로필
            {
                Id = 202,
                상호명 = "다른분식",
                공개여부 = true,
                주문가능여부 = true,
                메뉴목록 =
                [
                    new 음식점메뉴
                    {
                        Id = 2001,
                        메뉴명 = "다른김밥",
                        판매가 = 4_000,
                        공개여부 = true
                    }
                ]
            });
        await db.SaveChangesAsync();
    }

    private static SsalddelContext CreateContext()
        => new(
            new DbContextOptionsBuilder<SsalddelContext>()
                .UseInMemoryDatabase($"food-menu-validation-{Guid.NewGuid():N}")
                .Options,
            new PassThroughEncryptionService());

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
