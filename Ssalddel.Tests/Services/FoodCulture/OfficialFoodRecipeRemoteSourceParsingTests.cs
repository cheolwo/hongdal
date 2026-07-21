using System.Text.Json;
using System.Xml.Linq;
using Ssalddel.Services.FoodCulture;

namespace Ssalddel.Tests.Services.FoodCulture;

public sealed class OfficialFoodRecipeRemoteSourceParsingTests
{
    [Fact]
    public void 식약처_COOKRCP01_JSON을_구조화한다()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "RCP_SEQ": "42",
              "RCP_NM": "두부 김치",
              "RCP_WAY2": "볶기",
              "RCP_PAT2": "반찬",
              "INFO_WGT": "200",
              "INFO_ENG": "315",
              "INFO_NA": "430",
              "HASH_TAG": "저염",
              "RCP_PARTS_DTLS": "두부 1모\n배추김치 200g",
              "MANUAL01": "두부를 데친다.",
              "MANUAL02": "김치를 볶는다.",
              "ATT_FILE_NO_MAIN": "https://example.test/main.jpg",
              "RCP_NA_TIP": "소금은 더하지 않는다.",
              "CHNG_DT": "20260720"
            }
            """);

        var result = MfdsCookRecipeRemoteSource.ToRecord(document.RootElement);

        Assert.NotNull(result);
        Assert.Equal("42", result.ExternalId);
        Assert.Equal("두부 김치", result.Name);
        Assert.Equal(["두부 1모", "배추김치 200g"], result.Ingredients);
        Assert.Equal(2, result.Instructions.Count);
        Assert.Equal("315", result.Nutrition["energy_kcal"]);
        Assert.Equal("430", result.Nutrition["sodium_mg"]);
        Assert.Equal(new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), result.SourceModifiedAtUtc);
    }

    [Fact]
    public void 농사로_목록과_상세_XML을_향토음식으로_구조화한다()
    {
        var list = XElement.Parse(
            """
            <item>
              <cntntsNo>91511</cntntsNo>
              <trditfdNm>가례불고기</trditfdNm>
              <atptCodeNm>경상남도</atptCodeNm>
              <foodTyCodeFullname>부식류 &gt; 구이류</foodTyCodeFullname>
              <ckryCodeFullname>가열하여 굽는 음식</ckryCodeFullname>
              <rtnImgSeCode>209006</rtnImgSeCode>
              <rtnFileCours>cms_contents/food</rtnFileCours>
              <rtnThumbFileNm>91511.jpg</rtnThumbFileNm>
            </item>
            """);
        var detailDocument = XDocument.Parse(
            """
            <response><body><items><item>
              <trditfdNm>가례불고기</trditfdNm>
              <fdmtInfo>쇠고기 600g</fdmtInfo>
              <asstnMatrlInfo>간장 3큰술</asstnMatrlInfo>
              <stdCkryDtl>1. 양념한다.
              2. 석쇠에 굽는다.</stdCkryDtl>
              <originDtl>경남 지역의 잔치 음식이다.</originDtl>
              <referMatterDtl>센 불에 짧게 굽는다.</referMatterDtl>
              <infoOfferInfo>경상남도</infoOfferInfo>
            </item></items></body></response>
            """);

        var result = RdaLocalFoodRemoteSource.ToRecord(
            list,
            detailDocument.Descendants("item").Single(),
            detailDocument);

        Assert.NotNull(result);
        Assert.Equal("91511", result.ExternalId);
        Assert.Equal("가례불고기", result.Name);
        Assert.Contains(result.Ingredients, value => value.StartsWith("[주재료]", StringComparison.Ordinal));
        Assert.Equal(2, result.Instructions.Count);
        Assert.Equal("경상남도", result.RegionName);
        Assert.Equal("https://www.nongsaro.go.kr/cms_contents/food/91511.jpg", result.ImageReferenceUrl);
    }

    [Fact]
    public void 일본_MAFF_HTML에서_지역과_레시피를_분리한다()
    {
        const string html = """
            <h2 class="tit06 -prefecture"><span class="pref"><span class="gunma">Gunma</span> Prefecture</span></h2>
            <h2 class="tit06"><span class="name">Katemeshi</span></h2>
            <h2><img src="../../assets/katemeshi.jpg" alt="Katemeshi"></h2>
            <h3 class="tit06"><span>History/origin/related events</span></h3><p class="mt10">A rice dish born during scarcity.</p>
            <h3 class="tit06"><span>Main lore areas</span></h3><p class="mt10">The entire prefecture</p>
            <h3 class="tit06"><span>Main ingredients used</span></h3><p class="mt10">Rice and vegetables</p>
            <h4>Ingredients <span>(for 10 people)</span></h4>
            <div class="material-list"><ul><li><span class="name">rice</span> <span class="quantity">750g</span></li></ul></div>
            <h4>How to cook</h4><div class="howto-list"><ul><li><p>1. Cook the rice.</p></li></ul></div>
            <div class="recipe-notes"><p>provider: local food culture association</p></div>
            """;
        var page = new MaffRegionalCuisineRemoteSource.FetchedHtmlPage(
            html,
            new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc));

        var result = MaffRegionalCuisineRemoteSource.ParseRecipe(
            new Uri("https://www.maff.go.jp/e/policies/market/k_ryouri/search_menu/3022/index.html"),
            page);

        Assert.NotNull(result);
        Assert.Equal("3022", result.ExternalId);
        Assert.Equal("Katemeshi", result.Name);
        Assert.Equal("Gunma", result.RegionName);
        Assert.Equal(["rice 750g"], result.Ingredients);
        Assert.Equal(["1. Cook the rice."], result.Instructions);
        Assert.EndsWith("/e/policies/market/k_ryouri/assets/katemeshi.jpg", result.ImageReferenceUrl);
    }

    [Fact]
    public void NHS_JSON_LD와_화면_단계를_구조화하고_만료시각을_둔다()
    {
        const string html = """
            <div class="bh-recipe__description"><p>Easy family meal.</p><p>Prep: 10 mins<br/>Makes 4 bowls</p></div>
            <span>Nutritional information</span><div class="nhsuk-details__text"><p>Per bowl:</p><ul><li>120kcal</li><li>1g salt</li></ul></div>
            <h2>Method</h2><ol><li><p>Mix the ingredients.</p></li><li><p>Cook for 10 minutes.</p></li></ol>
            <script type="application/ld+json">
            {
              "@context":"https://schema.org",
              "@type":"Recipe",
              "name":"Vegetable bowl recipe",
              "description":"Easy family meal.",
              "recipeIngredient":["1 carrot", "100g beans"],
              "recipeInstructions":"Mix and cook."
            }
            </script>
            """;
        var expiresAt = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc);
        var page = new NhsHealthierFamiliesRecipeRemoteSource.FetchedHtmlPage(html, null);

        var result = NhsHealthierFamiliesRecipeRemoteSource.ParseRecipe(
            new Uri("https://www.nhs.uk/healthier-families/recipes/vegetable-bowl/"),
            page,
            expiresAt);

        Assert.NotNull(result);
        Assert.Equal("vegetable-bowl", result.ExternalId);
        Assert.Equal(2, result.Ingredients.Count);
        Assert.Equal(2, result.Instructions.Count);
        Assert.Equal("Per bowl", result.Nutrition["basis"]);
        Assert.Equal("120kcal", result.Nutrition["item_1"]);
        Assert.Equal("Makes 4 bowls", result.ServingText);
        Assert.Equal(expiresAt, result.ContentExpiresAtUtc);
        Assert.Empty(result.ImageReferenceUrl);
    }
}
