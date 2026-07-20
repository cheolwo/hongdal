using Ssalddel.Contracts.Admin.Transport;

namespace Ssalddel.Tests.Contracts.Admin;

public sealed class 관리자운송의뢰취소환불정책Tests
{
    [Fact]
    public void 결제대기_미배차_의뢰는_환불없이_취소할_수_있다()
    {
        var result = 관리자운송의뢰취소환불정책.평가(
            "생성됨",
            "결제대기",
            "결제대기",
            "미시작");

        Assert.True(result.처리가능);
        Assert.False(result.환불상태기록필요);
        Assert.Contains("결제 취소", result.처리명);
    }

    [Fact]
    public void 결제완료_미배차_의뢰는_환불상태_기록이_필요하다()
    {
        var result = 관리자운송의뢰취소환불정책.평가(
            "생성됨",
            "결제완료",
            "결제완료",
            "배차대기");

        Assert.True(result.처리가능);
        Assert.True(result.환불상태기록필요);
        Assert.Contains("환불", result.처리명);
    }

    [Theory]
    [InlineData("배차확정")]
    [InlineData("상차중")]
    [InlineData("운송중")]
    [InlineData("인수완료")]
    public void 운송이_진행된_의뢰는_자동_취소하지_않는다(string dispatchStatus)
    {
        var result = 관리자운송의뢰취소환불정책.평가(
            "생성됨",
            "결제완료",
            "결제완료",
            dispatchStatus);

        Assert.False(result.처리가능);
        Assert.Contains("별도 운영 절차", result.안내문구);
    }

    [Fact]
    public void 완료되거나_이미_취소된_의뢰는_다시_처리하지_않는다()
    {
        var completed = 관리자운송의뢰취소환불정책.평가(
            "완료",
            "결제완료",
            "정산완료",
            "인수완료");
        var canceled = 관리자운송의뢰취소환불정책.평가(
            "취소",
            "환불됨",
            "정산취소",
            "취소");

        Assert.False(completed.처리가능);
        Assert.False(canceled.처리가능);
    }

    [Fact]
    public void 명시적_확인은_의뢰Id와_사유를_검증한다()
    {
        Assert.Null(관리자운송의뢰취소환불정책.명시적확인오류(
            "REQ-001",
            " req-001 ",
            "화주 요청 확인"));
        Assert.NotNull(관리자운송의뢰취소환불정책.명시적확인오류(
            "REQ-001",
            "REQ-002",
            "화주 요청 확인"));
        Assert.NotNull(관리자운송의뢰취소환불정책.명시적확인오류(
            "REQ-001",
            "REQ-001",
            " "));
        Assert.NotNull(관리자운송의뢰취소환불정책.명시적확인오류(
            "REQ-001",
            "REQ-001",
            new string('가', 301)));
    }
}
