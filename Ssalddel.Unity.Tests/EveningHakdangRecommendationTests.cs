using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class EveningHakdangRecommendationTests
{
    [Fact]
    public void EVENING2_상차와이동은전차Fallback으로연결된다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = new 저녁학당추천Engine();
        var request = engine.CreateRequest(7, new[] { Morning(오전행동TagCodes.CargoLoaded) },
            snapshot.AvailableContents);

        var decision = engine.Fallback(request);

        Assert.Equal("learning:hongik.chariot.integrated-progress", decision.ContentStableId);
        Assert.Contains("전차", decision.Rationale);
        Assert.False(decision.UsedLlm);
    }

    [Fact]
    public void EVENING2_불확실성을건너뛴행동은바보Fallback으로연결된다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = new 저녁학당추천Engine();
        var request = engine.CreateRequest(8, new[] { Morning(오전행동TagCodes.UnknownSkipped) },
            snapshot.AvailableContents);

        Assert.Equal(저녁학당SimulationFixture.FoolContentStableId,
            engine.Fallback(request).ContentStableId);
    }

    [Fact]
    public void EVENING2_LLM은허용목록과오전행동인용안에서만추천한다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = new 저녁학당추천Engine();
        var action = Morning(오전행동TagCodes.CargoLoaded);
        var request = engine.CreateRequest(9, new[] { action }, snapshot.AvailableContents);
        var accepted = engine.Accept(request, new 저녁학당LLM추천Response
        {
            RequestStableId = request.StableId,
            RecommendedContentStableId = "learning:hongik.chariot.integrated-progress",
            Rationale = "상차 뒤 이동을 시작했으므로 통합된 정진을 돌아본다.",
            ReferencedMorningActionStableIds = new[] { action.StableId },
        });

        Assert.True(accepted.UsedLlm);
        Assert.Equal("learning:hongik.chariot.integrated-progress", accepted.ContentStableId);
    }

    [Fact]
    public void EVENING2_LLM의임의콘텐츠와근거없는추천은거부한다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = new 저녁학당추천Engine();
        var request = engine.CreateRequest(10, new[] { Morning(오전행동TagCodes.UnknownSkipped) },
            snapshot.AvailableContents);
        var response = new 저녁학당LLM추천Response
        {
            RequestStableId = request.StableId,
            RecommendedContentStableId = "learning:invented.negative-chariot",
            Rationale = "임의 추천",
            ReferencedMorningActionStableIds = new[] { "morning-action:unknown" },
        };

        Assert.Equal("EveningRecommendationLlmResponseInvalid",
            Assert.Throws<InvalidOperationException>(() => engine.Accept(request, response)).Message);
    }

    [Fact]
    public void EVENING2_전차학습은부정효과없이의지와통합정진을준다()
    {
        var snapshot = 저녁학당SimulationFixture.CreateFoolEvening();
        var engine = new 저녁학당SimulationEngine(new 저녁학당SimulationValidator());
        const string chariot = "learning:hongik.chariot.integrated-progress";
        var preview = engine.Preview(snapshot, chariot);
        var next = engine.Tick(snapshot, engine.Confirm(snapshot, preview, "힘과 지혜를 함께 쓴다."));

        Assert.Equal(내면StatCodes.Resolve, preview.TargetStatCode);
        Assert.Equal(1, next.InnerState.의지);
        Assert.Contains(내면규칙Codes.IntegratedProgress, next.InnerState.ActiveRuleCodes);
        Assert.Equal(0, next.InnerState.알아차림);
    }

    private static 오전행동Summary Morning(string tag) => new()
    {
        StableId = "morning-action:sim.potato.r1",
        Revision = 1,
        OccurredAt = new DateTimeOffset(2026, 4, 7, 10, 0, 0, TimeSpan.FromHours(9)),
        ActionCode = "PotatoWork",
        Summary = "감자 작업을 수행했다.",
        OutcomeTags = new[] { tag },
        SourceStableIds = new[] { "harvest-lot:potato.20260407" },
    };
}
