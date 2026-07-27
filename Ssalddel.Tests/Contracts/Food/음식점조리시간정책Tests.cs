using Ssalddel.Contracts.Food;

namespace Ssalddel.Tests.Contracts.Food;

public sealed class 음식점조리시간정책Tests
{
    [Fact]
    public void 여러상품주문은_가장긴상품기본시간을_추천한다()
    {
        var result = 음식점조리시간정책.주문추천분(
            [
                new 음식주문상품Dto { 상품명 = "우동", 수량 = 1 },
                new 음식주문상품Dto { 상품명 = "돈까스", 수량 = 1 }
            ],
            new Dictionary<string, int>
            {
                ["우동"] = 12,
                ["돈까스"] = 25
            },
            음식점기본조리분: 20);

        Assert.Equal(25, result);
    }

    [Fact]
    public void 상품설정이없으면_음식점기본시간을_사용한다()
    {
        var result = 음식점조리시간정책.주문추천분(
            [new 음식주문상품Dto { 상품명 = "새 메뉴", 수량 = 1 }],
            new Dictionary<string, int>(),
            음식점기본조리분: 18);

        Assert.Equal(18, result);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(181, 180)]
    public void 조리시간은_지원범위로_제한한다(int input, int expected)
    {
        Assert.Equal(expected, 음식점조리시간정책.Clamp(input));
    }
}
