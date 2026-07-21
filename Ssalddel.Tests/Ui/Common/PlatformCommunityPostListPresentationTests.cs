using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Ui.Common.Areas.App.Components.Community;

namespace Ssalddel.Tests.Ui.Common;

public sealed class PlatformCommunityPostListPresentationTests
{
    [Fact]
    public void 영문_표시는_게시판과_필터의_공개_이름만_번역한다()
    {
        Assert.Equal(
            "Life & Community",
            PlatformCommunityPostListPresentation.DisplayBoardName(
                DisplayLanguageCodes.English,
                PlatformCommunityPostCategories.General));
        Assert.Equal(
            "Recommended",
            PlatformCommunityPostListPresentation.DisplayFilter(
                DisplayLanguageCodes.English,
                "추천글"));
        Assert.Equal(
            "사용자 게시판",
            PlatformCommunityPostListPresentation.DisplayBoardName(
                DisplayLanguageCodes.English,
                "사용자 게시판"));
    }

    [Fact]
    public void 선택_공지_자동글_참여모집_상태는_한_행_class에_함께_표현된다()
    {
        var post = new PlatformCommunityPostResponse
        {
            Id = 73,
            IsOperatorPinned = true,
            IsSystemGenerated = true,
            IsCommunityMomentumPromoted = true
        };

        var cssClass = PlatformCommunityPostListPresentation.BuildPostRowClass(post, 73);

        Assert.Contains("platform-community-forum-row--selected", cssClass);
        Assert.Contains("platform-community-forum-row--notice", cssClass);
        Assert.Contains("platform-community-forum-row--completion", cssClass);
        Assert.Contains("platform-community-forum-row--momentum", cssClass);
    }

    [Fact]
    public void 신고글은_익명으로_표시하고_활동국가를_숨긴다()
    {
        var post = new PlatformCommunityPostResponse
        {
            Category = "신고/분쟁",
            Nickname = "원래 닉네임",
            IsAuthorDisplayCountryPublic = true,
            AuthorDisplayCountryCode = "KR",
            AuthorDisplayCountryName = "대한민국"
        };

        Assert.Equal("익명 신고자", PlatformCommunityPostListPresentation.DisplayPostNickname(post));
        Assert.False(PlatformCommunityPostListPresentation.HasPublicAuthorCountry(post));
        Assert.Empty(PlatformCommunityPostListPresentation.FormatPostCountryInline(post));
    }

    [Theory]
    [InlineData("https://youtu.be/example", true)]
    [InlineData("https://www.youtube.com/watch?v=example", true)]
    [InlineData("https://example.test/video", false)]
    [InlineData("javascript:alert(1)", false)]
    public void 음식_발견_영상_표시는_YouTube_절대_URL만_허용한다(string link, bool expected)
    {
        var post = new PlatformCommunityPostResponse
        {
            Title = "[음식 발견] 골목 식당",
            SharedLinkUrl = link
        };

        Assert.Equal(expected, PlatformCommunityPostListPresentation.IsFoodVideoPost(post));
    }

    [Fact]
    public void 판매_가격은_원화와_외화를_서로_다른_표기로_유지한다()
    {
        Assert.Equal(
            "12,500원",
            PlatformCommunityPostListPresentation.FormatSalesPrice(
                new PlatformCommunityPostSalesOfferResponse
                {
                    CurrencyCode = "KRW",
                    UnitPrice = 12500
                }));
        Assert.Equal(
            "USD 12.50",
            PlatformCommunityPostListPresentation.FormatSalesPrice(
                new PlatformCommunityPostSalesOfferResponse
                {
                    CurrencyCode = "usd",
                    UnitPrice = 12.5m
                }));
    }
}
