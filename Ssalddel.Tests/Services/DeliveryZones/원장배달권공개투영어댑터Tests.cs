using Ssalddel.Contracts.Common.DeliveryZones;
using 살뜰.Services.DeliveryZones;

namespace Ssalddel.Tests.Services.DeliveryZones;

public sealed class 원장배달권공개투영어댑터Tests
{
    [Fact]
    public void 플랫폼배달권은_공개권역으로_변환되고_정밀도는_이름으로_결정된다()
    {
        var source = new 플랫폼배달권Dto
        {
            배달권키 = "dp-zone-1",
            배달권명 = "강남권 집합권역",
            시도명 = "서울특별시",
            시군구명 = "강남구",
            대표위도 = 37.50123m,
            대표경도 = 127.03987m
        };

        var result = 원장배달권공개투영어댑터.공개권역변환(source, 원장배달권공개투영어댑터.기본국가코드);

        Assert.Equal("platform-delivery-zone:dp-zone-1", result.권역키);
        Assert.Equal(물류위치공개정밀도코드.시군구, result.정밀도코드);
        Assert.Equal(37.5m, result.공개대표위도);
        Assert.Equal(127.0m, result.공개대표경도);
        Assert.Equal("강남권 집합권역", result.표시명);
        Assert.Equal(원장배달권공개투영어댑터.기본노출근거, result.출처명);
    }

    [Fact]
    public void 플랫폼배달권_이름정보없으면_국가권역으로_분류된다()
    {
        var source = new 플랫폼배달권Dto
        {
            배달권키 = "dp-zone-2",
            배달권명 = "미지정권역",
            대표위도 = 37.50123m,
            대표경도 = 127.03987m
        };

        var result = 원장배달권공개투영어댑터.공개권역변환(source, "KR");

        Assert.Equal(물류위치공개정밀도코드.국가, result.정밀도코드);
    }

    [Fact]
    public void 참여자정밀위치는_범위와_권한_및_유효기간을_통과할때만_노출한다()
    {
        var source = new 플랫폼배달권Dto
        {
            배달권키 = "dp-zone-3",
            배달권명 = "권역",
            대표위도 = 37.50123m,
            대표경도 = 127.03987m
        };

        var now = DateTimeOffset.UtcNow;

        Assert.Null(원장배달권공개투영어댑터.참여자정밀위치변환(
            source,
            물류위치공개범위코드.공개권역,
            true,
            now,
            now.AddHours(1)));

        var result = 원장배달권공개투영어댑터.참여자정밀위치변환(
            source,
            물류위치공개범위코드.참여자정밀,
            true,
            now,
            now.AddHours(1));

        Assert.NotNull(result);
        Assert.Equal("platform-delivery-zone:dp-zone-3", result!.위치키);
        Assert.Equal(37.50123m, result.위도);
        Assert.Equal(127.03987m, result.경도);
        Assert.Equal(now, result.유효시각Utc);
        Assert.Equal(now.AddHours(1), result.만료시각Utc);

        Assert.Null(원장배달권공개투영어댑터.참여자정밀위치변환(
            source,
            물류위치공개범위코드.운영자정밀,
            true,
            now,
            now.AddMinutes(-1)));
    }
}

