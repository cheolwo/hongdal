using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityDisplayCountryCatalogTests
{
    [Fact]
    public void 공통국가목록은_Wasm문화권자료가부족해도_모두초기화된다()
    {
        Assert.Equal(11, CommunityDisplayCountryCatalog.Common.Count);

        var france = Assert.Single(
            CommunityDisplayCountryCatalog.Common,
            country => country.Code == "FR");

        Assert.Equal("프랑스", france.KoreanName);
        Assert.Equal("France", france.EnglishName);
    }
}
