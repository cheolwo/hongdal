using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 배달권실행공간StoreTests
{
    [Fact]
    public async Task 국내화물Store는_배달권별로_기사와_운송의뢰를_분리해서_보관한다()
    {
        var store = new InMemory국내화물배달권실행공간Store();

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
    public async Task 국내화물Store는_기사와_의뢰가_배달권을_옮기면_이전_공간에서_제거한다()
    {
        var store = new InMemory국내화물배달권실행공간Store();

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

    [Fact]
    public async Task 음식배달과_국내화물은_각자_소유한_물리공간에만_상태를_보관한다()
    {
        var foodStore = new InMemory음식배달권실행공간Store();
        var cargoStore = new InMemory국내화물배달권실행공간Store();

        await foodStore.Upsert기사Async("food-cell:v1:1500:5080", "FOOD-DRIVER-1", []);
        await foodStore.Upsert운송의뢰Async("food-cell:v1:1500:5080", "FOOD-ORDER-1", []);
        await cargoStore.Upsert기사Async("bjd-sigungu:11260", "CARGO-DRIVER-1", []);
        await cargoStore.Upsert운송의뢰Async("bjd-sigungu:11260", "CARGO-ORDER-1", []);

        var foodSpace = Assert.Single(await foodStore.SnapshotAsync());
        var cargoSpace = Assert.Single(await cargoStore.SnapshotAsync());

        Assert.Equal("dispatch:food:v1", InMemory음식배달권실행공간Store.물리공간식별자);
        Assert.Equal("dispatch:cargo:v1", InMemory국내화물배달권실행공간Store.물리공간식별자);
        Assert.Equal(["FOOD-DRIVER-1"], foodSpace.운행중기사Ids);
        Assert.Equal(["FOOD-ORDER-1"], foodSpace.미처리운송의뢰Ids);
        Assert.Equal(["CARGO-DRIVER-1"], cargoSpace.운행중기사Ids);
        Assert.Equal(["CARGO-ORDER-1"], cargoSpace.미처리운송의뢰Ids);
    }

    [Fact]
    public async Task 음식배달과_국내화물은_서로의_공간키를_받지_않는다()
    {
        var foodStore = new InMemory음식배달권실행공간Store();
        var cargoStore = new InMemory국내화물배달권실행공간Store();

        await Assert.ThrowsAsync<ArgumentException>(
            () => foodStore.Upsert기사Async("bjd-sigungu:11260", "FOOD-DRIVER-1", []));
        await Assert.ThrowsAsync<ArgumentException>(
            () => cargoStore.Upsert기사Async("food-cell:v1:1500:5080", "CARGO-DRIVER-1", []));
    }
}
