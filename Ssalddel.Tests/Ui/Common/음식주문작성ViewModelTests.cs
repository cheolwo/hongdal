using Ssalddel.Contracts.Food;
using Ssalddel.Contracts.Restaurants;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 음식주문작성ViewModelTests
{
    [Fact]
    public async Task 선택수량과수령정보를등록하고_서버주문번호를보관한다()
    {
        var service = new RecordingWriteService();
        var viewModel = new 음식주문작성ViewModel(service);
        viewModel.음식점설정(CreateRestaurant());
        viewModel.메뉴수량변경(1001, 2);
        viewModel.수령인명 = "주문자";
        viewModel.연락처 = "010-1234-5678";
        viewModel.주소 = "서울특별시 중구 세종대로 1";

        var succeeded = await viewModel.등록Async();

        Assert.True(succeeded);
        Assert.True(viewModel.제출가능);
        Assert.Equal(9_000m, viewModel.주문금액);
        Assert.Equal("FOOD-VM-1", viewModel.등록응답?.주문번호);
        var item = Assert.Single(service.LastRequest!.상품목록);
        Assert.Equal(1001, item.메뉴Id);
        Assert.Equal(2, item.수량);
        Assert.Equal("현장결제", service.LastRequest.결제수단);
    }

    [Fact]
    public async Task 최소주문금액미달은_Api를호출하지않는다()
    {
        var service = new RecordingWriteService();
        var viewModel = new 음식주문작성ViewModel(service);
        viewModel.음식점설정(CreateRestaurant());
        viewModel.메뉴수량변경(1001, 1);
        viewModel.수령인명 = "주문자";
        viewModel.연락처 = "010-1234-5678";
        viewModel.주소 = "서울특별시 중구 세종대로 1";

        var succeeded = await viewModel.등록Async();

        Assert.False(succeeded);
        Assert.Null(service.LastRequest);
        Assert.Contains("최소 주문 금액", viewModel.오류메시지);
    }

    private static 음식점공개상세응답 CreateRestaurant()
        => new()
        {
            음식점 = new 음식점공개요약응답
            {
                Id = 101,
                상호명 = "살뜰분식",
                주문가능여부 = true,
                최소주문금액 = 8_000
            },
            메뉴목록 =
            [
                new 음식점메뉴공개응답
                {
                    Id = 1001,
                    메뉴명 = "살뜰김밥",
                    판매가 = 4_500
                }
            ]
        };

    private sealed class RecordingWriteService : I주문자음식주문쓰기Service
    {
        public 음식주문등록요청? LastRequest { get; private set; }

        public Task<음식주문응답> 등록Async(
            음식주문등록요청 request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new 음식주문응답 { 주문번호 = "FOOD-VM-1" });
        }
    }
}
