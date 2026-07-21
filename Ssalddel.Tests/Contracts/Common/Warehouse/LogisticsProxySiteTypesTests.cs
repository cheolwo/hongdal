using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Tests.Contracts.Common.Warehouse;

public sealed class LogisticsProxySiteTypesTests
{
    [Fact]
    public void UrbanLogisticsCenter_IsSupportedAsWarehouseSpecialization()
    {
        var code = LogisticsProxySiteTypes.UrbanLogisticsCenter;

        Assert.True(LogisticsProxySiteTypes.IsValid(code));
        Assert.Equal(code, LogisticsProxySiteTypes.Normalize(code));
        Assert.Equal("도심 생활물류센터", LogisticsProxySiteTypes.GetDisplayName(code));
        Assert.False(LogisticsProxySiteTypes.RequiresCustoms(code));
    }
}
