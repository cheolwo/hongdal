using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 배달권기반배차조율계획ServiceTests
{
    [Fact]
    public async Task 계획은_주배달권_의뢰와_주변배달권_기사를_묶어_Factory입력대상을_만든다()
    {
        var store = new InMemory국내화물배달권실행공간Store();
        await store.Upsert운송의뢰Async(
            "bjd-sigungu:11260",
            "REQ-JN-1",
            ["bjd-sigungu:11215", "bjd-sigungu:11230"]);
        await store.Upsert기사Async("bjd-sigungu:11260", "DRV-JN", ["bjd-sigungu:11215"]);
        await store.Upsert기사Async("bjd-sigungu:11215", "DRV-GJ", ["bjd-sigungu:11260"]);
        await store.Upsert기사Async("bjd-sigungu:11680", "DRV-GN", []);

        var service = new 배달권기반배차조율계획Service(store);

        var 계획목록 = await service.계획Async(new 배달권기반배차조율요청());

        var 계획 = Assert.Single(계획목록);
        Assert.Equal("bjd-sigungu:11260", 계획.배달권키);
        Assert.Equal(["REQ-JN-1"], 계획.의뢰Ids);
        Assert.Contains("DRV-JN", 계획.기사Ids);
        Assert.Contains("DRV-GJ", 계획.기사Ids);
        Assert.DoesNotContain("DRV-GN", 계획.기사Ids);
    }

    [Fact]
    public async Task 계획은_인접배달권_기사포함을_끄면_주배달권_기사만_쓴다()
    {
        var store = new InMemory국내화물배달권실행공간Store();
        await store.Upsert운송의뢰Async("bjd-sigungu:11260", "REQ-JN-1", ["bjd-sigungu:11215"]);
        await store.Upsert기사Async("bjd-sigungu:11260", "DRV-JN", ["bjd-sigungu:11215"]);
        await store.Upsert기사Async("bjd-sigungu:11215", "DRV-GJ", ["bjd-sigungu:11260"]);

        var service = new 배달권기반배차조율계획Service(store);

        var 계획목록 = await service.계획Async(new 배달권기반배차조율요청
        {
            인접배달권기사포함 = false
        });

        var 계획 = Assert.Single(계획목록);
        Assert.Equal(["DRV-JN"], 계획.기사Ids);
        Assert.Empty(계획.인접배달권Keys);
    }

    [Fact]
    public async Task 실행은_여러_배달권에서_같은_기사가_후보가_되어도_추천잠금수를_넘기지_않는다()
    {
        var store = new InMemory국내화물배달권실행공간Store();
        await store.Upsert운송의뢰Async("scope-a", "REQ-A", ["scope-driver"]);
        await store.Upsert운송의뢰Async("scope-b", "REQ-B", ["scope-driver"]);
        await store.Upsert기사Async("scope-driver", "DRV-1", ["scope-a", "scope-b"]);
        var 계획Service = new 배달권기반배차조율계획Service(store);
        var 조율실행Service = new Fake국내화물배차조율실행Service();
        var 실행Service = new 배달권기반배차조율실행Service(계획Service, 조율실행Service);

        var 결과목록 = await 실행Service.실행Async(new 배달권기반배차조율요청
        {
            기사당최대추천건수 = 1
        });

        Assert.Single(결과목록);
        Assert.Single(조율실행Service.요청목록);
        Assert.Equal(["DRV-1"], 조율실행Service.요청목록[0].기사Ids);
    }

    private sealed class Fake국내화물배차조율실행Service : I국내화물배차조율실행Service
    {
        public List<국내화물배차조율입력요청> 요청목록 { get; } = [];

        public Task<(국내화물배차조율입력 Input, 국내화물배차조율결과 Result, 국내화물배차조율적용결과 ApplyResult)> 실행Async(
            국내화물배차조율입력요청 request,
            CancellationToken cancellationToken = default)
        {
            요청목록.Add(request);
            var 의뢰Id = request.의뢰Ids?.FirstOrDefault() ?? "REQ-EMPTY";
            var 기사Id = request.기사Ids?.FirstOrDefault() ?? "DRV-EMPTY";
            var input = new 국내화물배차조율입력(DateTime.UtcNow, request.기사당최대추천건수, [], [], []);
            var result = new 국내화물배차조율결과(
                DateTime.UtcNow,
                [new 국내화물배차제안(1, 의뢰Id, 기사Id, 1, 100m, null, null, null, "테스트 추천", [])],
                [],
                [],
                null,
                null,
                null);
            var applyResult = new 국내화물배차조율적용결과(
                DateTime.UtcNow,
                [new 국내화물배차추천잠금(의뢰Id, 기사Id, 1, DateTime.UtcNow, DateTime.UtcNow.AddSeconds(60))],
                []);

            return Task.FromResult((input, result, applyResult));
        }
    }
}
