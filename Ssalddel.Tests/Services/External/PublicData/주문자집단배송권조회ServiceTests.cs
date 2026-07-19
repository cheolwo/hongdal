using Ssalddel.Contracts.Common.PublicData;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.External.PublicData;

public sealed class 주문자집단배송권조회ServiceTests
{
    [Fact]
    public void 후보검색_UsesKakaoLevel2AsDefaultScope()
    {
        var service = new 주문자집단배송권조회Service();

        var result = service.후보검색(new 주문자집단배송권조회요청
        {
            KakaoRegionLevel1 = "경기도",
            KakaoRegionLevel2 = "수원시 영통구",
            KakaoRegionLevel3 = "이의동"
        });

        Assert.True(result.Success);
        var item = Assert.Single(result.Items, x => x.IsDefaultScope);
        Assert.Equal("RoadAddressLevel2", item.Basis);
        Assert.Equal("경기도", item.RoadAddressLevel1);
        Assert.Equal("수원시 영통구", item.RoadAddressLevel2);
        Assert.True(item.SupportsApartmentSubScope);
    }

    [Fact]
    public void 후보검색_ParsesRoadAddressLevel2BeforeDetailedAddress()
    {
        var service = new 주문자집단배송권조회Service();

        var result = service.후보검색(new 주문자집단배송권조회요청
        {
            RoadAddress = "서울특별시 강남구 테헤란로 123 10층"
        });

        Assert.True(result.Success);
        var item = result.Items[0];
        Assert.Equal("RoadAddressLevel2", item.Basis);
        Assert.Equal("서울특별시", item.RoadAddressLevel1);
        Assert.Equal("강남구", item.RoadAddressLevel2);
        Assert.Equal("테헤란로", item.RoadAddressLevel3);
    }

    [Fact]
    public void 후보검색_TreatsCityDistrictPairAsLevel2()
    {
        var service = new 주문자집단배송권조회Service();

        var result = service.후보검색(new 주문자집단배송권조회요청
        {
            RoadAddress = "경기도 수원시 영통구 센트럴타운로 85"
        });

        Assert.True(result.Success);
        var item = result.Items[0];
        Assert.Equal("경기도", item.RoadAddressLevel1);
        Assert.Equal("수원시 영통구", item.RoadAddressLevel2);
        Assert.Equal("센트럴타운로", item.RoadAddressLevel3);
    }
}
