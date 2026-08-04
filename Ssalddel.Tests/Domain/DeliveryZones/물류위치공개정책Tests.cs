using Ssalddel.Contracts.Common.DeliveryZones;
using 살뜰.도메인.배달권;

namespace Ssalddel.Tests.Domain.DeliveryZones;

public sealed class 물류위치공개정책Tests
{
    [Fact]
    public void 공개권역은_대표점을_격자화하고_주소와_정밀좌표를_노출하지않는다()
    {
        var result = 물류위치공개정책.공개권역만들기(new 물류위치원본(
            "transport:1:pickup",
            "KR",
            "11",
            "11680",
            "서울 강남 권역",
            물류위치공개정밀도코드.시군구,
            37.50123m,
            127.03987m,
            "업무 원장 확인",
            DateTimeOffset.UtcNow));

        Assert.Equal(37.5m, result.공개대표위도);
        Assert.Equal(127.0m, result.공개대표경도);
        Assert.DoesNotContain("주소", string.Join(',', typeof(공개물류권역Dto).GetProperties().Select(x => x.Name)));
        Assert.DoesNotContain("정밀위도", string.Join(',', typeof(공개물류권역Dto).GetProperties().Select(x => x.Name)));
    }

    [Fact]
    public void 좌표쌍이_불완전하거나_범위를벗어나면_공개대표점을_생략한다()
    {
        var result = 물류위치공개정책.공개권역만들기(new 물류위치원본(
            "warehouse:1",
            "KR",
            "11",
            null,
            "서울 권역",
            "unsupported",
            91m,
            127m,
            "공식 권역 자료",
            null));

        Assert.Equal(물류위치공개정밀도코드.국가, result.정밀도코드);
        Assert.Null(result.공개대표위도);
        Assert.Null(result.공개대표경도);
    }

    [Fact]
    public void 정밀위치는_명시적_업무권한과_유효기간이_있을때만허용한다()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.False(물류위치공개정책.정밀위치허용(
            물류위치공개범위코드.공개권역,
            true,
            now,
            now.AddHours(1)));
        Assert.True(물류위치공개정책.정밀위치허용(
            물류위치공개범위코드.참여자정밀,
            true,
            now,
            now.AddHours(1)));
        Assert.False(물류위치공개정책.정밀위치허용(
            물류위치공개범위코드.운영자정밀,
            true,
            now,
            now.AddMinutes(-1)));
    }
}
