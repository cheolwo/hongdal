namespace Ssalddel.Tests.Architecture;

public sealed class ApplicationPrivacyConsentCompositionTests
{
    [Fact]
    public void 동의Panel은_필수동의를기본미선택으로두고_두확인뒤에만계속한다()
    {
        var source = ReadRepositoryFile(
            "Ssalddel.WebApp",
            "Components",
            "ApplicationPrivacyConsentPanel.razor");

        Assert.Contains("개인정보 동의 · 필수", source);
        Assert.Contains("수집·이용 목적", source);
        Assert.Contains("보유·이용 기간", source);
        Assert.Contains("거부 권리와 영향", source);
        Assert.Contains("법적 안내 전문", source);
        Assert.Contains("<textarea", source);
        Assert.Contains("readonly", source);
        Assert.Contains("@LegalNoticeText", source);
        Assert.Contains("[제3자 제공]", source);
        Assert.Contains("[국외 이전]", source);
        Assert.Contains("[운영 전 확인]", source);
        Assert.Contains("WebLocalePreferenceService", source);
        Assert.Contains("EnglishLegalNoticeText", source);
        Assert.Contains("FTC Act", source);
        Assert.Contains("COPPA", source);
        Assert.Contains("California Consumer Privacy Act", source);
        Assert.Contains("Selecting English does not determine jurisdiction", source);
        Assert.DoesNotContain("<details>", source);
        Assert.Contains("type=\"checkbox\"", source);
        Assert.Contains("disabled=\"@(!CanContinue || _isSaving)\"", source);
        Assert.Contains("_collectionUseAccepted && _ageRequirementConfirmed", source);
        Assert.Contains("ConsentClient.RecordAsync", source);
        Assert.Contains("신청개인정보동의정책.현재버전", source);
        Assert.Contains("신청개인정보출처Codes.커뮤니티지도", source);
    }

    [Theory]
    [InlineData("ShipperInboundRequestCreatePage.razor", "신청개인정보업무Codes.물류대행")]
    [InlineData("ShipperRequestPage.razor", "신청개인정보업무Codes.운송대행")]
    [InlineData("OrdererMartOrderRequestPage.razor", "신청개인정보업무Codes.개별주문")]
    public void 지도출발신청은_동의전양식을렌더하지않는다(
        string pageName,
        string applicationKind)
    {
        var source = ReadRepositoryFile("Ssalddel.WebApp", "Pages", pageName);

        Assert.Contains("RequiresMapPrivacyConsent && !_privacyConsentAccepted", source);
        Assert.Contains("<ApplicationPrivacyConsentPanel", source);
        Assert.Contains(applicationKind, source);
        Assert.Contains("Accepted=\"AcceptPrivacyAsync\"", source);
        Assert.Contains("CommunityMapApplicationRoutes.SourceCode", source);
        Assert.Contains("_privacyConsentAccepted = true", source);
    }

    [Theory]
    [InlineData("Common", "창고작업Controller.cs", "신청개인정보업무Codes.물류대행")]
    [InlineData("Shipper/01_Request", "화주운송의뢰Controller.cs", "신청개인정보업무Codes.운송대행")]
    [InlineData("Orderer", "마트주문요청Controller.cs", "신청개인정보업무Codes.개별주문")]
    public void 지도출발신청Command는_서버에서유효동의증적을요구한다(
        string controllerFolder,
        string controllerName,
        string applicationKind)
    {
        var source = ReadRepositoryFile(
            ["Ssalddel", "Controllers", .. controllerFolder.Split('/'), controllerName]);

        Assert.Contains("유효한동의요구Async", source);
        Assert.Contains("신청개인정보동의증적Id", source);
        Assert.Contains("신청출처Code", source);
        Assert.Contains(applicationKind, source);
    }

    [Fact]
    public void 지도출발신청은_Main메뉴와운영Bar없는단독화면으로조립한다()
    {
        var layout = ReadRepositoryFile("Ssalddel.WebApp", "Layout", "MainLayout.razor");
        var css = ReadRepositoryFile("Ssalddel.WebApp", "wwwroot", "css", "app.css");

        Assert.Contains("IsMapApplicationStandalone", layout);
        Assert.Contains("CommunityMapApplicationRoutes.UsesStandaloneApplicationLayout", layout);
        Assert.Contains("map-application-standalone__main", layout);
        Assert.Contains("<NavMenu />", layout);
        Assert.Contains("<PreviewPortalBar />", layout);
        Assert.Contains(".map-application-standalone__main", css);
    }

    private static string ReadRepositoryFile(params string[] relativePath)
        => File.ReadAllText(Path.Combine([FindRepositoryRoot(), .. relativePath]));

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
