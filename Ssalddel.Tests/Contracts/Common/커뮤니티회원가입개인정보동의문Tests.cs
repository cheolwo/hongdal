using Ssalddel.Contracts.Common;

namespace Ssalddel.Tests.Contracts.Common;

public sealed class 커뮤니티회원가입개인정보동의문Tests
{
    [Fact]
    public void 현재버전에_명시적으로_동의한_경우만_유효하다()
    {
        Assert.True(커뮤니티회원가입개인정보동의문.유효한동의(
            true,
            커뮤니티회원가입개인정보동의문.현재버전));

        Assert.False(커뮤니티회원가입개인정보동의문.유효한동의(
            false,
            커뮤니티회원가입개인정보동의문.현재버전));
        Assert.False(커뮤니티회원가입개인정보동의문.유효한동의(true, "이전-버전"));
        Assert.False(커뮤니티회원가입개인정보동의문.유효한동의(true, null));
    }

    [Fact]
    public void 안내문은_필수고지와_비회원_익명이용_경계를_포함한다()
    {
        Assert.Contains("계정", 커뮤니티회원가입개인정보동의문.수집이용목적, StringComparison.Ordinal);
        Assert.Contains("이메일", 커뮤니티회원가입개인정보동의문.수집항목, StringComparison.Ordinal);
        Assert.Contains("회원 탈퇴", 커뮤니티회원가입개인정보동의문.보유이용기간, StringComparison.Ordinal);
        Assert.Contains("거부", 커뮤니티회원가입개인정보동의문.동의거부안내, StringComparison.Ordinal);
        Assert.Contains("회원가입 없이", 커뮤니티회원가입개인정보동의문.동의거부안내, StringComparison.Ordinal);
    }
}
