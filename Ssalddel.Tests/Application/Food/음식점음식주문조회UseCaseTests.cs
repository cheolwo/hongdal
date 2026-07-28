using Ssalddel.Application.Food;
using Ssalddel.Services.Food;

namespace Ssalddel.Tests.Application.Food;

public sealed class 음식점음식주문조회UseCaseTests
{
    [Fact]
    public void 목록과상세는_로그인클레임의음식점범위만반환한다()
    {
        var useCase = new 음식점음식주문조회UseCase(
            new InMemorySsalddelFoodOrderStore());

        var orders = useCase.목록(101);

        Assert.NotEmpty(orders.Items);
        Assert.All(orders.Items, order => Assert.Equal(101, order.음식점Id));
        var own = useCase.상세(orders.Items[0].주문번호, 101);
        Assert.NotNull(own);

        var otherRestaurantOrder = useCase.목록(102).Items.First();
        Assert.Null(useCase.상세(otherRestaurantOrder.주문번호, 101));
    }
}
