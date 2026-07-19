using Ssalddel.Services.Content;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Content;

public sealed class HongikHakdangCardPageParserTests
{
    [Fact]
    public void Parse_CollectsEveryGalleryAndPreservesCollectionMembership()
    {
        var parser = CreateParser();
        const string html = """
            <div class="_widget_data" data-widget-type="text">
              <span style="font-size: 22px;"><strong>첫 번째 묶음</strong></span>
            </div>
            <div id="container_gallery-one" class="img_rendering gallery2">
              <div class="_item item_gallary" data-org="S2019/card-one.jpg">
                <div id="caption_1" style="display:none">
                  <h4>양심 &amp; 실천</h4><p>관련 강의 https://youtu.be/example</p>
                </div>
                <div data-src="https://cdn.imweb.me/thumbnail/20240101/card-one.jpg"></div>
              </div>
            </div>
            <div class="_widget_data" data-widget-type="text">
              <span style="font-size: 22px;"><strong>두 번째 묶음</strong></span>
            </div>
            <div id="container_gallery-two" class="img_rendering slide gallery2">
              <div class="_item item_gallary" data-org="S2019/card-one.jpg">
                <div id="caption_2" style="display:none"><h4></h4><p></p></div>
                <div data-src="https://cdn.imweb.me/thumbnail/20240101/card-one-small.jpg"></div>
              </div>
              <div class="_item item_gallary _item_hide" data-org="S2019/card-two.png">
                <div id="caption_3" style="display:none"><h4>두 번째 카드</h4><p>설명</p></div>
                <div data-src="https://cdn.imweb.me/thumbnail/20240101/card-two.png"></div>
              </div>
            </div>
            """;

        var collections = parser.Parse(html);

        Assert.Equal(2, collections.Count);
        Assert.Equal("gallery-one", collections[0].SourceKey);
        Assert.Equal("첫 번째 묶음", collections[0].Name);
        Assert.Single(collections[0].Cards);
        Assert.Equal("양심 & 실천", collections[0].Cards[0].Title);
        Assert.Equal("https://youtu.be/example", collections[0].Cards[0].RelatedUrl);
        Assert.Equal(
            "https://cdn.imweb.me/upload/S2019/card-one.jpg",
            collections[0].Cards[0].OriginalImageUrl);

        Assert.Equal("두 번째 묶음", collections[1].Name);
        Assert.Equal(2, collections[1].Cards.Count);
        Assert.Equal("S2019/card-one.jpg", collections[1].Cards[0].SourceKey);
        Assert.Equal("S2019/card-two.png", collections[1].Cards[1].SourceKey);
    }

    [Fact]
    public void Parse_RejectsUnexpectedOriginalImagePath()
    {
        var parser = CreateParser();
        const string html = """
            <div id="container_gallery" class="gallery2">
              <div class="_item item_gallary" data-org="../private/card.jpg">
                <div id="caption_1" style="display:none"><h4></h4><p></p></div>
              </div>
            </div>
            """;

        Assert.Throws<InvalidOperationException>(() => parser.Parse(html));
    }

    private static HongikHakdangCardPageParser CreateParser()
        => new(Options.Create(new HongikHakdangCardOptions()));
}
