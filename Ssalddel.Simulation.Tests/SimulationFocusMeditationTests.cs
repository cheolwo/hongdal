using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "집중 판정·명상 성장·v29 저장 재생의 결정성과 계보를 검증한다.",
    Boundary = "자동 시험은 Unity 실제 입력과 Game View 증거를 대신하지 않는다.")]
public sealed class SimulationFocusMeditationTests
{
    [Theory]
    [InlineData(500, Simulation집중판정Codes.Perfect, 500000)]
    [InlineData(1500, Simulation집중판정Codes.Perfect, 500000)]
    [InlineData(400, Simulation집중판정Codes.Good, 400000)]
    [InlineData(0, Simulation집중판정Codes.Miss, 0)]
    public void 표준왕복판정은_정수위치와_양쪽중앙을_결정적으로계산한다(
        int offset, string resultCode, int position)
    {
        var result = Simulation집중판정Policy.Evaluate(
            Challenge(Simulation집중판정Codes.Standard), offset);

        Assert.Equal(resultCode, result.ResultCode);
        Assert.Equal(position, result.PositionMicro);
    }

    [Fact]
    public void 미입력과실패는_명상과회복을주지않고_위협계보도만들지않는다()
    {
        var challenge = Challenge(Simulation집중판정Codes.Standard);
        var noInput = Simulation집중판정Policy.Evaluate(challenge, null);
        var miss = Simulation집중판정Policy.Evaluate(challenge, 0);

        Assert.Equal(Simulation집중판정Codes.NoInput, noInput.ResultCode);
        Assert.Equal(0, noInput.명상경험증가Milli);
        Assert.Equal(0, noInput.회복증가Milli);
        Assert.Equal(Simulation집중판정Codes.Miss, miss.ResultCode);
        Assert.Equal(0, miss.명상경험증가Milli);
        Assert.Equal(0, miss.회복증가Milli);
    }

    [Fact]
    public void 중립건너뛰기는_성장_회복_실패연속을_변경하지않는다()
    {
        var result = Simulation집중판정Policy.Evaluate(
            Challenge(Simulation집중판정Codes.NeutralSkip), null);

        Assert.Equal(Simulation집중판정Codes.AssistedNeutral,
            result.ResultCode);
        Assert.Equal(0, result.명상경험증가Milli);
        Assert.Equal(0, result.회복증가Milli);
    }

