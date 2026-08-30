using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "FB01 기존 Preview 보존·권한·지정 1회·완료단위와 수량 보존을 시험한다.",
    Boundary = "판본 있는 합성 Fixture의 Core 정책 지원 시험만 수행한다. 전체 WI E3·실제 Session·수확·운반·Save 증거가 아니다.",
    WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05" })]
public sealed class Simulation농사수확위임Tests
{
    private static Simulation공간용량Snapshot 용량(decimal 수량) => new()
    { CapacityCode = Simulation공간용량Codes.StorageCapacity, Quantity = 수량, UnitCode = "KGM" };

    private static Simulation농사수확위임Input Fixture() => new()
    {
        수확Preview = new()
        {
            ActorStableId = "npc:farm", TargetStableId = "crop:1",
            ActionCode = SimulationFarmSurvivalCodes.Harvesting,
            AssignmentKindCode = SimulationFarmSurvivalCodes.NpcDelegated,
            CanConfirm = true, ProjectedQuantity = 100m, ProjectedQuantityUnitCode = "KGM"
        },
        승인ActorStableId = "npc:farm", 승인재배단위StableId = "crop:1",
        승인보관처StableId = "box:1", 대상보관처StableId = "box:1",
        승인최대수량Kgm = 100m, 기존위임자격확인 = true, 보관처재고사용권한확인 = true,
        안전상태Code = Simulation농사수확위임Codes.Safe,
        보관용량 = 용량(100m), 점유용량 = 용량(0m), 예약용량 = 용량(0m),
        운반여유수량 = 100m, 운반여유단위Code = "KGM", 완료단위Kgm = 5m,
        완료단위기준Revision = "fixture:fb01-completion-unit.r1"
    };

    [Theory]
    [InlineData(100, 0, 0, 100, 100, 100)]
    [InlineData(60, 12, 7, 100, 100, 40)]
    [InlineData(100, 0, 0, 18, 100, 15)]
    [InlineData(100, 0, 0, 100, 33, 30)]
    [InlineData(100, 95, 0, 100, 100, 5)]
    [InlineData(100, 90, 5, 100, 100, 5)]
    public void 가장작은여유를_완료단위로내리고_총량보존(int 전체, int 점유, int 예약,
        int 운반, int 승인, int 기대)
    {
        var 입력 = Fixture();
        입력.보관용량.Quantity = 전체; 입력.점유용량.Quantity = 점유; 입력.예약용량.Quantity = 예약;
        입력.운반여유수량 = 운반; 입력.승인최대수량Kgm = 승인;
        var 이전 = JsonSerializer.Serialize(입력);
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.True(결과.후보허용);
        Assert.Equal(기대, 결과.수용가능후보수량Kgm);
        Assert.Equal(100m, 결과.수용가능후보수량Kgm + 결과.작물잔량Kgm);
        Assert.Equal(이전, JsonSerializer.Serialize(입력));
        Assert.False(결과.StateChanged);
        Assert.True(결과.SimulationOnly);
        Assert.False(결과.IsOperationalState);
        Assert.Equal(Simulation농사수확위임Codes.AwaitAuthorityCommand, 결과.다음행동Code);
    }

