namespace Ssalddel.Tests.Architecture;

public sealed class AdminCommunityManagementCompositionTests
{
    [Fact]
    public void 통합관리자홈과탐색은_커뮤니티운영을중심으로구성한다()
    {
        var home = Read("SsalddelAdmin", "Components", "Pages", "Home.razor");
        var navigation = Read("SsalddelAdmin", "Services", "AdminV1NavigationPolicy.cs");
        var menu = Read("SsalddelAdmin", "Components", "Layout", "NavMenu.razor");

        Assert.Contains("NavigateTo(\"/community\"", home);
        Assert.Contains("new(\"커뮤니티 운영\", \"/community\"", navigation);
        Assert.Contains("new(\"사용자·콘텐츠 관리\", \"/community/users\"", navigation);
        Assert.Contains("new(\"공통 콘텐츠 관리\", \"/common-contents\"", navigation);
        Assert.Contains("new(\"운영 감사 기록\", \"/activity-logs\"", navigation);
        Assert.Contains("new(\"공개 범위 정책\", \"/view-policies\"", navigation);
        Assert.DoesNotContain("new(\"음식 배달 관리자\"", navigation);
        Assert.DoesNotContain("new(\"화물 배송 관리자\"", navigation);
        Assert.DoesNotContain("new(\"주문·창고 관리자\"", navigation);
        Assert.Contains("살뜰 커뮤니티 관리자", menu);
    }

    [Fact]
    public void 커뮤니티운영화면은_게시판과안전운영진입점을보여준다()
    {
        var dashboard = Read("SsalddelAdmin", "Components", "Pages", "CommunityAdminDashboard.razor");

        Assert.Contains("@page \"/community\"", dashboard);
        Assert.Contains("CommunityBoardCatalog.All", dashboard);
        Assert.Contains("Href=\"/community/users\"", dashboard);
        Assert.Contains("Href=\"/common-contents\"", dashboard);
        Assert.Contains("Href=\"/activity-logs\"", dashboard);
        Assert.Contains("관심·참여·연락처 공개·원장·실행은 서로 다른 동의 상태", dashboard);
    }

    [Fact]
    public void 사용자콘텐츠운영은_기존관리Api와감사사유및마스킹을사용한다()
    {
        var page = Read("SsalddelAdmin", "Components", "Pages", "CommunityUserManagement.razor");
        var service = Read("SsalddelAdmin", "Services", "CommunityManagementService.cs");
        var program = Read("SsalddelAdmin", "Program.cs");

        Assert.Contains("@page \"/community/users\"", page);
        Assert.Contains("MaskEmail(user.Email)", page);
        Assert.Contains("MaskPhone(user.PhoneNumber)", page);
        Assert.DoesNotContain("@user.Email", page);
        Assert.DoesNotContain("@user.PhoneNumber", page);
        Assert.Contains("actionReason.Trim().Length is >= 2 and <= 1000", page);
        Assert.Contains("api/v1/admin/community-management", service);
        Assert.Contains("SetPostVisibilityAsync", service);
        Assert.Contains("SetCommentVisibilityAsync", service);
        Assert.Contains("SetAttachmentCommentVisibilityAsync", service);
        Assert.Contains("AuthenticationHeaderValue(\"Bearer\"", service);
        Assert.Contains("AddHttpClient<CommunityManagementService>", program);
    }

    private static string Read(params string[] path)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. path]));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