    [Fact]
    public void 정확한집중은_명상과_벌목분야계보를_한번만남기고_v29로봉인된다()
    {
        var ledger = new Simulation행위발현Ledger("world:nature");
        var record = ledger.Append(new Simulation행위발현Record
        {
            WorldStableId = "world:nature",
            SessionStableId = "session:nature",
            PlayableLoopStableId = "playable-loop:nature-shelter-foundation.v1",
            WorldInteractionId = SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
            CommandId = "command:harvest:complete",
            TriggerSourceCode = SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
            InitiatorStableId = "player:one",
            ActorStableId = "player:one",
            ActorKindCode = "Player",
            TargetStableIds = new[] { "resource:nature-tree:01" },
            OutcomeStableId = "outcome:harvest:complete",
            PrimaryOutcomeCode = "HarvestCompleted",
            결과분류Code = Simulation행위결과분류Codes.성공,
            EffectBatchStableId = "effect-batch:harvest:complete",
            EffectReceiptStableIds = new[] { "effect-receipt:harvest:complete" },
            변화의미Codes = new[]
            {
                Simulation행위변화의미Codes.세계객체생성,
                Simulation행위변화의미Codes.플레이어진척변경,
            },
            BeforeWorldRevision = 2,
            AfterWorldRevision = 3,
            AppliedWorldTick = 3,
            RuleRevision = "fixture.r1",
        });
        var result = Simulation집중판정Policy.Evaluate(
            Challenge(Simulation집중판정Codes.Standard), 500);
        result.SourceActionRecordStableId = record.행위기록StableId;
        result.AppliedWorldRevision = record.AfterWorldRevision;
        var proficiency = new Simulation플레이어분야Engine("player:one");

        proficiency.ApplyField(new Simulation현장숙련기여Request
            { PlayerStableId = "player:one", 행위기록 = record });
        proficiency.ApplyMeditation(new Simulation명상숙련기여Request
        {
            PlayerStableId = "player:one",
            행위기록 = record,
            집중판정결과 = result,
        });
        var profile = proficiency.ApplyMeditation(
            new Simulation명상숙련기여Request
            {
                PlayerStableId = "player:one",
                행위기록 = record,
                집중판정결과 = result,
            });
        var domain = profile.분야진척들.Single(value =>
            value.분야StableId == Simulation플레이어분야Codes.채집자원);

        Assert.Equal(2, domain.현장숙련도);
        Assert.Equal(250, profile.명상경험Milli);
        Assert.Equal(0, profile.명상숙련도);
        Assert.Equal(Simulation분야단계Codes.미경험,
            profile.명상숙련도단계Code);
        Assert.Single(profile.명상기여기록들);
        Assert.Equal(Simulation집중판정Codes.Logging,
            profile.명상분야기여요약들.Single().세부숙련StableId);

        var session = CreateSession();
        var save = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:focus-meditation:v29",
            ExpectedRevision = session.Revision,
            ActionManifestationLedger = ledger.Snapshot(),
            PlayerDomainProfile = profile,
        });
        var restored = SimulationSessionReplay.Restore(save);
        var savedAgain = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = save.SaveStableId,
            ExpectedRevision = restored.Revision,
        });

        Assert.Equal(SimulationSaveSchemaVersions.V29, save.SchemaVersion);
        Assert.Equal(save.ReplayHash, savedAgain.ReplayHash);
        Assert.Equal(250, savedAgain.PlayerDomainProfile!.명상경험Milli);
    }

    [Fact]
    public void v1분야Profile은_명상0인_v2로읽기호환된다()
    {
        var legacy = new Simulation플레이어분야Engine("player:legacy")
            .Snapshot();
        legacy.SchemaCode = Simulation플레이어분야SchemaCodes.분야ProfileV1;
        legacy.StateHashSha256 = Simulation플레이어분야Engine
            .CalculateLegacyV1Hash(legacy);

        var restored = Simulation플레이어분야Engine.Restore(legacy).Snapshot();

        Assert.Equal(Simulation플레이어분야SchemaCodes.분야Profile,
            restored.SchemaCode);
        Assert.Equal(0, restored.명상숙련도);
        Assert.Equal(0, restored.명상경험Milli);
        Assert.Empty(restored.명상기여기록들);
    }

    [Fact]
    public void 집중ProfileCatalog은_모든WI를분류하고_벌목만첫적용으로표시한다()
    {
        var domains = Simulation기본플레이어분야Catalog.Create();
        var focus = Simulation기본집중ProfileCatalog.Create();

        Assert.Equal(domains.Wi결속들.Length, focus.Profiles.Length);
        Assert.All(domains.Wi결속들, binding => Assert.Single(
            focus.Profiles, value => value.WorldInteractionId ==
                binding.WorldInteractionId));
        var logging = focus.Profiles.Single(value => value.WorldInteractionId ==
            SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId);
        Assert.Equal(Simulation집중판정Codes.ProfileApplied,
            logging.적용상태Code);
        Assert.Equal(Simulation집중판정Codes.FocusTiming,
            logging.ChallengeKindCode);
        Assert.Contains(focus.Profiles, value => value.적용상태Code ==
            Simulation집중판정Codes.ProfileNpcOnly);
        Assert.Contains(focus.Profiles, value => value.적용상태Code ==
            Simulation집중판정Codes.ProfilePending);
        Assert.Contains(focus.Profiles, value => value.적용상태Code ==
            Simulation집중판정Codes.ProfileExcluded);
    }

    [Fact]
    public void 명상WiFamily는_비실행상위분류이며_모든WI를전수판정한다()
    {
        var domains = Simulation기본플레이어분야Catalog.Create();
        var catalog = Simulation기본명상WiFamilyCatalog.Create();

        var family = Assert.Single(catalog.Families);
        Assert.Equal(Simulation명상WiFamilyCodes.FamilyStableId,
            family.WiFamilyStableId);
        Assert.Equal(Simulation명상WiFamilyCodes.MetadataOnly,
            family.ExecutionKindCode);
        Assert.Equal(Simulation명상WiFamilyCodes.AfterActionRecord,
            family.ApplicationPhaseCode);
        Assert.False(family.IsExecutable);
        Assert.False(family.OwnsPreviewConfirmTaskEffect);
        Assert.Equal(domains.Wi결속들.Length, catalog.Bindings.Length);
        Assert.DoesNotContain(domains.Wi결속들, value => string.Equals(
            value.WorldInteractionId, family.WiFamilyStableId,
            StringComparison.Ordinal));

        foreach (var binding in domains.Wi결속들)
        {
            var familyBinding = Assert.Single(catalog.Bindings, value =>
                value.WorldInteractionId == binding.WorldInteractionId);
            var playerAction = binding.기여방식Code ==
                               Simulation분야기여방식Codes.PlayerDirect
                               || binding.기여방식Code ==
                               Simulation분야기여방식Codes.PlayerOrOperation
                               || binding.기여방식Code ==
                               Simulation분야기여방식Codes.LearningOnly;
            if (playerAction)
            {
                Assert.Equal(Simulation명상WiFamilyCodes.Bound,
                    familyBinding.결속상태Code);
                Assert.Equal(new[]
                    {
                        Simulation명상WiFamilyCodes.FamilyStableId,
                    }, familyBinding.상위WiFamilyStableIds);
                Assert.Empty(familyBinding.사유Code);
            }
            else
            {
                Assert.Equal(Simulation명상WiFamilyCodes.NotApplicable,
                    familyBinding.결속상태Code);
                Assert.Empty(familyBinding.상위WiFamilyStableIds);
                Assert.False(string.IsNullOrWhiteSpace(
                    familyBinding.사유Code));
            }
        }
    }

    private static Simulation집중판정ChallengeSnapshot Challenge(string mode)
        => new()
        {
            ChallengeStableId = "focus:harvest:01",
            PlayerStableId = "player:one",
            WorldInteractionId = SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
            OriginCommandId = "command:harvest",
            TargetStableId = "resource:nature-tree:01",
            분야StableId = Simulation플레이어분야Codes.채집자원,
            세부숙련StableId = Simulation집중판정Codes.Logging,
            Policy = Simulation집중판정Policy.Create(mode),
        };

    private static 경영SimulationSessionAggregate CreateSession()
        => new(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.Parse("fa55f0e1-715a-4fad-8929-94df87428631"),
            ScenarioStableId = "scenario:focus-meditation",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 20260828,
            RuleRevision = "simulation.rule.r1",
            DurationTicks = 20,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:solo",
                TerritoryStableId = "world:nature",
                SettlementStableId = "settlement:nature",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 28, 0, 0, 0, TimeSpan.Zero),
            },
        });
}
