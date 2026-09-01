using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "권위 행위 원장과 플레이어 분야 성장의 결정성·저장·계보 회귀를 검증한다.",
    Boundary = "자동 시험은 Unity 엔진 cursor 소비나 Play Mode·Game View 증거를 대신하지 않는다.")]
public sealed class SimulationActionManifestationAndPlayerDomainTests
{
    [Fact]
    public void 행위기록은_엔진실행없이_보존되고_독립조건으로_조회된다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        Append(ledger, "WI-NATURE-11", "command:battle", "player:one", 0, 1,
            Simulation행위결과분류Codes.성공,
            new[] { Simulation행위변화의미Codes.실외배치변경 },
            new[] { "h1:nature:encounter" });
        Append(ledger, "WI-NATURE-14", "command:sleep", "player:one", 1, 2,
            Simulation행위결과분류Codes.성공,
            new[] { Simulation행위변화의미Codes.대기변경 },
            new[] { "h1:nature:shelter" });

        var exterior = ledger.Query(new Simulation행위기록Query
        {
            WorldStableId = "world:nature",
            변화의미Codes = new[] { Simulation행위변화의미Codes.실외배치변경 },
        });
        var sky = ledger.Query(new Simulation행위기록Query
        {
            WorldStableId = "world:nature",
            변화의미Codes = new[] { Simulation행위변화의미Codes.대기변경 },
        });

