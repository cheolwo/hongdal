using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Community;
using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityMapApplicationLedgerPresentationTests
{
    [Fact]
    public void 제출전원장은_내가원장과현재작성단계를표시한다()
    {
        var badge = CommunityMapApplicationLedgerPresentation.For(new 지도신청가원장Response
        {
            업무Code = 신청개인정보업무Codes.물류대행,
            상태 = 커뮤니티원장상태.초안,
            현재단계Key = 지도신청가원장정책.신청접수단계
        });

        Assert.Equal("내 가원장", badge.KindLabel);
        Assert.Equal("물류대행", badge.WorkLabel);
        Assert.Equal(커뮤니티원장상태.초안, badge.StateLabel);
        Assert.Equal("신청서 작성", badge.StepLabel);
        Assert.Equal("이 원장 선택", badge.ActionLabel);
    }

    [Fact]
    public void 제출및동의철회원장은_성숙도와보호상태를우선표시한다()
    {
        var submitted = CommunityMapApplicationLedgerPresentation.For(new 지도신청가원장Response
        {
            업무Code = 신청개인정보업무Codes.운송대행,
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = 지도신청가원장정책.신청제출단계,
            실원장전환됨 = true
        });
        var withdrawn = CommunityMapApplicationLedgerPresentation.For(new 지도신청가원장Response
        {
            업무Code = 신청개인정보업무Codes.개별주문,
            상태 = 커뮤니티원장상태.보류,
            현재단계Key = 지도신청가원장정책.동의철회확인단계,
            동의철회보류 = true
        });

        Assert.Equal("내 신청 원장", submitted.KindLabel);
        Assert.Equal("운송대행", submitted.WorkLabel);
        Assert.Equal("신청 제출", submitted.StepLabel);
        Assert.Equal("동의 철회 검토", withdrawn.StateLabel);
        Assert.Equal("동의 철회 검토 상태 선택", withdrawn.ActionLabel);
    }
}
