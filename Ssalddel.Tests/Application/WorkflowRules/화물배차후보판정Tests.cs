using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;
using 살뜰.Services.Dispatch.Queue;

namespace Ssalddel.Tests.Application.WorkflowRules;

public sealed class 화물배차후보판정Tests
{
    [Fact]
    public void 용량부족과오래된위치는차단하고_적격트럭을추천한다()
    {
        var result = 화물배차후보선정Policy.판정(Request());

        Assert.Equal("carrier-candidate:sim.truck-1", result.추천후보StableId);
        Assert.Equal(1, result.적격후보수);
        Assert.Equal("freight-dispatch-candidate.v1", result.RuleRevision);
        Assert.Contains(result.후보평가목록.Single(value =>
                value.후보StableId == "carrier-candidate:sim.van-1").차단사유코드목록,
            value => value == 화물배차후보차단사유코드.차량용량부족);
        Assert.Contains(result.후보평가목록.Single(value =>
                value.후보StableId == "carrier-candidate:sim.truck-stale").차단사유코드목록,
            value => value == 화물배차후보차단사유코드.위치정보오래됨);
        var selected = result.후보평가목록.Single(value => value.적격여부);
        Assert.Equal(1, selected.추천순위);
        Assert.Equal(9m, selected.기사대기보정점수);
    }

    [Fact]
    public void 운영기사대기정책은_공통순수규칙과같은점수를사용한다()
    {
        var now = new DateTime(2026, 8, 12, 12, 0, 0, DateTimeKind.Utc);

        var operational = 기사대기Aging점수정책.계산(now.AddMinutes(-95), now);
        var shared = 화물배차기사대기점수Policy.계산(95m);

        Assert.Equal(shared, operational);
        Assert.Equal(9m, operational);
    }

    [Fact]
    public void 동일점수는거리와StableId순서로결정해_재현가능하다()
    {
        var request = Request();
        request.후보목록 = new[]
        {
            Eligible("carrier-candidate:sim.z", 4m),
            Eligible("carrier-candidate:sim.b", 2m),
            Eligible("carrier-candidate:sim.a", 2m),
        };

        var result = 화물배차후보선정Policy.판정(request);

        Assert.Equal("carrier-candidate:sim.a", result.추천후보StableId);
        Assert.Equal(new[]
        {
            "carrier-candidate:sim.a",
            "carrier-candidate:sim.b",
            "carrier-candidate:sim.z",
        }, result.후보평가목록.Select(value => value.후보StableId));
    }

    private static 화물배차후보선정요청 Request()
        => new()
        {
            화물수량 = 300m,
            화물단위코드 = "KGM",
            위치유효시간분 = 10m,
            기본상차접근반경Km = 5m,
            원거리상차접근최대반경Km = 30m,
            원거리상차평균속도KmH = 40m,
            원거리상차도착여유분 = 10m,
            상차시간창남은분 = 90m,
            후보목록 = new[]
            {
                Candidate("carrier-candidate:sim.van-1", "vehicle:sim.van-1", 200m, 2m, 2m, 30m),
                Candidate("carrier-candidate:sim.truck-stale", "vehicle:sim.truck-stale", 400m, 25m, 2m, 30m),
                Candidate("carrier-candidate:sim.truck-1", "vehicle:sim.truck-1", 400m, 2m, 6m, 90m),
            },
        };

    private static 화물배차후보입력 Eligible(string id, decimal distance)
        => Candidate(id, "vehicle:" + id, 400m, 2m, distance, 0m);

    private static 화물배차후보입력 Candidate(
        string id,
        string vehicle,
        decimal capacity,
        decimal locationAge,
        decimal distance,
        decimal waiting)
        => new()
        {
            후보StableId = id,
            차량StableId = vehicle,
            화물운송앱여부 = true,
            차량활성여부 = true,
            기사운행중여부 = true,
            위치경과분 = locationAge,
            상차거리Km = distance,
            상차접근허용반경Km = 30m,
            차량용량 = capacity,
            차량용량단위코드 = "KGM",
            차량적합여부 = true,
            기사대기분 = waiting,
            기본추천사유 = "가상 화물 후보",
            추천점수요청 = new 화물배차추천점수요청
            {
                경로기준거리Km = distance,
                추천유형 = "single",
            },
        };
}
