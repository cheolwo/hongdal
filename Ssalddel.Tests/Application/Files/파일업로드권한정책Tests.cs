using Ssalddel.Application.Files;
using 살뜰.도메인.운송;

namespace Ssalddel.Tests.Application.Files;

public sealed class 파일업로드권한정책Tests
{
    [Theory]
    [InlineData("TransportPickupComplete")]
    [InlineData("TransportDropoffComplete")]
    [InlineData("TransportIssueEvidence")]
    [InlineData("운송상차완료Command")]
    [InlineData("운송인수완료Command")]
    [InlineData("운송문제신고Command")]
    public void 운송증빙업로드인가_운송증빙CommandName이면_true(string commandName)
    {
        Assert.True(파일업로드권한정책.운송증빙업로드인가(commandName));
    }

    [Fact]
    public void 운송증빙업로드권한있음_배정기사이면_true()
    {
        var transport = new 운송원장
        {
            기사_운송자 = "driver-1"
        };

        Assert.True(파일업로드권한정책.운송증빙업로드권한있음(transport, "driver-1", "기사"));
    }

    [Fact]
    public void 운송증빙업로드권한있음_다른기사이면_false()
    {
        var transport = new 운송원장
        {
            기사_운송자 = "driver-1"
        };

        Assert.False(파일업로드권한정책.운송증빙업로드권한있음(transport, "driver-2", "기사"));
    }

    [Fact]
    public void 운송증빙업로드권한있음_서버관리자이면_true()
    {
        var transport = new 운송원장
        {
            기사_운송자 = "driver-1"
        };

        Assert.True(파일업로드권한정책.운송증빙업로드권한있음(transport, "admin-1", "서버관리자"));
    }
}