    [Theory]
    [InlineData("full", "FarmHarvestDelegationNoCompleteUnit")]
    [InlineData("subunit", "FarmHarvestDelegationNoCompleteUnit")]
    [InlineData("transportzero", "FarmHarvestDelegationNoCompleteUnit")]
    [InlineData("productionzero", "FarmHarvestDelegationNoCompleteUnit")]
    [InlineData("occupiedoverflow", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("reservedoverflow", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("occupiednegative", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("reservednegative", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("capacitynegative", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("capacitynull", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("slot", "FarmHarvestDelegationCapacityInvalid")]
    [InlineData("capacityunit", "FarmHarvestDelegationUnitMismatch")]
    [InlineData("occupiedunit", "FarmHarvestDelegationUnitMismatch")]
    [InlineData("reservedunit", "FarmHarvestDelegationUnitMismatch")]
    [InlineData("transportunit", "FarmHarvestDelegationUnitMismatch")]
    [InlineData("productionunit", "FarmHarvestDelegationUnitMismatch")]
    [InlineData("previewfalse", "FarmHarvestDelegationPreviewRejected")]
    [InlineData("previewblock", "ExistingFarmBlock")]
    [InlineData("previewnull", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("blocknull", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("action", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("assignment", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("simulationfalse", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("operational", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("productionnegative", "FarmHarvestDelegationPreviewInvalid")]
    [InlineData("actor", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("target", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("storage", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("emptyactor", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("emptytarget", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("emptystorage", "FarmHarvestDelegationScopeMismatch")]
    [InlineData("delegation", "FarmHarvestDelegationAuthorityDenied")]
    [InlineData("inventory", "FarmHarvestDelegationAuthorityDenied")]
    [InlineData("repeated", "FarmHarvestDelegationAlreadyExecuted")]
    [InlineData("negativecount", "FarmHarvestDelegationAlreadyExecuted")]
    [InlineData("unsafe", "FarmHarvestDelegationUnsafeOrUnknown")]
    [InlineData("unknown", "FarmHarvestDelegationUnsafeOrUnknown")]
    [InlineData("zeroquantum", "FarmHarvestDelegationCompletionUnitInvalid")]
    [InlineData("negativequantum", "FarmHarvestDelegationCompletionUnitInvalid")]
    [InlineData("norevision", "FarmHarvestDelegationCompletionUnitInvalid")]
    [InlineData("zeroapproval", "FarmHarvestDelegationInputInvalid")]
    [InlineData("negativeapproval", "FarmHarvestDelegationInputInvalid")]
    [InlineData("negativetransport", "FarmHarvestDelegationInputInvalid")]
    public void 미확인이나범위밖입력을_상태변경없이거부(string 사례, string 기대사유)
    {
        var 입력 = Fixture();
        switch (사례)
        {
            case "full": 입력.점유용량.Quantity = 100; break;
            case "subunit": 입력.운반여유수량 = 4; break;
            case "transportzero": 입력.운반여유수량 = 0; break;
            case "productionzero": 입력.수확Preview.ProjectedQuantity = 0; break;
            case "occupiedoverflow": 입력.점유용량.Quantity = decimal.MaxValue; break;
            case "reservedoverflow": 입력.점유용량.Quantity = 1; 입력.예약용량.Quantity = decimal.MaxValue; break;
            case "occupiednegative": 입력.점유용량.Quantity = -1; break;
            case "reservednegative": 입력.예약용량.Quantity = -1; break;
            case "capacitynegative": 입력.보관용량.Quantity = -1; break;
            case "capacitynull": 입력.보관용량 = null!; break;
            case "slot": 입력.보관용량.CapacityCode = Simulation공간용량Codes.WorkArea; break;
            case "capacityunit": 입력.보관용량.UnitCode = "slot"; break;
            case "occupiedunit": 입력.점유용량.UnitCode = "CapacityUnits"; break;
            case "reservedunit": 입력.예약용량.UnitCode = "EA"; break;
            case "transportunit": 입력.운반여유단위Code = ""; break;
            case "productionunit": 입력.수확Preview.ProjectedQuantityUnitCode = "MTK"; break;
            case "previewfalse": 입력.수확Preview.CanConfirm = false; break;
            case "previewblock": 입력.수확Preview.BlockingReasonCodes = new[] { "ExistingFarmBlock" }; break;
            case "previewnull": 입력.수확Preview = null!; break;
            case "blocknull": 입력.수확Preview.BlockingReasonCodes = null!; break;
            case "action": 입력.수확Preview.ActionCode = SimulationFarmSurvivalCodes.HarvestCollection; break;
            case "assignment": 입력.수확Preview.AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect; break;
            case "simulationfalse": 입력.수확Preview.SimulationOnly = false; break;
            case "operational": 입력.수확Preview.IsOperationalState = true; break;
            case "productionnegative": 입력.수확Preview.ProjectedQuantity = -1; break;
            case "actor": 입력.수확Preview.ActorStableId = "npc:other"; break;
            case "target": 입력.수확Preview.TargetStableId = "crop:other"; break;
            case "storage": 입력.대상보관처StableId = "box:other"; break;
            case "emptyactor": 입력.승인ActorStableId = ""; break;
            case "emptytarget": 입력.승인재배단위StableId = ""; break;
            case "emptystorage": 입력.승인보관처StableId = ""; break;
            case "delegation": 입력.기존위임자격확인 = false; break;
            case "inventory": 입력.보관처재고사용권한확인 = false; break;
            case "repeated": 입력.이미실행횟수 = 1; break;
            case "negativecount": 입력.이미실행횟수 = -1; break;
            case "unsafe": 입력.안전상태Code = "Unsafe"; break;
            case "unknown": 입력.안전상태Code = ""; break;
            case "zeroquantum": 입력.완료단위Kgm = 0; break;
            case "negativequantum": 입력.완료단위Kgm = -1; break;
            case "norevision": 입력.완료단위기준Revision = ""; break;
            case "zeroapproval": 입력.승인최대수량Kgm = 0; break;
            case "negativeapproval": 입력.승인최대수량Kgm = -1; break;
            case "negativetransport": 입력.운반여유수량 = -1; break;
            default: throw new ArgumentOutOfRangeException(nameof(사례));
        }
        var 이전 = JsonSerializer.Serialize(입력);
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.False(결과.후보허용);
        Assert.Equal(0m, 결과.수용가능후보수량Kgm);
        Assert.Equal(Math.Max(0m, 입력.수확Preview?.ProjectedQuantity ?? 0m), 결과.작물잔량Kgm);
        Assert.Contains(기대사유, 결과.차단사유Codes);
        Assert.Equal(Simulation농사수확위임Codes.ReviewBlockers, 결과.다음행동Code);
        Assert.Equal(이전, JsonSerializer.Serialize(입력));
    }

    [Fact]
    public void 전체입력없음은_거부() => Assert.Contains(Simulation농사수확위임Codes.InputInvalid,
        Simulation농사수확위임Policy.Evaluate(null!).차단사유Codes);

    [Fact]
    public void 차단배열은_입출력과_반복결과사이에서_격리()
    {
        var 입력 = Fixture();
        입력.수확Preview.CanConfirm = false;
        입력.수확Preview.BlockingReasonCodes = new[] { "first", "second", "first" };
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        var 반복 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.Equal(결과.차단사유Codes, 반복.차단사유Codes);
        Assert.Equal(new[] { Simulation농사수확위임Codes.PreviewRejected, "first", "second" }, 결과.차단사유Codes);
        결과.차단사유Codes[1] = "changed";
        Assert.Equal("first", 입력.수확Preview.BlockingReasonCodes[0]);
        Assert.Equal("first", 반복.차단사유Codes[1]);
    }

    [Fact]
    public void 극대수량과극소완료단위는_나눗셈overflow없이처리()
    {
        var 입력 = Fixture();
        입력.수확Preview.ProjectedQuantity = 입력.승인최대수량Kgm = 입력.운반여유수량
            = 입력.보관용량.Quantity = decimal.MaxValue;
        입력.완료단위Kgm = 0.0000000000000000000000000001m;
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.True(결과.후보허용);
        Assert.Equal(decimal.MaxValue, 결과.수용가능후보수량Kgm);
        Assert.Equal(0m, 결과.작물잔량Kgm);
    }

    [Fact]
    public void 소수완료단위는_생산반올림과분리된Fixture입력()
    {
        var 입력 = Fixture();
        입력.수확Preview.ProjectedQuantity = 0.99m;
        입력.완료단위Kgm = 0.3m;
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.Equal(0.9m, 결과.수용가능후보수량Kgm);
        Assert.Equal(0.09m, 결과.작물잔량Kgm);
        Assert.Equal(입력.완료단위기준Revision, 결과.완료단위기준Revision);
    }

    [Fact]
    public void 실제Farm서비스Preview의_미준비밭과Npc노동차단을_보존()
    {
        // 권한 bool은 신뢰된 호출자가 확인한 결과를 가정한다. 이것으로 실제 권한을 부여하지 않는다.
        var 입력 = Fixture();
        var 저장소 = new InMemory경영SimulationSessionStore();
        var 세션 = 저장소.CreateOrGet(new 경영SimulationSession생성Request
        {
            ClientRequestId = Guid.Parse("0c4ae1a5-0e0a-488c-8b56-7a691b597d44"),
            ScenarioStableId = "scenario:fb01-preview-block", ScenarioDataRevision = "fixture.r1",
            RuleRevision = "farm-fb01-preview-fixture.r1", DurationTicks = 28,
            WorldContext = new()
            {
                FactionStableId = "faction:farm", TerritoryStableId = "territory:farm",
                SettlementStableId = "settlement:farm", GameDateStartsOn = DateTimeOffset.UnixEpoch
            },
            FarmSurvival = new()
            {
                RegionStableId = "region:farm", AreaStableId = "area:farm", TileKey = "tile:farm",
                FarmBuildingStableId = "building:farm",
                Actors = new[]
                {
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = "player:farm", ActorKindCode = SimulationFarmSurvivalCodes.Player,
                        KoreanName = "시험 플레이어"
                    },
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = 입력.승인ActorStableId, ActorKindCode = SimulationFarmSurvivalCodes.Npc,
                        KoreanName = "시험 농사 NPC", CapabilityCodes = new[] { SimulationFarmActorCapabilityCodes.FarmHarvest }
                    }
                }
            }
        });
        var 서비스 = new SimulationFarmSurvivalService(저장소, authorityLocationCode: "LocalProcess");
        var 이전 = JsonSerializer.Serialize(서비스.Get(세션.SessionStableId));
        입력.수확Preview = 서비스.PreviewWork(세션.SessionStableId, new()
        {
            ExpectedRevision = 세션.Revision, ActorStableId = 입력.승인ActorStableId,
            TargetStableId = 입력.승인재배단위StableId, ActionCode = SimulationFarmSurvivalCodes.Harvesting,
            AssignmentKindCode = SimulationFarmSurvivalCodes.NpcDelegated
        });
        Assert.False(입력.수확Preview.CanConfirm);
        Assert.Contains("SimulationCultivationUnitNotFound", 입력.수확Preview.BlockingReasonCodes);
        Assert.Contains("SimulationSettlementRequiredForNpcLabor", 입력.수확Preview.BlockingReasonCodes);
        var 결과 = Simulation농사수확위임Policy.Evaluate(입력);
        Assert.False(결과.후보허용);
        foreach (var 사유 in 입력.수확Preview.BlockingReasonCodes) Assert.Contains(사유, 결과.차단사유Codes);
        Assert.Equal(이전, JsonSerializer.Serialize(서비스.Get(세션.SessionStableId)));
        Assert.Empty(서비스.Get(세션.SessionStableId).HarvestLots);
    }
}
