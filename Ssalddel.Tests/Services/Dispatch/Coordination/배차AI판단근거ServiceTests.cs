using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 배차AI판단근거ServiceTests
{
    [Fact]
    public void 조회는_묶음크기와_목표수익에_맞는_정책근거를_반환한다()
    {
        var service = new 규칙기반배차AI판단근거조회Service();

        var result = service.조회(new 배차AI판단근거요청(
            "국내화물운송OS:플랫폼수익묶음",
            ["REQ-A", "REQ-B"],
            [],
            ["scope-1"],
            묶음크기: 2,
            목표건당플랫폼순이익: 500m,
            키워드: ["플랫폼", "수익", "묶음"]));

        Assert.Contains(result.정책근거목록, x => x.근거Id == "DCT-POLICY-PLATFORM-BUNDLE");
        Assert.Contains(result.정책근거목록, x => x.근거Id == "DCT-POLICY-SCOPE-BUNDLE");
    }

    [Fact]
    public void 조회는_냉장_키워드가_맞는_판단사례를_우선한다()
    {
        var service = new 규칙기반배차AI판단근거조회Service();

        var result = service.조회(new 배차AI판단근거요청(
            "국내화물운송OS:기사배정",
            ["REQ-COLD"],
            ["DRV-A", "DRV-B"],
            ["scope-2"],
            목표기사건당지급액: 45_000m,
            키워드: ["냉장", "차량적합성"]));

        Assert.Equal("DCT-001", result.사례목록[0].사례Id);
        Assert.Contains(result.정책근거목록, x => x.근거Id == "DCT-POLICY-HARD-CONSTRAINT");
    }

    [Fact]
    public void 조회는_후보없음_키워드가_공개배차_사례를_반환한다()
    {
        var service = new 규칙기반배차AI판단근거조회Service();

        var result = service.조회(new 배차AI판단근거요청(
            "국내화물운송OS:공개배차전환",
            ["REQ-NONE"],
            [],
            ["scope-3"],
            키워드: ["후보없음", "공개배차", "냉동"]));

        Assert.Contains(result.사례목록, x => x.사례Id == "DCT-006");
    }

    [Fact]
    public void 조회는_음식배달OS_멀티배차_정책근거와_사례를_반환한다()
    {
        var service = new 규칙기반배차AI판단근거조회Service();

        var result = service.조회(new 배차AI판단근거요청(
            "음식배달OS:멀티배차",
            ["FOOD-A", "FOOD-B"],
            [],
            ["bjd-sigungu:11440"],
            묶음크기: 2,
            키워드: ["음식", "조리완료", "배달완료시간", "같은배달권"]));

        Assert.Contains(result.정책근거목록, x => x.근거Id == "FOOD-POLICY-MULTI-DELIVERY-SCOPE");
        Assert.Contains(result.정책근거목록, x => x.근거Id == "FOOD-POLICY-DELIVERY-TIME-LIMIT");
        Assert.Contains(result.사례목록, x => x.사례Id == "FOOD-001");
    }

    [Fact]
    public void 조회는_Source에_누적된_운영자_판단사례를_반환한다()
    {
        var source = new 운영자판단사례Source();
        var service = new 규칙기반배차AI판단근거조회Service(source);

        var result = service.조회(new 배차AI판단근거요청(
            "국내화물운송OS:플랫폼수익묶음",
            ["REQ-ADMIN-A", "REQ-ADMIN-B"],
            ["DRV-ADMIN"],
            ["scope-admin"],
            묶음크기: 2,
            목표건당플랫폼순이익: 500m,
            키워드: ["운영자확정", "같은배달권", "묶음"]));

        Assert.Equal("ADMIN-DCT-TEST", result.사례목록[0].사례Id);
        Assert.Contains("운영자가 승인한 묶음", result.사례목록[0].판단요약);
    }

    private sealed class 운영자판단사례Source : I배차AI판단근거Source
    {
        private readonly 정적배차AI판단근거Source _staticSource = new();

        public IReadOnlyList<배차AI정책근거Seed> 정책근거목록 => _staticSource.정책근거목록;

        public IReadOnlyList<배차AI판단사례Seed> 사례목록 { get; } =
        [
            new(
                "ADMIN-DCT-TEST",
                "운영자가 확정한 같은 배달권 묶음",
                "국내 화물 운송 OS",
                ["운영자확정", "같은배달권", "묶음", "플랫폼", "수익"],
                "같은 배달권 안에 있는 두 의뢰를 한 명의 기사에게 묶음 동시 배정할 수 있는 상황이다.",
                "필수 조건 충돌이 없고 건당 플랫폼 순이익이 목표값에 가까워 운영자가 승인한 묶음 사례다.",
                "묶음 승인",
                "운영자 승인",
                "admin-ledger:ADMIN-DCT-TEST")
        ];
    }
}
