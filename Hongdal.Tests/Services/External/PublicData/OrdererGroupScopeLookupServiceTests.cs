using Hongdal.Contracts.Common.PublicData;
using 홍달.Services.External.PublicData;

namespace Hongdal.Tests.Services.External.PublicData;

public sealed class OrdererGroupScopeLookupServiceTests
{
    [Fact]
    public void FindCandidates_UsesKakaoLevel2AsDefaultScope()
    {
        var service = new OrdererGroupScopeLookupService();

        var result = service.FindCandidates(new OrdererGroupScopeLookupRequest
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
    public void FindCandidates_ParsesRoadAddressLevel2BeforeDetailedAddress()
    {
        var service = new OrdererGroupScopeLookupService();

        var result = service.FindCandidates(new OrdererGroupScopeLookupRequest
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
    public void FindCandidates_TreatsCityDistrictPairAsLevel2()
    {
        var service = new OrdererGroupScopeLookupService();

        var result = service.FindCandidates(new OrdererGroupScopeLookupRequest
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
