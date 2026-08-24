using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 마트관리자PerspectiveCodes
    {
        public const string Role = "MarketManager";
        public const string Zone = "urban-market";
        public const string ReviewReplenishment = "ReviewReplenishment";
    }

    public static class 마트관리자InteractionIntentCodes
    {
        public const string ReviewDataAttention = "ReviewDataAttention";
        public const string ReviewInbound = "ReviewInbound";
        public const string PreviewShelfReplenishment = "PreviewShelfReplenishment";
        public const string ReviewTaskProgress = "ReviewTaskProgress";
    }

    public static class 마트관리자PerspectiveVersions
    {
        public const string Contract = "urban-market-manager-perspective.v2";
        public const string Rule = "urban-market-manager-context.v2";
        public const string PresentationContract = "urban-market-manager-surface.v2";
        public const string VisualRule = "urban-market-manager-primitive-visual.v2";
    }

    /// <summary>
    /// Shared Interpretation의 진열 상태를 관리자 문맥에 맞게 보존합니다.
    /// 우선순위나 업무 queue를 만들지 않으며 원본 NeedCode와 차단 사유를 유지합니다.
    /// </summary>
    public sealed class 마트관리자진열상태
    {
        public WorldStableId ShelfWorldId { get; set; }
        public WorldStableId ProductWorldId { get; set; }
        public string NeedCode { get; set; } = string.Empty;
        public int DisplayQuantity { get; set; }
        public int DisplayCapacity { get; set; }
        public int TargetQuantity { get; set; }
        public int CandidateQuantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public bool CanPreviewRequest { get; set; }
        public bool IsSourcePlanComplete { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
        public WorldStableId[] SourceWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public 도심마트진열보충SourcePlanSegment[] SourcePlan { get; set; } = Array.Empty<도심마트진열보충SourcePlanSegment>();
    }

    public sealed class 마트관리자PerspectiveWorldState
    {
        public 도심마트운영업무WorldState SharedWorld { get; set; } = null!;
        public InterpretationPerspectiveContext Context { get; set; } = null!;
        public 마트관리자진열상태[] ShelfStates { get; set; } = Array.Empty<마트관리자진열상태>();
        public WorldStableId[] FocusWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public WorldRelation[] FocusRelations { get; set; } = Array.Empty<WorldRelation>();
        public string PerspectiveInterpretationRevision { get; set; } = string.Empty;
    }

    public sealed class 마트관리자PerspectiveInterpreter :
        IPerspectiveInterpreter<도심마트운영업무WorldState, InterpretationPerspectiveContext, 마트관리자PerspectiveWorldState>
    {
        public 마트관리자PerspectiveWorldState Interpret(
            도심마트운영업무WorldState world,
            InterpretationPerspectiveContext context)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (!string.Equals(context.RoleCode, 마트관리자PerspectiveCodes.Role, StringComparison.Ordinal))
                throw new InvalidOperationException("UrbanMarketManagerRoleRequired");
            if (!string.Equals(context.ZoneCode, 마트관리자PerspectiveCodes.Zone, StringComparison.Ordinal))
                throw new InvalidOperationException("UrbanMarketManagerZoneRequired");
            if (!string.Equals(context.IntentCode, 마트관리자PerspectiveCodes.ReviewReplenishment, StringComparison.Ordinal))
                throw new InvalidOperationException("UrbanMarketManagerIntentUnsupported:" + context.IntentCode);
            var expectedMode = world.SharedWorld.Mode == DataRuntimeMode.Operational
                ? WorldInterpretationMode.Operational
                : WorldInterpretationMode.Simulation;
            if (context.Mode != expectedMode)
                throw new InvalidOperationException("UrbanMarketManagerModeMismatch");
            if (context.FocusWorldId.HasValue
                && !world.SharedWorld.Graph.NodesById.ContainsKey(context.FocusWorldId.Value))
                throw new InvalidOperationException("UrbanMarketManagerFocusUnknown:" + context.FocusWorldId.Value.Value);

            var shelfStates = world.Replenishments
                .Select(ProjectState)
                .OrderBy(value => value.ShelfWorldId)
                .ToArray();
            var focusWorldIds = ResolveFocusWorldIds(world, context.FocusWorldId);
            var focusSet = focusWorldIds.ToHashSet();
            var focusRelations = world.SharedWorld.Relations
                .Where(value => focusSet.Contains(value.From) && focusSet.Contains(value.To))
                .OrderBy(value => value.From)
                .ThenBy(value => value.To)
                .ThenBy(value => value.Kind)
                .ToArray();
            var parameters = string.Join("|", new[]
            {
                context.RoleCode,
                context.IntentCode,
                context.ZoneCode,
                context.FocusWorldId?.Value ?? string.Empty,
                context.Mode.ToString(),
                string.Join(",", shelfStates.Select(value => value.ShelfWorldId.Value + ":" + value.NeedCode)),
            });
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                world.Lineage.Inputs,
                마트관리자PerspectiveVersions.Contract,
                마트관리자PerspectiveVersions.Rule,
                world.Lineage.InterpretationRevision + "|" + parameters);

            return new 마트관리자PerspectiveWorldState
            {
                SharedWorld = world,
                Context = context,
                ShelfStates = shelfStates,
                FocusWorldIds = focusWorldIds,
                FocusRelations = focusRelations,
                PerspectiveInterpretationRevision = revision,
            };
        }

        private static 마트관리자진열상태 ProjectState(도심마트진열보충WorldState source)
        {
            var intents = new List<string>();
            if (source.NeedCode == 도심마트ReplenishmentNeedCodes.DataInsufficient)
                intents.Add(마트관리자InteractionIntentCodes.ReviewDataAttention);
            if (source.NeedCode == 도심마트ReplenishmentNeedCodes.InboundRequired)
                intents.Add(마트관리자InteractionIntentCodes.ReviewInbound);
            if (source.NeedCode == 도심마트ReplenishmentNeedCodes.TaskAlreadyActive)
                intents.Add(마트관리자InteractionIntentCodes.ReviewTaskProgress);
            if (source.CanPreviewRequest)
                intents.Add(마트관리자InteractionIntentCodes.PreviewShelfReplenishment);

            return new 마트관리자진열상태
            {
                ShelfWorldId = source.ShelfWorldId,
                ProductWorldId = source.ProductWorldId,
                NeedCode = source.NeedCode,
                DisplayQuantity = source.DisplayQuantity,
                DisplayCapacity = source.DisplayCapacity,
                TargetQuantity = source.TargetQuantity,
                CandidateQuantity = source.CandidateQuantity,
                QuantityUnit = source.QuantityUnit,
                CanPreviewRequest = source.CanPreviewRequest,
                IsSourcePlanComplete = source.IsSourcePlanComplete,
                BlockReasonCodes = source.BlockReasonCodes,
                AllowedInteractionIntentCodes = intents
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                RuleRevision = source.RuleRevision,
                SourceWorldIds = source.SourceWorldIds
                    .Append(source.ShelfWorldId)
                    .Append(source.ProductWorldId)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToArray(),
                SourcePlan = source.SourcePlan,
            };
        }

        private static WorldStableId[] ResolveFocusWorldIds(
            도심마트운영업무WorldState world,
            WorldStableId? focusWorldId)
        {
            if (!focusWorldId.HasValue) return Array.Empty<WorldStableId>();
            return world.Replenishments
                .Where(value => value.ShelfWorldId == focusWorldId.Value
                                || value.ProductWorldId == focusWorldId.Value
                                || value.SourceWorldIds.Contains(focusWorldId.Value))
                .SelectMany(value => value.SourceWorldIds
                    .Append(value.ShelfWorldId)
                    .Append(value.ProductWorldId))
                .Append(focusWorldId.Value)
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
        }
    }

    public sealed class 도심마트ManagerPresentationContext
    {
        public string LocaleCode { get; set; } = "ko-KR";
        public string QualityTierCode { get; set; } = "Primitive";
    }

    public sealed class 도심마트ShelfSurfaceItem
    {
        public PresentationStableId StableId { get; set; }
        public WorldStableId ShelfWorldId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public int DisplayBoxCount { get; set; }
        public string QuantityText { get; set; } = string.Empty;
        public string VisualStateCode { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public bool IsHighlighted { get; set; }
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 도심마트TaskMarkerSurfaceItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public bool IsHighlighted { get; set; }
    }

    public sealed class 도심마트SourcePlanSurfaceItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string QuantityText { get; set; } = string.Empty;
        public string FromLabelText { get; set; } = string.Empty;
        public string ToLabelText { get; set; } = string.Empty;
    }

    public sealed class 도심마트DetailPanelSurfaceItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string QuantityText { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public string RuleText { get; set; } = string.Empty;
        public string BoundaryText { get; set; } = string.Empty;
    }

    public sealed class 도심마트PresentationSnapshot
    {
        public string PresentationRevision { get; set; } = string.Empty;
        public 도심마트ShelfSurfaceItem[] Shelves { get; set; } = Array.Empty<도심마트ShelfSurfaceItem>();
        public 도심마트TaskMarkerSurfaceItem[] TaskMarkers { get; set; } = Array.Empty<도심마트TaskMarkerSurfaceItem>();
        public 도심마트SourcePlanSurfaceItem[] SourcePlans { get; set; } = Array.Empty<도심마트SourcePlanSurfaceItem>();
        public 도심마트DetailPanelSurfaceItem[] Details { get; set; } = Array.Empty<도심마트DetailPanelSurfaceItem>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 도심마트ManagerVisualPolicy
    {
        public string Color(string needCode)
        {
            if (needCode == 도심마트ReplenishmentNeedCodes.DataInsufficient) return "Red";
            if (needCode == 도심마트ReplenishmentNeedCodes.InboundRequired) return "Orange";
            if (needCode == 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate) return "Yellow";
            if (needCode == 도심마트ReplenishmentNeedCodes.TaskAlreadyActive) return "Blue";
            return "Green";
        }

        public int DisplayBoxCount(int displayQuantity)
            => Math.Max(0, Math.Min(12, displayQuantity));
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 도심마트PresentationProjector :
        IPresentationProjector<마트관리자PerspectiveWorldState, 도심마트ManagerPresentationContext, 도심마트PresentationSnapshot>
    {
        private readonly 도심마트ManagerVisualPolicy visualPolicy;

        public 도심마트PresentationProjector(도심마트ManagerVisualPolicy visualPolicy)
            => this.visualPolicy = visualPolicy ?? throw new ArgumentNullException(nameof(visualPolicy));

        public 도심마트PresentationSnapshot Project(
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            if (perspective == null) throw new ArgumentNullException(nameof(perspective));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var products = perspective.SharedWorld.SharedWorld.Nodes
                .OfType<도심마트운영상품WorldNode>()
                .ToDictionary(value => value.StableId);
            var locations = perspective.SharedWorld.SharedWorld.Nodes
                .OfType<도심마트위치WorldNode>()
                .ToDictionary(value => value.StableId);
            var tasks = perspective.SharedWorld.SharedWorld.Nodes
                .OfType<도심마트작업WorldNode>()
                .ToArray();
            var revision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                perspective.PerspectiveInterpretationRevision,
                perspective.Context.RoleCode + ":" + perspective.Context.IntentCode,
                마트관리자PerspectiveVersions.VisualRule + ":" + context.QualityTierCode,
                마트관리자PerspectiveVersions.PresentationContract + ":" + context.LocaleCode);
            var focus = perspective.FocusWorldIds.ToHashSet();

            return new 도심마트PresentationSnapshot
            {
                PresentationRevision = revision,
                Shelves = perspective.ShelfStates
                    .Select(value => ShelfSurface(value, focus, perspective, context))
                    .ToArray(),
                TaskMarkers = tasks
                    .Where(value => value.TaskKindCode == 도심마트TaskKindCodes.ShelfReplenishment
                                    && value.StateCode != 도심마트TaskStateCodes.Completed)
                    .OrderBy(value => value.StableId)
                    .Select(value => TaskSurface(value, products[value.ProductWorldId], focus, perspective, context))
                    .ToArray(),
                SourcePlans = perspective.ShelfStates
                    .SelectMany(value => value.SourcePlan.Select((segment, index) =>
                        SourcePlanSurface(value, segment, index, locations, perspective, context)))
                    .ToArray(),
                Details = FocusedShelfStates(perspective.ShelfStates, perspective.Context.FocusWorldId)
                    .Select(value => DetailSurface(value, products[value.ProductWorldId], perspective, context))
                    .ToArray(),
            };
        }

        private 도심마트ShelfSurfaceItem ShelfSurface(
            마트관리자진열상태 source,
            ISet<WorldStableId> focus,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-shelf:" + source.ShelfWorldId.Value);
            var highlighted = focus.Contains(source.ShelfWorldId) || focus.Contains(source.ProductWorldId);
            return new 도심마트ShelfSurfaceItem
            {
                StableId = id,
                ShelfWorldId = source.ShelfWorldId,
                Identity = new PresentationIdentityLineage(id, source.SourceWorldIds),
                PresentationRevision = ItemRevision("shelf", perspective, context,
                    source.ShelfWorldId.Value,
                    source.DisplayQuantity.ToString(CultureInfo.InvariantCulture),
                    source.DisplayCapacity.ToString(CultureInfo.InvariantCulture),
                    source.NeedCode,
                    string.Join(",", source.BlockReasonCodes),
                    highlighted.ToString()),
                DisplayBoxCount = visualPolicy.DisplayBoxCount(source.DisplayQuantity),
                QuantityText = source.DisplayQuantity + "/" + source.DisplayCapacity + " " + source.QuantityUnit,
                VisualStateCode = source.NeedCode,
                ColorCode = visualPolicy.Color(source.NeedCode),
                IsHighlighted = highlighted,
                AllowedInteractionIntentCodes = source.AllowedInteractionIntentCodes,
            };
        }

        private static 도심마트TaskMarkerSurfaceItem TaskSurface(
            도심마트작업WorldNode source,
            도심마트운영상품WorldNode product,
            ISet<WorldStableId> focus,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-task:" + source.StableId.Value);
            var lineage = new[] { source.StableId, source.ProductWorldId, source.SourceInventoryWorldId, source.TargetShelfWorldId };
            var highlighted = lineage.Any(focus.Contains);
            return new 도심마트TaskMarkerSurfaceItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, lineage),
                PresentationRevision = ItemRevision("task", perspective, context,
                    source.StableId.Value,
                    source.StateCode,
                    source.Quantity.ToString(CultureInfo.InvariantCulture),
                    highlighted.ToString()),
                StateCode = source.StateCode,
                LabelText = product.상품명 + " " + source.Quantity + " " + source.QuantityUnit,
                IsHighlighted = highlighted,
            };
        }

        private static 도심마트SourcePlanSurfaceItem SourcePlanSurface(
            마트관리자진열상태 shelfState,
            도심마트진열보충SourcePlanSegment source,
            int index,
            IReadOnlyDictionary<WorldStableId, 도심마트위치WorldNode> locations,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-source-plan:" + shelfState.ShelfWorldId.Value + ":" + source.InventoryWorldId.Value);
            var lineage = new[] { shelfState.ShelfWorldId, shelfState.ProductWorldId, source.InventoryWorldId, source.LocationWorldId };
            return new 도심마트SourcePlanSurfaceItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, lineage),
                PresentationRevision = ItemRevision("source-plan", perspective, context,
                    shelfState.ShelfWorldId.Value,
                    source.InventoryWorldId.Value,
                    source.Quantity.ToString(CultureInfo.InvariantCulture),
                    source.QuantityUnit),
                Sequence = index + 1,
                QuantityText = source.Quantity + " " + source.QuantityUnit,
                FromLabelText = locations.TryGetValue(source.LocationWorldId, out var location)
                    ? location.이름
                    : source.LocationWorldId.Value,
                ToLabelText = shelfState.ShelfWorldId.Value,
            };
        }

        private static 도심마트DetailPanelSurfaceItem DetailSurface(
            마트관리자진열상태 source,
            도심마트운영상품WorldNode product,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-detail:" + source.ShelfWorldId.Value);
            return new 도심마트DetailPanelSurfaceItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, source.SourceWorldIds),
                PresentationRevision = ItemRevision("detail", perspective, context,
                    source.ShelfWorldId.Value,
                    source.NeedCode,
                    string.Join(",", source.BlockReasonCodes),
                    source.RuleRevision),
                TitleText = product.상품명 + " 진열 상세",
                QuantityText = "진열 " + source.DisplayQuantity + "/목표 " + source.TargetQuantity + " " + source.QuantityUnit,
                ReasonText = string.Join(", ", source.BlockReasonCodes),
                RuleText = source.RuleRevision,
                BoundaryText = "후보와 표시만 제공하며 서버 재고·작업을 변경하지 않습니다.",
            };
        }

        private static IEnumerable<마트관리자진열상태> FocusedShelfStates(
            IEnumerable<마트관리자진열상태> states,
            WorldStableId? focusWorldId)
        {
            if (!focusWorldId.HasValue) return Array.Empty<마트관리자진열상태>();
            var items = states.ToArray();
            var direct = items.Where(value => value.ShelfWorldId == focusWorldId.Value
                                              || value.ProductWorldId == focusWorldId.Value)
                .ToArray();
            return direct.Length > 0
                ? direct
                : items.Where(value => value.SourceWorldIds.Contains(focusWorldId.Value));
        }

        private static string ItemRevision(
            string surface,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context,
            params string[] values)
            => WorldDataFlowRevisionCalculator.CalculatePresentation(
                "interpretation-item:" + string.Join("|", values),
                perspective.Context.RoleCode + ":" + perspective.Context.IntentCode,
                마트관리자PerspectiveVersions.VisualRule + ":" + surface + ":" + context.QualityTierCode,
                마트관리자PerspectiveVersions.PresentationContract + ":" + context.LocaleCode);
    }
}
