using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// DB의 건물-업무 규칙-아이템 관계로 이전할 수 있는 결정적 C# 구성 대장이다.
    /// 관계와 후보 판정만 제공하며 아이템 생성이나 Session 상태 변경은 하지 않는다.
    /// </summary>
    public static class SimulationWorldBuildingItemRuleCatalog
    {
        public static SimulationWorldBuildingItemRelationResponse[] CreateRelations(
            string tileKey,
            IReadOnlyCollection<SimulationWorldTileObjectPlacementResponse> buildings)
        {
            if (tileKey != PyeongchangWorldExplorationFixtureIds.DaegwallyeongFarmCenterTile)
                return Array.Empty<SimulationWorldBuildingItemRelationResponse>();

            var relations = CreatePyeongchangFixtureRelations();
            ValidateRelations(relations, buildings);
            return relations;
        }

        public static SimulationWorldBuildingItemEligibilityPreviewResponse Evaluate(
            string sessionStableId,
            string tileKey,
            long currentWorldRevision,
            SimulationWorldBuildingItemEligibilityPreviewRequest request,
            IReadOnlyCollection<SimulationWorldBuildingItemRelationResponse> relations)
        {
            ValidateRequest(sessionStableId, request);
            var enteredBuildingId = request.EnteredBuildingStableId.Trim();
            var activeConditions = new HashSet<string>(
                request.ActiveSimulationConditionCodes ?? Array.Empty<string>(),
                StringComparer.Ordinal)
            {
                SimulationWorldExplorationCodes.PlayerInsideBuilding,
            };
            var revisionMatches = request.ObservedWorldRevision == currentWorldRevision;
            var evaluations = relations
                .Where(relation => string.Equals(
                    relation.AnchorObjectStableId,
                    enteredBuildingId,
                    StringComparison.Ordinal))
                .OrderBy(relation => relation.RelationStableId, StringComparer.Ordinal)
                .Select(relation => EvaluateRelation(
                    relation,
                    activeConditions,
                    revisionMatches))
                .ToArray();

            return new SimulationWorldBuildingItemEligibilityPreviewResponse
            {
                SessionStableId = sessionStableId.Trim(),
                TileKey = tileKey,
                ObservedWorldRevision = currentWorldRevision,
                RuleRevision = SimulationWorldExplorationCodes.RuleRevision,
                Evaluations = evaluations,
                HasEligibleCandidate = evaluations.Any(value => value.IsEligible),
                StateChanged = false,
                SimulationOnly = true,
            };
        }

        public static string HashRelations(
            string tileKey,
            IEnumerable<SimulationWorldBuildingItemRelationResponse> relations)
        {
            var canonical = tileKey + "|" + SimulationWorldExplorationCodes.RuleRevision + "|"
                + string.Join(",", relations
                    .OrderBy(value => value.RelationStableId, StringComparer.Ordinal)
                    .Select(CanonicalRelation));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var value in bytes) builder.Append(value.ToString("x2"));
                return builder.ToString();
            }
        }

        private static SimulationWorldBuildingItemRelationResponse[]
            CreatePyeongchangFixtureRelations()
            => new[]
            {
                Relation(
                    "building-item:pyeongchang-farm:barn:potato-sample",
                    PyeongchangWorldExplorationFixtureIds.Barn,
                    "InteriorStorageA",
                    PyeongchangWorldExplorationFixtureIds.PotatoSample,
                    "대관령 감자 표본",
                    SimulationWorldExplorationCodes.RegionalFeature,
                    "world.item.regional-potato",
                    new[]
                    {
                        SimulationWorldExplorationCodes.PlayerInsideBuilding,
                        SimulationWorldExplorationCodes.FarmExplorationActive,
                        SimulationWorldExplorationCodes.HarvestContextActive,
                    },
                    -3d,
                    1.5d,
                    "평창군 밭 통계와 대관령 Farm 시나리오를 결합한 표현이며 실제 필지 작물 관측은 아닙니다."),
                Relation(
                    "building-item:pyeongchang-farm:silo:water-supply",
                    PyeongchangWorldExplorationFixtureIds.Silo,
                    "ServiceBayA",
                    PyeongchangWorldExplorationFixtureIds.BasicWaterSupply,
                    "기본 급수품",
                    SimulationWorldExplorationCodes.UniversalSupply,
                    "world.item.basic-water",
                    new[]
                    {
                        SimulationWorldExplorationCodes.PlayerInsideBuilding,
                        SimulationWorldExplorationCodes.FarmExplorationActive,
                        SimulationWorldExplorationCodes.SupplyInspectionActive,
                    },
                    0d,
                    -2d,
                    "시설 보급 점검 시나리오에서만 후보가 되는 보편 급수품입니다."),
                Relation(
                    "building-item:pyeongchang-farm:barn:evidence-note",
                    PyeongchangWorldExplorationFixtureIds.Barn,
                    "InformationDeskA",
                    PyeongchangWorldExplorationFixtureIds.RegionalEvidenceNote,
                    "지역 근거 기록",
                    SimulationWorldExplorationCodes.UniversalSupply,
                    "world.item.field-note",
                    new[]
                    {
                        SimulationWorldExplorationCodes.PlayerInsideBuilding,
                        SimulationWorldExplorationCodes.FarmExplorationActive,
                        SimulationWorldExplorationCodes.RegionalEvidenceReviewActive,
                    },
                    4d,
                    -1d,
                    "공공데이터의 관측 사실과 시나리오 표현을 구분하는 안내 후보입니다."),
            };

        private static void ValidateRelations(
            IReadOnlyCollection<SimulationWorldBuildingItemRelationResponse> relations,
            IReadOnlyCollection<SimulationWorldTileObjectPlacementResponse> buildings)
        {
            if (relations.Select(value => value.RelationStableId)
                    .Distinct(StringComparer.Ordinal).Count() != relations.Count)
                throw new SimulationContractException(
                    "SimulationWorldBuildingItemRelationStableIdDuplicate");

            var anchors = buildings.ToDictionary(
                value => value.ObjectStableId,
                value => value,
                StringComparer.Ordinal);
            foreach (var relation in relations)
            {
                if (!anchors.TryGetValue(relation.AnchorObjectStableId, out var anchor))
                    throw new SimulationContractException(
                        SimulationWorldExplorationCodes.AnchorMissing + ":"
                        + relation.AnchorObjectStableId);
                if (anchor.ObjectTypeCode != SimulationWorldStreamCodes.BuildingObject
                    || relation.AnchorObjectTypeCode != SimulationWorldStreamCodes.BuildingObject)
                    throw new SimulationContractException(
                        "SimulationWorldBuildingItemAnchorTypeInvalid");
                if (string.IsNullOrWhiteSpace(relation.AnchorSocketCode)
                    || string.IsNullOrWhiteSpace(relation.ItemCode)
                    || string.IsNullOrWhiteSpace(relation.VisualKey)
                    || relation.RequiredConditionCodes.Length == 0
                    || relation.RequiredConditionCodes.Distinct(StringComparer.Ordinal).Count()
                        != relation.RequiredConditionCodes.Length)
                    throw new SimulationContractException(
                        "SimulationWorldBuildingItemRelationInvalid");
                if (Math.Abs(relation.SocketOffsetXMeters) > anchor.FootprintWidthMeters / 2d
                    || Math.Abs(relation.SocketOffsetYMeters) > anchor.FootprintDepthMeters / 2d)
                    throw new SimulationContractException(
                        "SimulationWorldBuildingItemSocketOutsideAnchor");
            }
        }

        private static void ValidateRequest(
            string sessionStableId,
            SimulationWorldBuildingItemEligibilityPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdMissing");
            if (request.ObservedWorldRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (string.IsNullOrWhiteSpace(request.EnteredBuildingStableId))
                throw new SimulationContractException(
                    SimulationWorldExplorationCodes.EnteredBuildingStableIdMissing);
        }

        private static SimulationWorldBuildingItemEvaluationResponse EvaluateRelation(
            SimulationWorldBuildingItemRelationResponse relation,
            ISet<string> activeConditions,
            bool revisionMatches)
        {
            var missing = relation.RequiredConditionCodes
                .Where(value => !activeConditions.Contains(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            var blocks = new List<string>();
            if (!revisionMatches)
                blocks.Add(SimulationWorldExplorationCodes.ExpectedRevisionMismatch);
            if (missing.Length > 0)
                blocks.Add(SimulationWorldExplorationCodes.ConditionMissing);
            var eligible = blocks.Count == 0;
            return new SimulationWorldBuildingItemEvaluationResponse
            {
                RelationStableId = relation.RelationStableId,
                AnchorObjectStableId = relation.AnchorObjectStableId,
                ItemCode = relation.ItemCode,
                KoreanName = relation.KoreanName,
                EligibilityStateCode = eligible
                    ? SimulationWorldExplorationCodes.Eligible
                    : SimulationWorldExplorationCodes.Ineligible,
                IsEligible = eligible,
                MissingConditionCodes = missing,
                BlockReasonCodes = blocks.ToArray(),
            };
        }

        private static string CanonicalRelation(
            SimulationWorldBuildingItemRelationResponse value)
            => string.Join(":", new[]
            {
                value.RelationStableId,
                value.AnchorObjectStableId,
                value.AnchorObjectTypeCode,
                value.AnchorSocketCode,
                value.ItemCode,
                value.KoreanName,
                value.ItemKindCode,
                value.VisualKey,
                value.EvidenceKindCode,
                value.EvidenceKorean,
                string.Join("+", value.RequiredConditionCodes
                    .OrderBy(condition => condition, StringComparer.Ordinal)),
                value.SocketOffsetXMeters.ToString("R", CultureInfo.InvariantCulture),
                value.SocketOffsetYMeters.ToString("R", CultureInfo.InvariantCulture),
                value.InitialStateCode,
                value.PresentationOnly.ToString(),
            });

        private static SimulationWorldBuildingItemRelationResponse Relation(
            string relationId,
            string anchorId,
            string socketCode,
            string itemCode,
            string koreanName,
            string kind,
            string visualKey,
            string[] conditions,
            double x,
            double y,
            string evidence)
            => new SimulationWorldBuildingItemRelationResponse
            {
                RelationStableId = relationId,
                AnchorObjectStableId = anchorId,
                AnchorObjectTypeCode = SimulationWorldStreamCodes.BuildingObject,
                AnchorSocketCode = socketCode,
                ItemCode = itemCode,
                KoreanName = koreanName,
                ItemKindCode = kind,
                VisualKey = visualKey,
                EvidenceKindCode = SimulationWorldExplorationCodes.SimulationScenario,
                EvidenceKorean = evidence,
                RequiredConditionCodes = conditions,
                SocketOffsetXMeters = x,
                SocketOffsetYMeters = y,
                InitialStateCode = SimulationWorldExplorationCodes.PendingRuleEvaluation,
                PresentationOnly = true,
            };
    }
}
