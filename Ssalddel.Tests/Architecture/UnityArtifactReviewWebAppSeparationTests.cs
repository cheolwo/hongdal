namespace Ssalddel.Tests.Architecture;

public sealed class UnityArtifactReviewWebAppSeparationTests
{
    [Fact]
    public void Unity산출물검토는_일반WebApp과_별도프로젝트다()
    {
        var project = Read("Ssalddel.Web.UnityReviewApp/Ssalddel.Web.UnityReviewApp.csproj");
        var mainSolution = Read("Ssalddel.slnx");
        var dedicatedSolution = Read("Ssalddel.UnityReview.slnx");
        var productSolution = Read("Ssalddel.v3.5.slnx");

        Assert.DoesNotContain("Ssalddel.WebApp.csproj", project);
        Assert.Contains("Ssalddel.Contracts.csproj", project);
        Assert.Contains("Ssalddel.Web.UnityReviewApp.csproj", mainSolution);
        Assert.Contains("Ssalddel.Web.UnityReviewApp.csproj", dedicatedSolution);
        Assert.DoesNotContain("Ssalddel.Web.UnityReviewApp.csproj", productSolution);
    }

    [Fact]
    public void 일반WebApp은_Unity산출물검토화면과Client를_포함하지않는다()
    {
        Assert.False(File.Exists(PathAt(
            "Ssalddel.WebApp/Pages/Synty공간조립모바일검토Page.razor")));
        Assert.False(File.Exists(PathAt(
            "Ssalddel.WebApp/Services/Synty공간조립모바일검토Client.cs")));
        Assert.DoesNotContain(
            "Synty공간조립모바일검토Client",
            Read("Ssalddel.WebApp/Program.cs"));
    }

    [Fact]
    public void 전용WebApp은_관리자권한과_별도브라우저저장경계를_사용한다()
    {
        var page = Read(
            "Ssalddel.Web.UnityReviewApp/Pages/Synty공간조립Web검토Page.razor");
        var auth = Read(
            "Ssalddel.Web.UnityReviewApp/Services/UnityReviewAuthSessionService.cs");
        var client = Read(
            "Ssalddel.Web.UnityReviewApp/Services/Synty공간조립모바일검토Client.cs");
        var offlineStore = Read(
            "Ssalddel.Web.UnityReviewApp/Services/Synty공간조립오프라인검토Store.cs");

        Assert.Contains("서버관리자", page);
        Assert.Contains("공개 읽기 방식", page);
        Assert.Contains("ssalddel.unity-review.auth.v1", auth);
        Assert.Contains("ssalddel.unity-review.composition-review.offline.v1", offlineStore);
        Assert.DoesNotContain("localStorage", client);
        Assert.Contains("AuthenticationHeaderValue", client);
    }

    [Fact]
    public void 전용WebApp은_화면상태와_전송과_브라우저저장을_분리한다()
    {
        var page = Read(
            "Ssalddel.Web.UnityReviewApp/Pages/Synty공간조립Web검토Page.razor");
        var pageCodeBehind = Read(
            "Ssalddel.Web.UnityReviewApp/Pages/Synty공간조립Web검토Page.razor.cs");
        var workspace = Read(
            "Ssalddel.Web.UnityReviewApp/Services/Synty공간조립검토Workspace.cs");
        var client = Read(
            "Ssalddel.Web.UnityReviewApp/Services/Synty공간조립모바일검토Client.cs");
        var offlineStore = Read(
            "Ssalddel.Web.UnityReviewApp/Services/Synty공간조립오프라인검토Store.cs");

        Assert.DoesNotContain("@code", page);
        Assert.Contains("Workspace.", page);
        Assert.Contains("AuthSession.Changed", pageCodeBehind);
        Assert.Contains("ISynty공간조립모바일검토Client", workspace);
        Assert.Contains("HttpClient", client);
        Assert.DoesNotContain("IJSRuntime", client);
        Assert.Contains("IJSRuntime", offlineStore);
    }

    private static string Read(string relativePath)
        => File.ReadAllText(PathAt(relativePath));

    private static string PathAt(string relativePath)
        => Path.Combine(
            FindRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar));

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
