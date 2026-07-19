using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 배달권실행공간StoreTests
{
    [Fact]
    public async Task Store는_배달권별로_기사와_운송의뢰를_분리해서_보관한다()
    {
        var store = new InMemory배달권실행공간Store();

        await store.Upsert기사Async("bjd-sigungu:11260", "DRV-1", ["bjd-sigungu:11215"]);
        await store.Upsert운송의뢰Async("bjd-sigungu:11260", "REQ-1", ["bjd-sigungu:11215"]);
        await store.Upsert기사Async("bjd-sigungu:11680", "DRV-2", []);

        var 중랑구공간 = await store.GetAsync("bjd-sigungu:11260");
        var 강남구공간 = await store.GetAsync("bjd-sigungu:11680");

        Assert.NotNull(중랑구공간);
        Assert.Contains("DRV-1", 중랑구공간.운행중기사Ids);
        Assert.Contains("REQ-1", 중랑구공간.미처리운송의뢰Ids);
        Assert.Contains("bjd-sigungu:11215", 중랑구공간.인접배달권Keys);
        Assert.NotNull(강남구공간);
        Assert.Contains("DRV-2", 강남구공간.운행중기사Ids);
        Assert.DoesNotContain("DRV-2", 중랑구공간.운행중기사Ids);
    }

    [Fact]
    public async Task Store는_기사와_의뢰가_배달권을_옮기면_이전_공간에서_제거한다()
    {
        var store = new InMemory배달권실행공간Store();

        await store.Upsert기사Async("bjd-sigungu:11260", "DRV-1", []);
        await store.Upsert기사Async("bjd-sigungu:11215", "DRV-1", []);
        await store.Upsert운송의뢰Async("bjd-sigungu:11260", "REQ-1", []);
        await store.Upsert운송의뢰Async("bjd-sigungu:11215", "REQ-1", []);

        var 이전공간 = await store.GetAsync("bjd-sigungu:11260");
        var 현재공간 = await store.GetAsync("bjd-sigungu:11215");

        Assert.NotNull(이전공간);
        Assert.DoesNotContain("DRV-1", 이전공간.운행중기사Ids);
        Assert.DoesNotContain("REQ-1", 이전공간.미처리운송의뢰Ids);
        Assert.NotNull(현재공간);
        Assert.Contains("DRV-1", 현재공간.운행중기사Ids);
        Assert.Contains("REQ-1", 현재공간.미처리운송의뢰Ids);
    }
}
