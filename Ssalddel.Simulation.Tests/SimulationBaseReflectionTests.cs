using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "거점 성찰의 승인자료 동기화, Preview, Confirm, 다음 활동 적용과 저장 hash를 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3저장재생검증,
    WorkOrderIds = new[] { "E9-WO-NATURE-BASE-REFLECTION" },
    Boundary = "Fixture 검증은 실제 Provider 호출, Unity H1 배치, Play Mode 또는 Game View 증거가 아니다.")]
public sealed class SimulationBaseReflectionTests
{
    [Fact]
    public void 원문관측에서_해석후보를거쳐_사람승인Publication을만든다()
    {
        var pipeline = new Simulation학습자료승인Pipeline();
        var observation = new SimulationYouTube학습원문관측Snapshot
        {
            관측StableId = "youtube-observation:pipeline-1",
            VideoStableId = "pipeline-1",
            SourceUrl = "https://www.youtube.com/watch?v=pipeline-1",
            제목 = "돌아보기 자료",
            채널명 = "fixture",
            조회시각 = new DateTimeOffset(2026, 8, 26, 0, 0, 0,
                TimeSpan.Zero),
            원문MetadataHashSha256 = new string('3', 64),
            수집AdapterCode = "ApifyTranscriptAdapter",
            이용한계 = "선택 구간 요약만 사용하며 원문 전체와 댓글은 제외한다.",
            근거구간들 = new[]
            {
                new Simulation학습근거구간Snapshot
                {
                    시작Millisecond = 1000,
                    종료Millisecond = 5000,
                    근거요약 = "위험 단서를 다시 확인한다.",
                    구간HashSha256 = new string('4', 64),
                },
            },
        };
        var candidate = pipeline.CreateCandidate(observation,
            new Simulation학습해석후보Snapshot
            {
                후보StableId = "candidate:pipeline-1",
                분류Code = Simulation학습분류Codes.상황인식,
                요약 = "원정에서 놓친 위험을 되짚는다.",
                성찰질문들 = new[] { "어떤 단서를 지나쳤는가?" },
                제안내면능력치Code = Simulation내면능력치Codes.알아차림,
                제안내면효과Code = Simulation내면효과Codes.초심,
                해석RuleRevision = "learning-interpretation.r1",
            });

        Assert.Equal(Simulation학습자료상태Codes.Candidate,
            candidate.상태Code);
        var publication = pipeline.Approve(observation, candidate,
            "learning:pipeline-1", "r1", "admin:fixture",
            new DateTimeOffset(2026, 8, 26, 1, 0, 0, TimeSpan.Zero));

        Assert.Equal(Simulation학습자료상태Codes.Approved,
            publication.상태Code);
        Assert.Equal(candidate.InputHashSha256, publication.InputHashSha256);
        Assert.Equal(Simulation거점성찰Rules.CalculatePublicationHash(publication),
            publication.PublicationHashSha256);
        Assert.DoesNotContain("transcript", publication.요약,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void 승인자료파생원장은_같은묶음을멱등동기화하고_충돌hash를거부한다()
    {
        var bundle = CreateBundle();
        var ledger = new Simulation승인학습자료파생원장();

        Assert.True(ledger.Synchronize(bundle));
        Assert.False(ledger.Synchronize(bundle));
        Assert.Single(ledger.Freeze().Publications);

        var tampered = CreateBundle();
        tampered.Publications[0].제목 = "승인 뒤 몰래 바꾼 제목";
        var error = Assert.Throws<SimulationContractException>(
            () => ledger.Synchronize(tampered));
        Assert.Equal("SimulationApprovedLearningPublicationInvalid", error.ErrorCode);
    }

    [Fact]
    public void 세션은_승인revision을동결하고_성찰효과를다음활동에한번만적용한다()
    {
        var bundle = CreateBundle();
        var engine = CreateEngine(bundle);
        bundle.Publications[0].제목 = "세션 시작 뒤 들어온 새 관측";

        var preview = engine.Preview(new Simulation거점성찰PreviewRequest
        {
            ExpectedRevision = 0,
            PlayerStableId = "player:solo",
            일차 = 1,
            선택StableId = Simulation거점성찰선택Codes.오늘행동성찰,
            PublicationStableId = "learning:nature-awareness",
            PublicationRevision = "r1",
        });
        var pending = engine.Confirm(new Simulation거점성찰ConfirmRequest
        {
            CommandId = "reflection-confirm:1",
            ExpectedRevision = 0,
            Preview = preview,
        });

        Assert.Equal("승인된 위험 알아차림", pending.FrozenPublications[0].제목);
        Assert.Equal(0, pending.내면상태.알아차림);
        Assert.Equal(Simulation거점성찰결과Codes.다음활동적용대기,
            Assert.Single(pending.Grants).상태Code);

        var applied = engine.ApplyAtNextActivity("reflection-apply:1", 1, 2);
        Assert.Equal(1, applied.내면상태.알아차림);
        Assert.Contains(Simulation내면효과Codes.초심,
            applied.내면상태.획득내면효과Codes);
        Assert.Equal(Simulation거점성찰결과Codes.내면학습적용,
            Assert.Single(applied.Grants).상태Code);

        var duplicate = engine.ApplyAtNextActivity("reflection-apply:1", 0, 99);
        Assert.Equal(applied.StateHashSha256, duplicate.StateHashSha256);
        Assert.Equal(1, duplicate.내면상태.알아차림);
    }

    [Fact]
    public void 원문열기는_영상시청보상을만들지않고_그냥휴식도자료를요구하지않는다()
    {
        var engine = CreateEngine(CreateBundle());
        var sourcePreview = engine.Preview(new Simulation거점성찰PreviewRequest
        {
            ExpectedRevision = 0,
            PlayerStableId = "player:solo",
            일차 = 1,
            선택StableId = Simulation거점성찰선택Codes.원문열기,
            PublicationStableId = "learning:nature-awareness",
            PublicationRevision = "r1",
        });

        Assert.False(sourcePreview.보상적용가능);
        Assert.Contains("NoLearningReward", sourcePreview.설명Codes);
        var afterSource = engine.Confirm(new Simulation거점성찰ConfirmRequest
        {
            CommandId = "open-source:1",
            ExpectedRevision = 0,
            Preview = sourcePreview,
        });
        Assert.Empty(afterSource.Grants);

        var restPreview = engine.Preview(new Simulation거점성찰PreviewRequest
        {
            ExpectedRevision = 1,
            PlayerStableId = "player:solo",
            일차 = 1,
            선택StableId = Simulation거점성찰선택Codes.그냥휴식,
        });
        Assert.False(restPreview.보상적용가능);
        Assert.Equal(Simulation거점성찰결과Codes.휴식함,
            restPreview.결과Code);
    }

    [Fact]
    public void 같은날두번째성찰과_같은PublicationRevision재지급을거부한다()
    {
        var engine = CreateEngine(CreateBundle());
        var first = ReflectionPreview(engine, 0, 1);
        engine.Confirm(new Simulation거점성찰ConfirmRequest
        {
            CommandId = "reflection-confirm:1",
            ExpectedRevision = 0,
            Preview = first,
        });

        var daily = Assert.Throws<SimulationConflictException>(() =>
            ReflectionPreview(engine, 1, 1));
        Assert.Equal("SimulationReflectionDailyLimitReached", daily.ErrorCode);

        engine.ApplyAtNextActivity("reflection-apply:1", 1, 2);
        var repeatedRevision = Assert.Throws<SimulationConflictException>(() =>
            ReflectionPreview(engine, 2, 2));
        Assert.Equal("SimulationReflectionPublicationAlreadyGranted",
            repeatedRevision.ErrorCode);
    }

    [Fact]
    public void 상태hash는_변조를거부하고_LocalProcess와RemoteHost계산이같다()
    {
        var local = CreateEngine(CreateBundle());
        var hosted = CreateEngine(CreateBundle());

        CloseSameReflection(local);
        CloseSameReflection(hosted);
        var localSnapshot = local.Snapshot();
        var hostedSnapshot = hosted.Snapshot();

        Assert.Equal(localSnapshot.StateHashSha256,
            hostedSnapshot.StateHashSha256);
        Assert.Equal(localSnapshot.Revision, hostedSnapshot.Revision);
        Assert.Equal(localSnapshot.내면상태.알아차림,
            hostedSnapshot.내면상태.알아차림);
        Assert.Equal(localSnapshot.StateHashSha256,
            Simulation거점성찰Engine.Restore(localSnapshot)
                .Snapshot().StateHashSha256);

        localSnapshot.내면상태.알아차림 = 99;
        var error = Assert.Throws<SimulationContractException>(() =>
            Simulation거점성찰Engine.Restore(localSnapshot));
        Assert.Equal("SimulationReflectionStateHashMismatch", error.ErrorCode);
    }

    [Fact]
    public void 기존학습카드Schema는_호환Publication으로승인할수있다()
    {
        var bundle = CreateBundle();
        bundle.Publications[0].SchemaCode =
            Simulation거점성찰SchemaCodes.기존학습카드Publication;
        Seal(bundle);

        var engine = CreateEngine(bundle);

        Assert.Equal(Simulation거점성찰SchemaCodes.기존학습카드Publication,
            engine.Snapshot().FrozenPublications.Single().SchemaCode);
    }

    private static void CloseSameReflection(Simulation거점성찰Engine engine)
    {
        var preview = ReflectionPreview(engine, 0, 1);
        engine.Confirm(new Simulation거점성찰ConfirmRequest
        {
            CommandId = "reflection-confirm:parity",
            ExpectedRevision = 0,
            Preview = preview,
        });
        engine.ApplyAtNextActivity("reflection-apply:parity", 1, 2);
    }

    private static Simulation거점성찰Preview ReflectionPreview(
        Simulation거점성찰Engine engine,
        long revision,
        int day)
        => engine.Preview(new Simulation거점성찰PreviewRequest
        {
            ExpectedRevision = revision,
            PlayerStableId = "player:solo",
            일차 = day,
            선택StableId = Simulation거점성찰선택Codes.오늘행동성찰,
            PublicationStableId = "learning:nature-awareness",
            PublicationRevision = "r1",
        });

    private static Simulation거점성찰Engine CreateEngine(
        Simulation승인학습자료동기화Bundle bundle)
        => new(new Simulation거점성찰InitialStateRequest
        {
            PlayerStableId = "player:solo",
            시작일차 = 1,
            승인자료묶음 = bundle,
        });

    private static Simulation승인학습자료동기화Bundle CreateBundle()
    {
        var publication = new Simulation승인학습자료Publication
        {
            PublicationStableId = "learning:nature-awareness",
            Revision = "r1",
            제목 = "승인된 위험 알아차림",
            분류Code = Simulation학습분류Codes.상황인식,
            요약 = "원정에서 지나친 위험 단서를 되짚는다.",
            성찰질문들 = new[] { "돌아오기 전에 놓친 단서는 무엇인가?" },
            내면능력치Code = Simulation내면능력치Codes.알아차림,
            내면효과Code = Simulation내면효과Codes.초심,
            원문관측StableId = "youtube-observation:test-1",
            원문관측HashSha256 = new string('1', 64),
            SourceUrl = "https://www.youtube.com/watch?v=test-1",
            승인자StableId = "admin:fixture",
            승인시각 = new DateTimeOffset(2026, 8, 26, 0, 0, 0,
                TimeSpan.Zero),
            InputHashSha256 = new string('2', 64),
            이용한계 = "선택적 원문 링크이며 시청 여부를 보상 근거로 사용하지 않는다.",
        };
        var bundle = new Simulation승인학습자료동기화Bundle
        {
            LedgerRevision = "approved-learning-ledger.r1",
            수집시각 = new DateTimeOffset(2026, 8, 26, 1, 0, 0,
                TimeSpan.Zero),
            Publications = new[] { publication },
        };
        Seal(bundle);
        return bundle;
    }

    private static void Seal(Simulation승인학습자료동기화Bundle bundle)
    {
        foreach (var publication in bundle.Publications)
            publication.PublicationHashSha256 =
                Simulation거점성찰Rules.CalculatePublicationHash(publication);
        bundle.InputHashSha256 =
            Simulation거점성찰Rules.CalculateBundleInputHash(bundle);
    }
}
