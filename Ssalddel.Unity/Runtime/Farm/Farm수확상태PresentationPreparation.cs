using System;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.Farm
{
    /// <summary>선택된 한 재배 단위의 읽기 전용 정적 표현 자료. 수확 실행·자산 조립 결과가 아니다.</summary>
    public sealed class Farm수확상태PresentationState
    {
        internal Farm수확상태PresentationState(string session, string rule, long revision, int tick,
            string soil, string soilState, Simulation재배단위Snapshot crop, Simulation수확LotSnapshot? lot)
        {
            SessionStableId = session;
            RuleRevision = rule;
            SourceWorldRevision = revision;
            SourceWorldTick = tick;
            SoilTileStableId = soil;
            SoilStateCode = soilState;
            CultivationUnitStableId = crop.CultivationUnitStableId;
            CultivationRevision = crop.Revision;
            ProductStableId = crop.ProductStableId;
            StateCode = crop.StateCode;
            HarvestLotStableId = lot?.HarvestLotStableId ?? string.Empty;
            HarvestLotRevision = lot?.Revision;
            Quantity = lot?.Quantity;
            UnitCode = lot?.UnitCode ?? string.Empty;
            LotStateCode = lot?.StateCode ?? string.Empty;
            CausedByTaskStableId = lot?.CausedByTaskStableId ?? string.Empty;
            PresentationSlot = lot == null ? "farm.crop.grow" : "farm.crop.harvest";
            StateLabel = StateCode == Simulation재배단위상태Codes.Growing ? "생육 중"
                : StateCode == Simulation재배단위상태Codes.HarvestReady ? "수확 준비 상태 · 작업 미리보기 필요"
                : "수확 결과 · 상태 사본";

            // 표시 필드 전체를 길이 구분자로 결속한다. 표시량을 생산 규칙으로 다시 계산하지 않는다.
            var parts = new[] { rule, tick.ToString(CultureInfo.InvariantCulture), soil, soilState,
                CultivationUnitStableId, CultivationRevision.ToString(CultureInfo.InvariantCulture),
                ProductStableId, StateCode, HarvestLotStableId,
                HarvestLotRevision?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                Quantity?.ToString("G29", CultureInfo.InvariantCulture) ?? string.Empty,
                UnitCode, LotStateCode, CausedByTaskStableId };
            var parameters = string.Concat(parts.Select(x => x.Length.ToString(CultureInfo.InvariantCulture) + ":" + x));
            var interpretation = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                new DataRevisionSet(new[] { new DataRevisionReference(session, revision.ToString(CultureInfo.InvariantCulture)) }),
                "farm-harvest-state-projection.r1", rule, parameters);
            PresentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(interpretation,
                "SelectedFarmCultivation", "farm-harvest-static-state.r1", "farm-harvest-preparation.r1");
        }

        public string SessionStableId { get; }
        public string RuleRevision { get; }
        public long SourceWorldRevision { get; }
        public int SourceWorldTick { get; }
        public string SoilTileStableId { get; }
        public string SoilStateCode { get; }
        public string CultivationUnitStableId { get; }
        public long CultivationRevision { get; }
        public string ProductStableId { get; }
        public string StateCode { get; }
        public string StateLabel { get; }
        public string HarvestLotStableId { get; }
        public long? HarvestLotRevision { get; }
        public decimal? Quantity { get; }
        public string UnitCode { get; }
        public string LotStateCode { get; }
        public string CausedByTaskStableId { get; }
        public string PresentationSlot { get; }
        public string PresentationRevision { get; }
        public bool PresentationOnly => true;
        public bool CanConfirmAuthority => false;
        public string SceneBindingStatus => "E5Unlinked";
    }

    /// <summary>
    /// 같은 Session·규칙·밭·재배 단위를 명시해 생성하는 단일 대상 표현 준비.
    /// 실패 시 out state는 null이며 Current는 마지막 성공 자료일 뿐 현재 연결의 증거가 아니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "한 밭의 기존 수확 상태 사본을 판본에 결속한 정적 표현 준비로 변환한다.",
        Boundary = "표현 자료만 생성하며 Preview/Confirm·Scene·실제 수확·E5 완료를 대신하지 않는다.")]
    public sealed class Farm수확상태PresentationPreparation
    {
        private readonly string session;
        private readonly string rule;
        private readonly string soil;
        private readonly string cultivation;
        private static readonly StableIdReconciler<Farm수확상태PresentationState> Reconciler =
            new StableIdReconciler<Farm수확상태PresentationState>(
                new StableIdReconciliationPolicy<Farm수확상태PresentationState>(
                    x => x.CultivationUnitStableId,
                    presentationRevision: x => x.PresentationRevision,
                    dataRevisionComparison: (incoming, current) => incoming.SourceWorldRevision.CompareTo(current.SourceWorldRevision)));

        public Farm수확상태PresentationPreparation(string sessionStableId, string ruleRevision,
            string soilTileStableId, string cultivationUnitStableId)
        {
            StableDataId.EnsureValid(sessionStableId, nameof(sessionStableId));
            StableDataId.EnsureValid(soilTileStableId, nameof(soilTileStableId));
            StableDataId.EnsureValid(cultivationUnitStableId, nameof(cultivationUnitStableId));
            if (string.IsNullOrWhiteSpace(ruleRevision)) throw new ArgumentException("FarmRuleRevisionMissing", nameof(ruleRevision));
            session = sessionStableId;
            rule = ruleRevision;
            soil = soilTileStableId;
            cultivation = cultivationUnitStableId;
        }

        public Farm수확상태PresentationState? Current { get; private set; }

        public bool TryPrepare(SimulationFarmSurvivalStateSnapshot? source,
            out Farm수확상태PresentationState? state, out string diagnostic)
        {
            state = null;
            try
            {
                var next = Project(source);
                if (Current != null && next.SourceWorldRevision == Current.SourceWorldRevision
                    && next.PresentationRevision != Current.PresentationRevision)
                    throw new PreparationException("FarmSameRevisionConflict");
                var changes = Reconciler.Reconcile(Current == null
                    ? Array.Empty<Farm수확상태PresentationState>() : new[] { Current }, new[] { next });
                var unchanged = changes.Unchanged.Length == 1;
                Current = unchanged ? changes.Unchanged[0] : next;
                state = Current;
                diagnostic = unchanged ? "Unchanged" : "Prepared";
                return true;
            }
            catch (PreparationException error) { diagnostic = error.Message; return false; }
            catch (StableIdReconciliationException error) { diagnostic = error.ErrorCode; return false; }
        }

        private Farm수확상태PresentationState Project(SimulationFarmSurvivalStateSnapshot? source)
        {
            if (source == null) throw new PreparationException("FarmSnapshotMissing_E5Unlinked");
            Require(!string.IsNullOrWhiteSpace(source.SessionStableId), "FarmSessionMissing");
            Require(!string.IsNullOrWhiteSpace(source.RuleRevision), "FarmRuleRevisionMissing");
            Require(source.SessionStableId == session && source.RuleRevision == rule, "FarmSourceBindingMismatch");
            Require(source.WorldRevision >= 0 && source.WorldTick >= 0, "FarmSourceRevisionInvalid");
            Require(source.SimulationOnly && !source.IsOperationalState, "FarmSimulationBoundaryInvalid");
            Require(source.SoilTiles != null && source.CultivationUnits != null && source.HarvestLots != null,
                "FarmCollectionsMissing");
            Require(source.SoilTiles!.All(x => x != null) && source.CultivationUnits!.All(x => x != null)
                && source.HarvestLots!.All(x => x != null), "FarmCollectionItemMissing");
            var soils = source.SoilTiles.Where(x => x.SoilTileStableId == soil).ToArray();
            var crops = source.CultivationUnits.Where(x => x.CultivationUnitStableId == cultivation).ToArray();
            Require(soils.Length == 1 && crops.Length == 1, "FarmSelectedTargetMissingOrDuplicate");
            var crop = crops[0];
            Require(crop.TileStableId == soil, "FarmCultivationSoilMismatch");
            Require(!string.IsNullOrWhiteSpace(soils[0].StateCode) && StableDataId.IsValid(crop.ProductStableId)
                && crop.Revision >= 0, "FarmSelectedStateInvalid");
            Require(crop.StateCode == Simulation재배단위상태Codes.Growing
                || crop.StateCode == Simulation재배단위상태Codes.HarvestReady
                || crop.StateCode == Simulation재배단위상태Codes.Harvested, "FarmCultivationStateUnsupported");
            var lots = source.HarvestLots.Where(x => x.CultivationUnitStableId == cultivation).ToArray();
            Simulation수확LotSnapshot? lot = null;
            if (crop.StateCode == Simulation재배단위상태Codes.Harvested)
            {
                Require(lots.Length == 1, "FarmHarvestLotMissingOrDuplicate");
                lot = lots[0];
                Require(StableDataId.IsValid(lot.HarvestLotStableId) && StableDataId.IsValid(lot.CausedByTaskStableId)
                    && lot.ProductStableId == crop.ProductStableId && lot.Revision >= 0 && lot.Quantity >= 0
                    && !string.IsNullOrWhiteSpace(lot.UnitCode) && !string.IsNullOrWhiteSpace(lot.StateCode),
                    "FarmHarvestLotInvalid");
            }
            else Require(lots.Length == 0, "FarmHarvestStateConflict");
            return new Farm수확상태PresentationState(session, rule, source.WorldRevision, source.WorldTick,
                soil, soils[0].StateCode, crop, lot);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition) throw new PreparationException(diagnostic);
        }

        private sealed class PreparationException : InvalidOperationException
        {
            public PreparationException(string diagnostic) : base(diagnostic) { }
        }
    }
}
