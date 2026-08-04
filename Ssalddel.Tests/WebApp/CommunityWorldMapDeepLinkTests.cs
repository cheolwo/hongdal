using Ssalddel.WebApp.Services;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityWorldMapDeepLinkTests
{
    private static readonly string[] Countries = ["KR", "US", "CN", "AU"];
    private static readonly string[] Layers = ["culture", "price", "news"];

    [Theory]
    [InlineData(" kr ", "KR")]
    [InlineData("zz", null)]
    [InlineData(null, null)]
    public void 국가코드는_허용목록안에서만정규화한다(string? value, string? expected)
        => Assert.Equal(
            expected,
            CommunityWorldMapDeepLink.NormalizeCountryCode(value, Countries));

    [Fact]
    public void 레이어는_허용된중복없는순서로해석한다()
        => Assert.Equal(
            ["news", "culture"],
            CommunityWorldMapDeepLink.ParseLayerCodes(
                "news,unknown,culture,news",
                Layers));

    [Theory]
    [InlineData("none")]
    [InlineData("")]
    public void 레이어없음은_명시적인빈선택으로해석한다(string value)
        => Assert.Empty(CommunityWorldMapDeepLink.ParseLayerCodes(value, Layers)!);

    [Fact]
    public void 레이어질의가없으면_역할기본값복원을위해Null을유지한다()
        => Assert.Null(CommunityWorldMapDeepLink.ParseLayerCodes(null, Layers));

    [Fact]
    public void 레이어직렬화는_화면카탈로그순서와None계약을지킨다()
    {
        Assert.Equal(
            "culture,news",
            CommunityWorldMapDeepLink.SerializeLayerCodes(
                ["news", "culture"],
                Layers));
        Assert.Equal(
            "none",
            CommunityWorldMapDeepLink.SerializeLayerCodes([], Layers));
    }

    [Fact]
    public void 마커와관측식별자는_공백과제어문자와과도한길이를거부한다()
    {
        Assert.Equal("tourism:1001", CommunityWorldMapDeepLink.NormalizeStableId(" tourism:1001 "));
        Assert.Null(CommunityWorldMapDeepLink.NormalizeStableId("marker\ninvalid"));
        Assert.Null(CommunityWorldMapDeepLink.NormalizeStableId(new string('a', 201)));
    }

    [Theory]
    [InlineData("snapshot-1", "source-1", "snapshot-1", "source-1", CommunityWorldMapDeepLink.SourceVersionMatched)]
    [InlineData("snapshot-1", "source-1", "snapshot-2", "source-1", CommunityWorldMapDeepLink.SourceVersionMatchedSnapshotUpdated)]
    [InlineData("snapshot-1", "source-1", "snapshot-2", "source-2", CommunityWorldMapDeepLink.SourceVersionUpdated)]
    [InlineData("snapshot-1", null, "snapshot-1", null, CommunityWorldMapDeepLink.SnapshotRevisionMatched)]
    [InlineData("snapshot-1", null, "snapshot-2", null, CommunityWorldMapDeepLink.SnapshotRevisionUpdated)]
    [InlineData(null, null, "snapshot-2", "source-2", null)]
    public void 게시당시와현재의Source및SnapshotVersion을구분한다(
        string? requestedSnapshot,
        string? requestedSource,
        string? currentSnapshot,
        string? currentSource,
        string? expected)
        => Assert.Equal(
            expected,
            CommunityWorldMapDeepLink.ResolveEvidenceVersionStatus(
                requestedSnapshot,
                requestedSource,
                currentSnapshot,
                currentSource));
}
