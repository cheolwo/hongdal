using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Recommendation;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 음식배달권정책Tests
{
    [Fact]
    public void 음식배달공간은_주소보다_현재좌표를_우선한다()
    {
        var result = 음식배달권정책.판정(
            new 배차경로좌표(37.5665m, 126.9780m),
            "부산광역시 중구");

        Assert.StartsWith("food-cell:v1:", result.배달권키, StringComparison.Ordinal);
        Assert.Equal("food-cell-v1", result.판정방식);
    }

    [Fact]
    public void Food셀은_자기자신을_제외한_여덟개_인접셀을_가진다()
    {
        var primary = 음식배달권정책.판정(
            new 배차경로좌표(37.5665m, 126.9780m),
            null);

        var adjacent = 음식배달권정책.인접배달권키조회(primary.배달권키);

        Assert.Equal(8, adjacent.Count);
        Assert.All(adjacent, key => Assert.StartsWith("food-cell:v1:", key, StringComparison.Ordinal));
        Assert.DoesNotContain(primary.배달권키, adjacent);
    }

    [Fact]
    public void 제한거리확장은_동일셀과_인접셀을_제외한_Food셀만_반환한다()
    {
        var primary = 음식배달권정책.판정(
            new 배차경로좌표(37.5m, 127m),
            null);
        var adjacent = 음식배달권정책.인접배달권키조회(primary.배달권키);

        var expanded = 음식배달권정책.거리확장배달권키조회(primary.배달권키, 5m);

        Assert.NotEmpty(expanded);
        Assert.All(expanded, key => Assert.StartsWith("food-cell:v1:", key, StringComparison.Ordinal));
        Assert.DoesNotContain(primary.배달권키, expanded);
        Assert.DoesNotContain(expanded, key => adjacent.Contains(key, StringComparer.Ordinal));
    }

    [Fact]
    public void 좌표가_없을때도_화물키가_아닌_Food_fallback키를_만든다()
    {
        var result = 음식배달권정책.판정(null, "서울특별시 중랑구");

        Assert.StartsWith("food-scope:v1:", result.배달권키, StringComparison.Ordinal);
        Assert.Contains("food-cell-v1", result.판정방식, StringComparison.Ordinal);
    }
}
