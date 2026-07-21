using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.WebApp.Models;

public static class CommunityPersonalPresentation
{
    public static string FormatDate(DateTime value)
        => value.ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    public static string ResolveDecorationTargetLabel(CommunityDecorationTarget target)
        => target switch
        {
            CommunityDecorationTarget.HomeNavigatorTheme => "홈 · 게시판 테마",
            CommunityDecorationTarget.TraditionalMarketTheme => "전통시장 꾸미기 팩",
            CommunityDecorationTarget.Bagua => "사방 이동판",
            CommunityDecorationTarget.DiagramNode => "다이어그램 도형",
            CommunityDecorationTarget.BaguaTransitionMotion => "이동 효과",
            _ => "꾸미기"
        };

    public static string ResolveProductSymbol(CommunityDecorationProduct product)
        => product.Assets.FirstOrDefault()?.PreviewSymbol
           ?? product.HomeTheme?.ClosedHandle.Title
           ?? product.TraditionalMarketTheme?.MarketDayBanner.Title
           ?? product.BaguaMotion?.PreviewSymbol
           ?? "◇";

    public static string ResolveProductAccent(CommunityDecorationProduct product)
        => product.Assets.FirstOrDefault()?.AccentColor
           ?? product.HomeTheme?.AccentColor
           ?? product.TraditionalMarketTheme?.AccentColor
           ?? product.BaguaMotion?.AccentColor
           ?? "#147d64";

    public static string BuildDecorationProductClass(
        CommunityDecorationProduct product,
        bool isActive)
    {
        var classes = new List<string> { "community-decoration-product" };
        if (product.IsTraditionalMarketTheme)
        {
            classes.Add("community-decoration-product--market");
        }

        if (isActive)
        {
            classes.Add("community-decoration-product--active");
        }

        return string.Join(' ', classes);
    }
}
