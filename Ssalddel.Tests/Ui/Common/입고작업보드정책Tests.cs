using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 입고작업보드정책Tests
{
    [Theory]
    [InlineData(입고상태코드.예정, "도착·상품 확인", true)]
    [InlineData(입고상태코드.운송중, "도착 확인·검수 준비", true)]
    [InlineData(입고상태코드.완료, "재고 확인", false)]
    [InlineData(입고상태코드.취소, "추가 작업 없음", false)]
    [InlineData("미정상태", "관리자 확인", false)]
    public void 서버상태에따라_다음행동과상태전이후보를분리한다(
        string status,
        string expectedAction,
        bool expectedTransitionCandidate)
    {
        var result = 입고작업보드정책.해석(status);

        Assert.Equal(expectedAction, result.다음행동);
        Assert.Equal(expectedTransitionCandidate, result.상태전이후보);
    }
}
