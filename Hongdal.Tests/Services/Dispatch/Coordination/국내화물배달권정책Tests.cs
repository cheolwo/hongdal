using 홍달.Services.Dispatch.Coordination;
using 홍달.Services.Dispatch.Recommendation;

namespace Hongdal.Tests.Services.Dispatch.Coordination;

public sealed class 국내화물배달권정책Tests
{
    [Fact]
    public void 판정은_공공코드_카탈로그에_있는_주소면_시군구_배달권을_우선한다()
    {
        var result = 국내화물배달권정책.판정(new 배차경로좌표(37.51m, 127.02m), "서울특별시 강남구 테헤란로");

        Assert.Equal("bjd-sigungu:11680", result.배달권키);
        Assert.Equal("강남구", result.배달권명);
        Assert.Equal("법정동시군구", result.판정방식);
        Assert.Equal("1168000000", result.법정동코드);
        Assert.Equal("강남구청", result.대표건물명);
        Assert.Equal("서울특별시 강남구 학동로 426", result.대표건물주소);
        Assert.Equal(37.517236m, result.대표위도);
        Assert.Equal(127.047325m, result.대표경도);
    }

    [Fact]
    public void 판정은_서울_중랑구와_도봉구를_서로_다른_배달권으로_만든다()
    {
        var jungnang = 국내화물배달권정책.판정(null, "서울특별시 중랑구 망우로");
        var dobong = 국내화물배달권정책.판정(null, "서울특별시 도봉구 도봉로");

        Assert.Equal("bjd-sigungu:11260", jungnang.배달권키);
        Assert.Equal("중랑구", jungnang.배달권명);
        Assert.Equal("중랑구청", jungnang.대표건물명);
        Assert.Equal("서울특별시 중랑구 봉화산로 179", jungnang.대표건물주소);
        Assert.Equal(37.606560m, jungnang.대표위도);
        Assert.Equal(127.092652m, jungnang.대표경도);
        Assert.Equal("bjd-sigungu:11320", dobong.배달권키);
        Assert.Equal("도봉구", dobong.배달권명);
        Assert.Equal("도봉구청", dobong.대표건물명);
        Assert.Equal("서울특별시 도봉구 마들로 656", dobong.대표건물주소);
        Assert.Equal(37.668691m, dobong.대표위도);
        Assert.Equal(127.047131m, dobong.대표경도);
    }

    [Fact]
    public void 인접배달권은_서울_시군구_지리관계를_기준으로_판정한다()
    {
        var 중랑구 = 국내화물배달권정책.판정(null, "서울특별시 중랑구 망우로");
        var 광진구 = 국내화물배달권정책.판정(null, "서울특별시 광진구 자양로");
        var 동대문구 = 국내화물배달권정책.판정(null, "서울특별시 동대문구 천호대로");
        var 강남구 = 국내화물배달권정책.판정(null, "서울특별시 강남구 테헤란로");

        Assert.True(국내화물배달권정책.인접배달권여부(중랑구, 광진구));
        Assert.True(국내화물배달권정책.인접배달권여부(중랑구, 동대문구));
        Assert.False(국내화물배달권정책.인접배달권여부(중랑구, 강남구));
    }

    [Fact]
    public void 카탈로그는_인접배달권키를_내부판정용으로_제공한다()
    {
        var 인접목록 = 국내행정구역배달권Catalog.인접배달권키조회("bjd-sigungu:11260");

        Assert.Contains("bjd-sigungu:11215", 인접목록);
        Assert.Contains("bjd-sigungu:11230", 인접목록);
        Assert.Contains("bjd-sigungu:11350", 인접목록);
    }

    [Fact]
    public void 카탈로그는_서울_25개_시군구_대표기준점을_제공한다()
    {
        var items = 국내행정구역배달권Catalog.시군구조회();

        Assert.Equal(25, items.Count);
        Assert.All(items, item =>
        {
            Assert.EndsWith("00000", item.법정동코드, StringComparison.Ordinal);
            Assert.EndsWith("구청", item.대표건물명, StringComparison.Ordinal);
            Assert.NotNull(item.시군구명);
            Assert.Contains(item.시군구명!, item.대표건물주소, StringComparison.Ordinal);
            Assert.InRange(item.대표위도, 37m, 38m);
            Assert.InRange(item.대표경도, 126m, 128m);
        });
    }

    [Fact]
    public void 카탈로그는_전국_17개_시도_대표기준점을_제공한다()
    {
        var items = 국내행정구역배달권Catalog.시도조회();

        Assert.Equal(17, items.Count);
        Assert.Contains(items, x => x.법정동코드 == "4100000000" && x.대표건물명 == "경기도청");
        Assert.Contains(items, x => x.법정동코드 == "5000000000" && x.대표건물명 == "제주특별자치도청");
        Assert.All(items, item =>
        {
            Assert.Equal("시도", item.행정계층);
            Assert.StartsWith("bjd-sido:", item.배달권키, StringComparison.Ordinal);
            Assert.NotEqual(0m, item.대표위도);
            Assert.NotEqual(0m, item.대표경도);
        });
    }

    [Fact]
    public void 판정은_서울_밖_주소를_시도_배달권으로_fallback한다()
    {
        var result = 국내화물배달권정책.판정(null, "경기도 수원시 영통구 도청로 30");

        Assert.Equal("bjd-sido:41", result.배달권키);
        Assert.Equal("경기도", result.배달권명);
        Assert.Equal("법정동시도", result.판정방식);
        Assert.Equal("경기도청", result.대표건물명);
        Assert.Equal("경기도 수원시 영통구 도청로 30", result.대표건물주소);
        Assert.Equal(37.289056m, result.대표위도);
        Assert.Equal(127.053503m, result.대표경도);
    }

    [Fact]
    public void 판정은_카탈로그에_없는_주소면_좌표격자로_fallback한다()
    {
        var result = 국내화물배달권정책.판정(new 배차경로좌표(37.51m, 127.02m), "임시 주소");

        Assert.StartsWith("geo:", result.배달권키, StringComparison.Ordinal);
        Assert.Equal("좌표격자", result.판정방식);
    }

    [Fact]
    public void 판정은_좌표도_공공코드도_없으면_주소_앞부분으로_fallback한다()
    {
        var result = 국내화물배달권정책.판정(null, "알수시 알수구 상세주소");

        Assert.Equal("address:알수시-알수구", result.배달권키);
        Assert.Equal("주소", result.판정방식);
    }
}
