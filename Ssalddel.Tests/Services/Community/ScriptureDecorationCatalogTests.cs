using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class ScriptureDecorationCatalogTests
{
    [Theory]
    [InlineData("PLaNHcYq59k3yLJ1M0JsZrSvrqvGcAoozE", "홍익학당 [주역] 8괘편", "i-ching")]
    [InlineData("PLaNHcYq59k3yO3Rmf5m-uVz9ECeIMr3M0", "홍익학당 [불교철학] 반야심경과 금강경", "prajna-diamond")]
    [InlineData("PLaNHcYq59k3whmh8PASsf1EynZNqE29hJ", "윤홍식의 [유교 철학의 핵심 대학과 중용] 강의", "great-learning-mean")]
    [InlineData("PLaNHcYq59k3z20Pz5aep8psvaJRveONLG", "홍익학당 [불교철학] 대승기신론", "awakening-faith")]
    [InlineData("PLaNHcYq59k3ymfQ8O1RfDfsge1CWEg7uy", "홍익학당 [동양철학] 맹자 강의", "mencius")]
    [InlineData("PLaNHcYq59k3yhMQKJodQfhDPa12PYb6ve", "홍익학당 [동양철학] 주역 강의", "i-ching")]
    [InlineData("PLaNHcYq59k3ya80BT2WpzWXh7bjbVwhk4", "윤홍식의 [유마경] 강의", "vimalakirti")]
    [InlineData("PLaNHcYq59k3ymuHX8Mr3ZE1DG9pRkSm88", "윤홍식의 [유식학] 강의", "yogacara")]
    [InlineData("PLaNHcYq59k3wFKLb_mRlB7Y7VsH_a1c8g", "윤홍식의 [법구경] 강의", "dhammapada")]
    [InlineData("PLaNHcYq59k3zpnpmSV1sZcca6ucULttgq", "홍익학당 [동양철학] 장자 강의", "zhuangzi")]
    [InlineData("PLaNHcYq59k3xxKP5aBEj_Ju2NBJvPx2oc", "홍익학당 [동양철학] 논어 강의", "analects")]
    [InlineData("PLaNHcYq59k3xOCCCYMZx1HLpQctJbW8Cl", "윤홍식의 [화엄경] 강의", "avatamsaka")]
    public void 현재공개재생목록_경전고전팩으로_연결된다(
        string playlistId,
        string title,
        string expectedKey)
    {
        var definition = ScriptureDecorationCatalog.FindByPlaylist(playlistId, title);

        Assert.NotNull(definition);
        Assert.Equal(expectedKey, definition.Key);
    }

    [Fact]
    public void 새로운제목도_경전명이포함되면_관련팩으로_연결된다()
    {
        var definition = ScriptureDecorationCatalog.FindByPlaylist(
            "future-playlist",
            "새로 읽는 금강경 핵심 강의");

        Assert.Equal("prajna-diamond", definition?.Key);
    }

    [Fact]
    public void 경전고전과무관한재생목록은_꾸미기팩을_억지로연결하지않는다()
    {
        var definition = ScriptureDecorationCatalog.FindByPlaylist(
            "unrelated-playlist",
            "홍익학당 생활 속 양심 실천");

        Assert.Null(definition);
    }

    [Fact]
    public void 팩키와상품키는_중복되지않고_각팩에출처가있다()
    {
        var definitions = ScriptureDecorationCatalog.Definitions;
        var products = ScriptureDecorationCatalog.CreateProducts();

        Assert.Equal(11, definitions.Count);
        Assert.Equal(definitions.Count, definitions.Select(item => item.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(products.Count, products.Select(item => item.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(products, product =>
        {
            Assert.True(product.IsFree);
            Assert.True(product.IsHomeTheme);
            Assert.NotNull(product.ScriptureSource);
            Assert.NotEmpty(product.ScriptureSource.Playlists);
            Assert.All(product.ScriptureSource.Playlists, playlist =>
                Assert.StartsWith("https://www.youtube.com/playlist?list=", playlist.Url, StringComparison.Ordinal));
        });
    }
}
