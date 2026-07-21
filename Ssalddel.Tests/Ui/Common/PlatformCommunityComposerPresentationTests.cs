using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Components.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class PlatformCommunityComposerPresentationTests
{
    [Fact]
    public void 비로그인_익명_게시판은_자동_닉네임과_작성_조건을_함께_안내한다()
    {
        var access = PlatformCommunityComposerPresentation.ResolvePostingAccess(
            PlatformCommunityPostCategories.General,
            isAuthenticated: false,
            editingPostId: null,
            draftNickname: null);

        Assert.Equal(CommunityBoardPostingAccessCodes.Anonymous, access.Code);
        Assert.Equal(Severity.Success, access.Severity);
        Assert.True(access.UsesAutomaticAnonymousNickname);
        Assert.Equal("지나가는 이웃-****", access.AutomaticAnonymousNicknameDisplay);
        Assert.Contains("자동", access.Notice);
    }

    [Fact]
    public void 로그인_작성자는_익명_게시판에서도_계정_공개_닉네임을_사용한다()
    {
        var access = PlatformCommunityComposerPresentation.ResolvePostingAccess(
            PlatformCommunityPostCategories.General,
            isAuthenticated: true,
            editingPostId: null,
            draftNickname: "동네 이웃");

        Assert.False(access.UsesAutomaticAnonymousNickname);
        Assert.Contains("계정의 공개 닉네임", access.Notice);
    }

    [Fact]
    public void 운영자_게시판은_일반_작성_불가를_경고한다()
    {
        var access = PlatformCommunityComposerPresentation.ResolvePostingAccess(
            CommunityBoardCatalog.NoticeGuide.DisplayName,
            isAuthenticated: true,
            editingPostId: null,
            draftNickname: null);

        Assert.Equal(CommunityBoardPostingAccessCodes.OperatorOnly, access.Code);
        Assert.Equal("운영자 작성", access.Label);
        Assert.Equal(Severity.Warning, access.Severity);
        Assert.Contains("직접 작성할 수 없는", access.Notice);
    }

    [Theory]
    [InlineData(true, null, false, "저장 중")]
    [InlineData(false, 42L, false, "수정")]
    [InlineData(false, null, true, "예약")]
    [InlineData(false, null, false, "등록")]
    public void 저장_버튼은_작성_상태별_행동을_명확히_표시한다(
        bool isSaving,
        long? editingPostId,
        bool isScheduledPublication,
        string expected)
        => Assert.Equal(
            expected,
            PlatformCommunityComposerPresentation.ResolvePublishActionText(
                isSaving,
                editingPostId,
                isScheduledPublication));

    [Fact]
    public void ViewModel_상태는_외부_상태보다_우선한다()
    {
        var status = PlatformCommunityComposerPresentation.ResolveStatus(
            "작성 내용을 확인해 주세요.",
            CommunityComposerMessageKind.Warning,
            "외부 작업 완료",
            Severity.Success);

        Assert.Equal("작성 내용을 확인해 주세요.", status.Message);
        Assert.Equal(Severity.Warning, status.Severity);
    }

    [Fact]
    public void ViewModel_상태가_없으면_외부_도구_상태를_표시한다()
    {
        var status = PlatformCommunityComposerPresentation.ResolveStatus(
            null,
            CommunityComposerMessageKind.Info,
            "원장을 연결했습니다.",
            Severity.Success);

        Assert.Equal("원장을 연결했습니다.", status.Message);
        Assert.Equal(Severity.Success, status.Severity);
    }
}
