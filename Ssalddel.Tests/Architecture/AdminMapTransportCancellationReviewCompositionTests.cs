namespace Ssalddel.Tests.Architecture;

public sealed class AdminMapTransportCancellationReviewCompositionTests
{
    [Fact]
    public void 운송취소검토는_일반WebApp이아닌_통합관리자운송화면에병합한다()
    {
        var root = FindRepositoryRoot();
        var list = Read("SsalddelAdmin", "Components", "Pages", "Requests.razor");
        var detail = Read("SsalddelAdmin", "Components", "Pages", "RequestDetail.razor");

        Assert.Contains("<MapTransportCancellationReviewQueue", list);
        Assert.Contains("reviewLedgerId", detail);
        Assert.Contains("CancellationReviewService.처리Async", detail);
        Assert.Contains("취소 요청 거절", detail);
        Assert.False(File.Exists(Path.Combine(
            root,
            "Ssalddel.WebApp",
            "Pages",
            "AdminMapTransportCancellationReviewPage.razor")));
    }

    [Fact]
    public void 통합관리자Client는_정식Admin경로와Bearer인증을사용한다()
    {
        var service = Read("SsalddelAdmin", "Services", "지도신청운송취소검토AdminService.cs");
        var program = Read("SsalddelAdmin", "Program.cs");

        Assert.Contains("api/v1/admin/community/map-transport-cancellation-reviews", service);
        Assert.Contains("AuthenticationHeaderValue(\"Bearer\"", service);
        Assert.Contains("session.서버관리자인가", service);
        Assert.Contains("AddHttpClient<지도신청운송취소검토AdminService>", program);
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
