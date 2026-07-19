using 살뜰.Services.Dispatch.Coordination;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 국내화물배차조율ServiceTests
{
    [Fact]
    public void 조율은_기사당_최대추천건수를_넘기지_않는다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1"),
                Request("REQ-2"),
                Request("REQ-3")
            ],
            기사후보목록:
            [
                Driver("DRV-1")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m),
                Candidate("REQ-2", "DRV-1", 90m),
                Candidate("REQ-3", "DRV-1", 80m)
            ]);

        var result = service.조율(input);

        Assert.Equal(2, result.추천배정목록.Count);
        Assert.Equal(["REQ-1", "REQ-2"], result.추천배정목록.Select(x => x.의뢰Id).ToArray());
        Assert.Single(result.보류목록, x => x.의뢰Id == "REQ-3");
    }

    [Fact]
    public void 조율은_하나의_운송의뢰를_중복배정하지_않는다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1"),
                Request("REQ-2")
            ],
            기사후보목록:
            [
                Driver("DRV-1"),
                Driver("DRV-2")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m),
                Candidate("REQ-1", "DRV-2", 95m),
                Candidate("REQ-2", "DRV-2", 90m)
            ]);

        var result = service.조율(input);

        Assert.Equal(2, result.추천배정목록.Count);
        Assert.Equal(result.추천배정목록.Count, result.추천배정목록.Select(x => x.의뢰Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-1" && x.기사Id == "DRV-1");
        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-2" && x.기사Id == "DRV-2");
    }

    [Fact]
    public void 조율은_단건_고점수보다_전체_저비용_조합을_우선한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 1,
            운송의뢰목록:
            [
                Request("REQ-1"),
                Request("REQ-2")
            ],
            기사후보목록:
            [
                Driver("DRV-1"),
                Driver("DRV-2")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m, expectedCost: 100000m),
                Candidate("REQ-1", "DRV-2", 90m, expectedCost: 1000m),
                Candidate("REQ-2", "DRV-1", 80m, expectedCost: 1000m)
            ]);

        var result = service.조율(input);

        Assert.Equal(2, result.추천배정목록.Count);
        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-1" && x.기사Id == "DRV-2");
        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-2" && x.기사Id == "DRV-1");
        Assert.Equal(2000m, result.전체예상비용);
    }

    [Fact]
    public void 조율은_이미_두_건을_수락한_기사를_추천에서_제외한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1")
            ],
            기사후보목록:
            [
                Driver("DRV-1", acceptedTransportCount: 2)
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m)
            ]);

        var result = service.조율(input);

        Assert.Empty(result.추천배정목록);
        Assert.Single(result.보류목록, x => x.의뢰Id == "REQ-1");
    }

    [Fact]
    public void 조율은_기존_수락건수를_차감한_남은_용량만큼만_추천한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1"),
                Request("REQ-2")
            ],
            기사후보목록:
            [
                Driver("DRV-1", acceptedTransportCount: 1)
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m),
                Candidate("REQ-2", "DRV-1", 90m)
            ]);

        var result = service.조율(input);

        Assert.Single(result.추천배정목록);
        Assert.Equal("REQ-1", result.추천배정목록[0].의뢰Id);
        Assert.Single(result.보류목록, x => x.의뢰Id == "REQ-2");
    }

    [Fact]
    public void 조율은_기사_여유_상황에서_근거리_단건_알고리즘을_적용한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1")
            ],
            기사후보목록:
            [
                Driver("DRV-FAR"),
                Driver("DRV-NEAR"),
                Driver("DRV-IDLE")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-FAR", 100m, expectedCost: 1000m, pickupDistanceKm: 50m),
                Candidate("REQ-1", "DRV-NEAR", 90m, expectedCost: 20000m, pickupDistanceKm: 1m)
            ]);

        var result = service.조율(input);

        Assert.Equal("기사여유_근거리단건우선", result.적용알고리즘);
        Assert.Equal(3m, result.가용기사운송의뢰비율);
        Assert.Single(result.추천배정목록);
        Assert.Equal("DRV-NEAR", result.추천배정목록[0].기사Id);
    }

    [Fact]
    public void 조율은_기사_목표_건당지급액에_가까운_기사후보를_우선한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 1,
            운송의뢰목록:
            [
                Request("REQ-1")
            ],
            기사후보목록:
            [
                Driver("DRV-LOW"),
                Driver("DRV-TARGET")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-LOW", 80m, expectedCost: 1_000m, pickupDistanceKm: 1m),
                Candidate("REQ-1", "DRV-TARGET", 80m, expectedCost: 5_000m, pickupDistanceKm: 1m)
            ],
            기사배정AI정책: new 국내화물기사배정AI정책(목표기사건당지급액: 5_000m));

        var result = service.조율(input);

        Assert.Single(result.추천배정목록);
        Assert.Equal("DRV-TARGET", result.추천배정목록[0].기사Id);
        Assert.Contains("기사목표단가회귀", result.추천배정목록[0].배지);
    }

    [Fact]
    public void 조율은_의뢰가_많은_상황에서_경로삽입효율_알고리즘을_적용한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-1"),
                Request("REQ-2"),
                Request("REQ-3")
            ],
            기사후보목록:
            [
                Driver("DRV-1")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-1", 100m),
                Candidate("REQ-2", "DRV-1", 90m),
                Candidate("REQ-3", "DRV-1", 80m)
            ]);

        var result = service.조율(input);

        Assert.Equal("의뢰많음_경로삽입효율우선", result.적용알고리즘);
        Assert.Equal(0.33m, result.가용기사운송의뢰비율);
        Assert.Equal(2, result.추천배정목록.Count);
    }

    [Fact]
    public void 조율은_수익묶음_후보에_포함된_의뢰를_우선한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-A"),
                Request("REQ-B"),
                Request("REQ-C")
            ],
            기사후보목록:
            [
                Driver("DRV-1")
            ],
            조합평가목록:
            [
                Candidate("REQ-A", "DRV-1", 80m, expectedCost: 40_000m),
                Candidate("REQ-B", "DRV-1", 80m, expectedCost: 42_000m),
                Candidate("REQ-C", "DRV-1", 95m, expectedCost: 10_000m)
            ],
            수익묶음후보목록:
            [
                new 운송의뢰수익묶음후보(
                    "REQ-A+REQ-B",
                    "수익우선묶음",
                    ["REQ-A", "REQ-B"],
                    예상운임합계: 160_000m,
                    예상원가합계: 80_000m,
                    예상플랫폼순이익: 80_000m,
                    우선순위점수: 90_000m,
                    묶음크기: 2,
                    묶음의뢰Ids: ["REQ-A", "REQ-B"],
                    배지: ["2건묶음"],
                    경고: [],
                    묶음가능여부: true,
                    제외사유: [])
            ]);

        var result = service.조율(input);

        Assert.Equal(["REQ-A", "REQ-B"], result.추천배정목록.Select(x => x.의뢰Id).Order(StringComparer.Ordinal).ToArray());
        Assert.Single(result.보류목록, x => x.의뢰Id == "REQ-C");
        Assert.All(result.추천배정목록, x => Assert.Contains("수익묶음우선", x.배지));
    }

    [Fact]
    public void 조율은_수익묶음_후보를_가능하면_한_명의_기사에게_동시배정한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-A"),
                Request("REQ-B")
            ],
            기사후보목록:
            [
                Driver("DRV-BUNDLE"),
                Driver("DRV-SPLIT")
            ],
            조합평가목록:
            [
                Candidate("REQ-A", "DRV-BUNDLE", 80m, expectedCost: 20_000m),
                Candidate("REQ-B", "DRV-BUNDLE", 80m, expectedCost: 20_000m),
                Candidate("REQ-A", "DRV-SPLIT", 95m, expectedCost: 5_000m)
            ],
            수익묶음후보목록:
            [
                new 운송의뢰수익묶음후보(
                    "REQ-A+REQ-B",
                    "수익우선묶음",
                    ["REQ-A", "REQ-B"],
                    예상운임합계: 160_000m,
                    예상원가합계: 40_000m,
                    예상플랫폼순이익: 120_000m,
                    우선순위점수: 130_000m,
                    묶음크기: 2,
                    묶음의뢰Ids: ["REQ-A", "REQ-B"],
                    배지: ["2건묶음"],
                    경고: [],
                    묶음가능여부: true,
                    제외사유: [])
            ]);

        var result = service.조율(input);

        Assert.Equal(2, result.추천배정목록.Count);
        Assert.All(result.추천배정목록, x => Assert.Equal("DRV-BUNDLE", x.기사Id));
        Assert.All(result.추천배정목록, x => Assert.Contains("한 명의 기사에게 묶음 동시 배정", x.배지));
    }

    [Fact]
    public void 조율은_수익묶음도_기사_목표_평균지급액에_가까운_기사에게_배정한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 2,
            운송의뢰목록:
            [
                Request("REQ-A"),
                Request("REQ-B")
            ],
            기사후보목록:
            [
                Driver("DRV-LOW"),
                Driver("DRV-TARGET")
            ],
            조합평가목록:
            [
                Candidate("REQ-A", "DRV-LOW", 80m, expectedCost: 1_000m),
                Candidate("REQ-B", "DRV-LOW", 80m, expectedCost: 1_000m),
                Candidate("REQ-A", "DRV-TARGET", 80m, expectedCost: 5_000m),
                Candidate("REQ-B", "DRV-TARGET", 80m, expectedCost: 5_000m)
            ],
            수익묶음후보목록:
            [
                new 운송의뢰수익묶음후보(
                    "REQ-A+REQ-B",
                    "수익우선묶음",
                    ["REQ-A", "REQ-B"],
                    예상운임합계: 160_000m,
                    예상원가합계: 40_000m,
                    예상플랫폼순이익: 120_000m,
                    우선순위점수: 130_000m,
                    묶음크기: 2,
                    묶음의뢰Ids: ["REQ-A", "REQ-B"],
                    배지: ["2건묶음"],
                    경고: [],
                    묶음가능여부: true,
                    제외사유: [])
            ],
            기사배정AI정책: new 국내화물기사배정AI정책(목표기사건당지급액: 5_000m));

        var result = service.조율(input);

        Assert.Equal(2, result.추천배정목록.Count);
        Assert.All(result.추천배정목록, x => Assert.Equal("DRV-TARGET", x.기사Id));
        Assert.All(result.추천배정목록, x => Assert.Contains("기사목표단가회귀", x.배지));
        Assert.All(result.추천배정목록, x => Assert.Contains("한 명의 기사에게 묶음 동시 배정", x.배지));
    }

    [Fact]
    public void 조율은_정책상_세건_수익묶음도_한_명의_기사에게_동시배정할_수_있다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 3,
            운송의뢰목록:
            [
                Request("REQ-A"),
                Request("REQ-B"),
                Request("REQ-C")
            ],
            기사후보목록:
            [
                Driver("DRV-BUNDLE")
            ],
            조합평가목록:
            [
                Candidate("REQ-A", "DRV-BUNDLE", 80m, expectedCost: 20_000m),
                Candidate("REQ-B", "DRV-BUNDLE", 80m, expectedCost: 20_000m),
                Candidate("REQ-C", "DRV-BUNDLE", 80m, expectedCost: 20_000m)
            ],
            수익묶음후보목록:
            [
                new 운송의뢰수익묶음후보(
                    "REQ-A+REQ-B+REQ-C",
                    "수익우선묶음",
                    ["REQ-A", "REQ-B", "REQ-C"],
                    예상운임합계: 240_000m,
                    예상원가합계: 60_000m,
                    예상플랫폼순이익: 180_000m,
                    우선순위점수: 160_000m,
                    묶음크기: 3,
                    묶음의뢰Ids: ["REQ-A", "REQ-B", "REQ-C"],
                    배지: ["3건묶음"],
                    경고: [],
                    묶음가능여부: true,
                    제외사유: [],
                    예상건당플랫폼순이익: 60_000m,
                    목표수익회귀점수: 20_000m,
                    선택근거: "테스트")
            ]);

        var result = service.조율(input);

        Assert.Equal(3, result.추천배정목록.Count);
        Assert.All(result.추천배정목록, x => Assert.Equal("DRV-BUNDLE", x.기사Id));
        Assert.All(result.추천배정목록, x => Assert.Contains("한 명의 기사에게 묶음 동시 배정", x.배지));
    }

    [Fact]
    public void 조율은_같은_배달권_안의_기사와_의뢰를_먼저_묶는다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 1,
            운송의뢰목록:
            [
                Request("REQ-A", "scope-a"),
                Request("REQ-B", "scope-b")
            ],
            기사후보목록:
            [
                Driver("DRV-A", deliveryScopeKey: "scope-a"),
                Driver("DRV-B", deliveryScopeKey: "scope-b")
            ],
            조합평가목록:
            [
                Candidate("REQ-A", "DRV-A", 90m, sameDeliveryScope: true),
                Candidate("REQ-A", "DRV-B", 100m, sameDeliveryScope: false),
                Candidate("REQ-B", "DRV-A", 100m, sameDeliveryScope: false),
                Candidate("REQ-B", "DRV-B", 90m, sameDeliveryScope: true)
            ]);

        var result = service.조율(input);

        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-A" && x.기사Id == "DRV-A");
        Assert.Contains(result.추천배정목록, x => x.의뢰Id == "REQ-B" && x.기사Id == "DRV-B");
    }

    [Fact]
    public void 조율은_외부권보다_인접배달권_기사를_우선한다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 1,
            운송의뢰목록:
            [
                Request("REQ-JN", "bjd-sigungu:11260")
            ],
            기사후보목록:
            [
                Driver("DRV-ADJ", deliveryScopeKey: "bjd-sigungu:11215"),
                Driver("DRV-FAR", deliveryScopeKey: "bjd-sigungu:11680")
            ],
            조합평가목록:
            [
                Candidate("REQ-JN", "DRV-ADJ", 80m, sameDeliveryScope: false, adjacentDeliveryScope: true),
                Candidate("REQ-JN", "DRV-FAR", 90m, sameDeliveryScope: false, adjacentDeliveryScope: false)
            ]);

        var result = service.조율(input);

        Assert.Single(result.추천배정목록);
        Assert.Equal("DRV-ADJ", result.추천배정목록[0].기사Id);
    }

    [Fact]
    public void 조율은_퇴근시간대_복귀부담이_큰_후보를_후순위로_둔다()
    {
        var service = new 국내화물배차조율Service();
        var input = new 국내화물배차조율입력(
            DateTime.UtcNow,
            기사당최대추천건수: 1,
            운송의뢰목록:
            [
                Request("REQ-1")
            ],
            기사후보목록:
            [
                Driver("DRV-RETURN-FAR"),
                Driver("DRV-RETURN-OK"),
                Driver("DRV-IDLE")
            ],
            조합평가목록:
            [
                Candidate("REQ-1", "DRV-RETURN-FAR", 100m, expectedCost: 1000m, pickupDistanceKm: 1m, returnBurdenScore: 28m),
                Candidate("REQ-1", "DRV-RETURN-OK", 80m, expectedCost: 1000m, pickupDistanceKm: 1m, returnBurdenScore: 0m)
            ]);

        var result = service.조율(input);

        Assert.Single(result.추천배정목록);
        Assert.Equal("DRV-RETURN-OK", result.추천배정목록[0].기사Id);
    }

    private static 운송의뢰조율입력 Request(string requestId, string deliveryScopeKey = "scope-default")
        => new(
            배차대기Id: 1,
            requestId,
            원본의뢰유형: "CargoTransport",
            화물종류: "일반",
            화물온도조건: "상온",
            화물중량Kg: null,
            최종운임: 50000m,
            배달권키: deliveryScopeKey,
            배달권명: deliveryScopeKey,
            상차좌표: null,
            하차좌표: null,
            상차시간창시작Utc: null,
            상차시간창종료Utc: null,
            하차시간창시작Utc: null,
            하차시간창종료Utc: null,
            추천라운드: 0,
            생성시각Utc: DateTime.UtcNow);

    private static 기사후보조율입력 Driver(
        string driverId,
        int acceptedTransportCount = 0,
        string deliveryScopeKey = "scope-default")
        => new(
            driverId,
            차량종류: "1톤 카고",
            운행상태: "운행중",
            현재수락운송건수: acceptedTransportCount,
            배달권키: deliveryScopeKey,
            배달권명: deliveryScopeKey,
            현재좌표: null,
            Aging점수: 0m,
            Aging기준시각Utc: DateTime.UtcNow,
            상차접근허용반경Km: null,
            최근위치수신시각Utc: DateTime.UtcNow);

    private static 운송의뢰기사조합평가 Candidate(
        string requestId,
        string driverId,
        decimal score,
        decimal expectedCost = 10000m,
        decimal pickupDistanceKm = 1m,
        bool sameDeliveryScope = true,
        bool adjacentDeliveryScope = false,
        decimal? returnDistanceKm = null,
        decimal returnBurdenScore = 0m,
        bool eveningReturnBurden = false)
        => new(
            requestId,
            driverId,
            추천가능여부: true,
            상차지거리Km: pickupDistanceKm,
            상차지이동시간분: 5m,
            화물운송시간분: 20m,
            총예상시간분: 25m,
            총예상거리Km: 10m,
            예상톨비: 0m,
            예상운임: 50000m,
            예상총비용: expectedCost,
            예상순이익: 50000m - expectedCost,
            일정삽입가능여부: true,
            전체일정완수가능여부: true,
            최적삽입인덱스: null,
            경로변경이점여부: false,
            경로변경절감분: null,
            총추가지연분: null,
            동일배달권여부: sameDeliveryScope,
            인접배달권여부: adjacentDeliveryScope,
            하차후복귀거리Km: returnDistanceKm,
            복귀시간대부담점수: returnBurdenScore,
            퇴근시간대복귀부담여부: eveningReturnBurden,
            추천점수: score,
            추천사유: "테스트 추천",
            배지: [],
            경고: [],
            제외사유: []);
}