        Assert.Single(exterior.Records);
        Assert.Equal("WI-NATURE-11", exterior.Records[0].WorldInteractionId);
        Assert.Single(sky.Records);
        Assert.Equal("WI-NATURE-14", sky.Records[0].WorldInteractionId);
        Assert.Equal(2, ledger.Snapshot().TailRecords.Length);
    }

    [Fact]
    public void 체크포인트는_이후기록만_보존하고_오래된Cursor에_재조립을_요구한다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        Append(ledger, "WI-NATURE-11", "command:one", "player:one", 0, 1,
            Simulation행위결과분류Codes.성공);
        Append(ledger, "WI-NATURE-11", "command:two", "player:one", 1, 2,
            Simulation행위결과분류Codes.후퇴복구);
        ledger.CreateCheckpoint(1, "world-state-hash:1");

        var snapshot = ledger.Snapshot();
        var restored = Simulation행위발현Ledger.Restore(snapshot);
        var page = restored.Query(new Simulation행위기록Query
        {
            WorldStableId = "world:nature",
            Cursor = new Simulation행위기록Cursor(),
        });

        Assert.Equal(1, snapshot.Checkpoint.ConsolidatedThroughWorldRevision);
        Assert.Single(snapshot.TailRecords);
        Assert.True(page.RequiresCheckpointRebuild);
        Assert.Equal("command:two", page.Records[0].CommandId);
        Assert.Equal(snapshot.StateHashSha256, restored.Snapshot().StateHashSha256);
    }

    [Fact]
    public void 직접행위는_성공2_후퇴1을_주고_같은기록은_중복지급하지않는다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        var success = Append(ledger, "WI-NATURE-11", "command:victory",
            "player:one", 0, 1, Simulation행위결과분류Codes.성공);
        var retreat = Append(ledger, "WI-NATURE-11", "command:retreat",
            "player:one", 1, 2, Simulation행위결과분류Codes.후퇴복구);
        var engine = new Simulation플레이어분야Engine("player:one");

        engine.ApplyField(new Simulation현장숙련기여Request
            { PlayerStableId = "player:one", 행위기록 = success });
        engine.ApplyField(new Simulation현장숙련기여Request
            { PlayerStableId = "player:one", 행위기록 = success });
        var state = engine.ApplyField(new Simulation현장숙련기여Request
            { PlayerStableId = "player:one", 행위기록 = retreat });

        var combat = state.분야진척들.Single(value =>
            value.분야StableId == Simulation플레이어분야Codes.전투사냥);
        Assert.Equal(3, combat.현장숙련도);
        Assert.Equal(Simulation분야단계Codes.기초, combat.현장숙련도단계Code);
        Assert.Equal(2, state.기여기록들.Length);
    }

    [Fact]
    public void Npc업무는_위임_완료_플레이어검토가_모두있을때만_운영숙련을준다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        var delegation = Append(ledger, "WI-NATURE-17", "command:delegate",
            "npc:worker", 0, 1, Simulation행위결과분류Codes.성공,
            initiator: "player:one");
        var completion = Append(ledger, "WI-NATURE-16", "command:npc-craft",
            "npc:worker", 1, 2, Simulation행위결과분류Codes.성공,
            sources: new[] { delegation.행위기록StableId });
        var review = Append(ledger, "WI-REVIEW-01", "command:review",
            "player:one", 2, 3, Simulation행위결과분류Codes.성공,
            sources: new[] { delegation.행위기록StableId,
                completion.행위기록StableId });
        var engine = new Simulation플레이어분야Engine("player:one");

        var state = engine.ApplyOperation(new Simulation운영숙련기여Request
        {
            PlayerStableId = "player:one",
            위임행위기록 = delegation,
            Npc완료행위기록 = completion,
            검토행위기록 = review,
        });
        engine.ApplyOperation(new Simulation운영숙련기여Request
        {
            PlayerStableId = "player:one",
            위임행위기록 = delegation,
            Npc완료행위기록 = completion,
            검토행위기록 = review,
        });

        var operations = state.분야진척들.Single(value =>
            value.분야StableId == Simulation플레이어분야Codes.운영조직);
        Assert.Equal(1, operations.운영숙련도);
        Assert.Single(state.기여기록들);
    }

    [Fact]
    public void 승인성찰은_이해도를올리고_사냥제안만추가하며_사실을숨기지않는다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        var reflection = Append(ledger, "WI-REFLECT-01", "command:reflection",
            "player:one", 0, 1, Simulation행위결과분류Codes.성공);
        var engine = new Simulation플레이어분야Engine("player:one");
        engine.ApplyLearning(new Simulation분야학습기여Request
        {
            PlayerStableId = "player:one",
            PublicationStableId = "publication:hunting-safety",
            PublicationRevision = "r1",
            PublicationHashSha256 = "publication-hash",
            AppliedWorldRevision = 1,
            적용행위기록 = reflection,
            효과선들 = new[]
            {
                new Simulation분야이해효과선Snapshot
                {
                    분야StableId = Simulation플레이어분야Codes.전투사냥,
                    세부숙련StableId = "threat-assessment",
                    이해도증가량 = 3,
                    RuleRevision = "learning-domain.r1",
                },
            },
        });

        var perspective = engine.CreatePerspective("data:r1", "interpretation:r1",
            new[] { "fact:weather", "fact:threat", "fact:inventory" });

        Assert.Contains("optional-hunt-offer:nature",
            perspective.선택형기회후보Codes);
        Assert.Equal(3, perspective.전체자료접근Codes.Length);
        Assert.Contains("fact:weather", perspective.전체자료접근Codes);
    }

    [Fact]
    public void 기본Catalog는_계획_열원_세계자원재생을포함한68개를_중복없이결속한다()
    {
        var catalog = Simulation기본플레이어분야Catalog.Create();

        Assert.Equal(68, catalog.Wi결속들.Length);
        Assert.Equal(68, catalog.Wi결속들.Select(value => value.WorldInteractionId)
            .Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(catalog.Wi결속들,
            value => value.WorldInteractionId == "WI-REVIEW-01");
        Assert.Contains(catalog.Wi결속들,
            value => value.WorldInteractionId ==
                     Simulation개인계획Codes.WorldInteractionId
                     && value.기여방식Code ==
                     Simulation분야기여방식Codes.None
                     && value.NoPlayerProgressReason ==
                     Simulation개인계획Codes
                         .PlayerProgressionNotApplicableReason);
        Assert.Contains(catalog.Wi결속들,
            value => value.WorldInteractionId ==
                     Simulation열원상태Codes.WorldInteractionId
                     && value.기여방식Code ==
                     Simulation분야기여방식Codes.None
                     && value.NoPlayerProgressReason ==
                     Simulation열원상태Codes
                         .PlayerProgressionNotApplicableReason);
        Assert.Contains(catalog.Wi결속들,
            value => value.WorldInteractionId ==
                     Simulation세계자원재생Codes.WorldInteractionId
                     && value.기여방식Code ==
                     Simulation분야기여방식Codes.None
                     && value.NoPlayerProgressReason ==
                     Simulation세계자원재생Codes
                         .PlayerProgressionNotApplicableReason);
        Assert.Contains(catalog.분야들,
            value => value.분야StableId == Simulation플레이어분야Codes.설비에너지
                     && value.준비상태Code == Simulation분야준비상태Codes.AssetSeed);
    }

    [Fact]
    public void SaveV28은_체크포인트Tail과_분야진척을_함께봉인하고_재저장한다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        var record = Append(ledger, "WI-NATURE-11", "command:save-v28",
            "player:one", 0, 1, Simulation행위결과분류Codes.성공);
        var proficiency = new Simulation플레이어분야Engine("player:one");
        proficiency.ApplyField(new Simulation현장숙련기여Request
        {
            PlayerStableId = "player:one",
            행위기록 = record,
        });
        var session = new 경영SimulationSessionAggregate(new()
        {
            ClientRequestId = Guid.Parse("f06e238a-6985-45ba-ac04-1dcb1f182901"),
            ScenarioStableId = "scenario:action-journal-save-v28",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 20260827,
            RuleRevision = "simulation.rule.r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:solo",
                TerritoryStableId = "territory:nature",
                SettlementStableId = "settlement:nature-home",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 27, 0, 0, 0, TimeSpan.Zero),
            },
        });

        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:action-journal:v28",
            ExpectedRevision = session.Revision,
            ActionManifestationLedger = ledger.Snapshot(),
            PlayerDomainProfile = proficiency.Snapshot(),
        });
        var restored = SimulationSessionReplay.Restore(saved);
        var savedAgain = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        });

        Assert.Equal(SimulationSaveSchemaVersions.V28, saved.SchemaVersion);
        Assert.NotNull(saved.ActionManifestationLedger);
        Assert.NotNull(saved.PlayerDomainProfile);
        Assert.Equal(saved.ReplayHash, savedAgain.ReplayHash);
        Assert.Equal(saved.ActionManifestationLedger!.StateHashSha256,
            savedAgain.ActionManifestationLedger!.StateHashSha256);
        Assert.Equal(saved.PlayerDomainProfile!.StateHashSha256,
            savedAgain.PlayerDomainProfile!.StateHashSha256);
    }

    private static Simulation행위발현Record Append(
        Simulation행위발현Ledger ledger, string wi, string command, string actor,
        long before, long after, string result,
        string[]? changes = null, string[]? spaces = null,
        string? initiator = null, string[]? sources = null)
        => ledger.Append(new Simulation행위발현Record
        {
            WorldStableId = "world:nature",
            SessionStableId = "session:nature",
            PlayableLoopStableId = "playable-loop:nature-integrated",
            WorldInteractionId = wi,
            CommandId = command,
            TriggerSourceCode = "PlayerDriven",
            InitiatorStableId = initiator ?? actor,
            ActorStableId = actor,
            ActorKindCode = actor.StartsWith("npc:", StringComparison.Ordinal)
                ? "Npc" : "Player",
            TargetStableIds = new[] { "target:nature" },
            OutcomeStableId = "outcome:" + command,
            PrimaryOutcomeCode = "Completed",
            결과분류Code = result,
            EffectBatchStableId = "effect-batch:" + command,
            EffectReceiptStableIds = new[] { "effect-receipt:" + command },
            변화의미Codes = (changes ?? Array.Empty<string>())
                .Concat(new[] { Simulation행위변화의미Codes.플레이어진척변경 })
                .ToArray(),
            영향공간StableIds = spaces ?? new[] { "h1:nature" },
            SourceReferenceIds = sources ?? Array.Empty<string>(),
            BeforeWorldRevision = before,
            AfterWorldRevision = after,
            AppliedWorldTick = (int)after,
            RuleRevision = "fixture.r1",
        });
}
