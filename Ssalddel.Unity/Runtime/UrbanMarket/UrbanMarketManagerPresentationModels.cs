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

    public static class 마트관리자QueueCodes
    {
        public const string UrgentActions = "UrgentActions";
        public const string PendingActions = "PendingActions";
        public const string InProgress = "InProgress";
        public const string DataAttention = "DataAttention";
        public const string NoActionNeeded = "NoActionNeeded";
    }

    public static class 마트관리자PriorityReasonCodes
    {
        public const string DataIntegrityAttention = "DataIntegrityAttention";
        public const string ShelfEmpty = "ShelfEmpty";
        public const string InboundReviewRequired = "InboundReviewRequired";
        public const string ReplenishmentReady = "ReplenishmentReady";
        public const string TaskProgressReview = "TaskProgressReview";
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
        public const string Contract = "urban-market-manager-perspective.v1";
        public const string Rule = "urban-market-manager-priority.v1";
        public const string PresentationContract = "urban-market-manager-surface.v1";
        public const string VisualRule = "urban-market-manager-primitive-visual.v1";
    }

    public sealed class 마트관리자업무QueueItem
    {
        public string QueueCode { get; set; } = string.Empty;
        public int PriorityScore { get; set; }
        public string[] PriorityReasonCodes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
        public WorldStableId ShelfWorldId { get; set; }
        public WorldStableId ProductWorldId { get; set; }
        public string NeedCode { get; set; } = string.Empty;
        public int DisplayQuantity { get; set; }
        public int TargetQuantity { get; set; }
        public int CandidateQuantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public bool CanPreviewRequest { get; set; }
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
        public WorldStableId[] SourceWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public 도심마트진열보충SourcePlanSegment[] SourcePlan { get; set; } = Array.Empty<도심마트진열보충SourcePlanSegment>();
    }

    public sealed class 마트관리자PerspectiveWorldState
    {
        public 도심마트운영업무WorldState SharedWorld { get; set; } = null!;
        public InterpretationPerspectiveContext Context { get; set; } = null!;
        public 마트관리자업무QueueItem[] UrgentActions { get; set; } = Array.Empty<마트관리자업무QueueItem>();
        public 마트관리자업무QueueItem[] PendingActions { get; set; } = Array.Empty<마트관리자업무QueueItem>();
        public 마트관리자업무QueueItem[] InProgress { get; set; } = Array.Empty<마트관리자업무QueueItem>();
        public 마트관리자업무QueueItem[] DataAttention { get; set; } = Array.Empty<마트관리자업무QueueItem>();
        public int NoActionNeededCount { get; set; }
        public WorldStableId[] FocusWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public WorldRelation[] FocusRelations { get; set; } = Array.Empty<WorldRelation>();
        public string PerspectiveInterpretationRevision { get; set; } = string.Empty;

        public 마트관리자업무QueueItem[] ActionQueue
            => DataAttention
                .Concat(UrgentActions)
                .Concat(PendingActions)
                .Concat(InProgress)
                .ToArray();
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

            var items = world.Replenishments
                .Select(Classify)
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
                string.Join(",", items.Select(value => value.QueueCode + ":" + value.ShelfWorldId.Value)),
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
                DataAttention = Queue(items, 마트관리자QueueCodes.DataAttention),
                UrgentActions = Queue(items, 마트관리자QueueCodes.UrgentActions),
                PendingActions = Queue(items, 마트관리자QueueCodes.PendingActions),
                InProgress = Queue(items, 마트관리자QueueCodes.InProgress),
                NoActionNeededCount = items.Count(value => value.QueueCode == 마트관리자QueueCodes.NoActionNeeded),
                FocusWorldIds = focusWorldIds,
                FocusRelations = focusRelations,
                PerspectiveInterpretationRevision = revision,
            };
        }

        private static 마트관리자업무QueueItem Classify(도심마트진열보충WorldState source)
        {
            string queueCode;
            int score;
            var reasons = new List<string>();
            var intents = new List<string>();
            if (source.NeedCode == 도심마트ReplenishmentNeedCodes.DataInsufficient)
            {
                queueCode = 마트관리자QueueCodes.DataAttention;
                score = 400;
                reasons.Add(마트관리자PriorityReasonCodes.DataIntegrityAttention);
                reasons.AddRange(source.BlockReasonCodes);
                intents.Add(마트관리자InteractionIntentCodes.ReviewDataAttention);
            }
            else if (source.DisplayQuantity == 0
                     || source.NeedCode == 도심마트ReplenishmentNeedCodes.InboundRequired)
            {
                queueCode = 마트관리자QueueCodes.UrgentActions;
                score = 300;
                if (source.DisplayQuantity == 0) reasons.Add(마트관리자PriorityReasonCodes.ShelfEmpty);
                if (source.NeedCode == 도심마트ReplenishmentNeedCodes.InboundRequired)
                {
                    reasons.Add(마트관리자PriorityReasonCodes.InboundReviewRequired);
                    intents.Add(마트관리자InteractionIntentCodes.ReviewInbound);
                }
                if (source.CanPreviewRequest)
                {
                    reasons.Add(마트관리자PriorityReasonCodes.ReplenishmentReady);
                    intents.Add(마트관리자InteractionIntentCodes.PreviewShelfReplenishment);
                }
                reasons.AddRange(source.BlockReasonCodes);
            }
            else if (source.NeedCode == 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate)
            {
                queueCode = 마트관리자QueueCodes.PendingActions;
                score = 200;
                reasons.Add(마트관리자PriorityReasonCodes.ReplenishmentReady);
                reasons.AddRange(source.BlockReasonCodes);
                if (source.CanPreviewRequest)
                    intents.Add(마트관리자InteractionIntentCodes.PreviewShelfReplenishment);
            }
            else if (source.NeedCode == 도심마트ReplenishmentNeedCodes.TaskAlreadyActive)
            {
                queueCode = 마트관리자QueueCodes.InProgress;
                score = 100;
                reasons.Add(마트관리자PriorityReasonCodes.TaskProgressReview);
                reasons.AddRange(source.BlockReasonCodes);
                intents.Add(마트관리자InteractionIntentCodes.ReviewTaskProgress);
            }
            else
            {
                queueCode = 마트관리자QueueCodes.NoActionNeeded;
                score = 0;
            }

            return new 마트관리자업무QueueItem
            {
                QueueCode = queueCode,
                PriorityScore = score,
                PriorityReasonCodes = reasons.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                RuleRevision = source.RuleRevision,
                ShelfWorldId = source.ShelfWorldId,
                ProductWorldId = source.ProductWorldId,
                NeedCode = source.NeedCode,
                DisplayQuantity = source.DisplayQuantity,
                TargetQuantity = source.TargetQuantity,
                CandidateQuantity = source.CandidateQuantity,
                QuantityUnit = source.QuantityUnit,
                CanPreviewRequest = source.CanPreviewRequest,
                AllowedInteractionIntentCodes = intents.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceWorldIds = source.SourceWorldIds
                    .Append(source.ShelfWorldId)
                    .Append(source.ProductWorldId)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToArray(),
                SourcePlan = source.SourcePlan,
            };
        }

        private static 마트관리자업무QueueItem[] Queue(
            IEnumerable<마트관리자업무QueueItem> items,
            string queueCode)
            => items
                .Where(value => value.QueueCode == queueCode)
                .OrderByDescending(value => value.PriorityScore)
                .ThenBy(value => value.ShelfWorldId)
                .ToArray();

        private static WorldStableId[] ResolveFocusWorldIds(
            도심마트운영업무WorldState world,
            WorldStableId? focusWorldId)
        {
            if (!focusWorldId.HasValue) return Array.Empty<WorldStableId>();
            var related = world.Replenishments
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
            return related;
        }
    }

    public sealed class 도심마트ManagerPresentationContext
    {
        public string LocaleCode { get; set; } = "ko-KR";
        public string QualityTierCode { get; set; } = "Primitive";
    }

    public sealed class 도심마트ManagerSummarySurface
    {
        public PresentationStableId StableId { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public int RefreshIntervalSeconds { get; set; }
        public int UrgentCount { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int DataAttentionCount { get; set; }
        public int HiddenNoActionCount { get; set; }
        public string ModeCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트PriorityQueueSurfaceItem
    {
        public PresentationStableId StableId { get; set; }
        public PresentationIdentityLineage Identity { get; set; } = null!;
        public string PresentationRevision { get; set; } = string.Empty;
        public string QueueCode { get; set; } = string.Empty;
        public int PriorityScore { get; set; }
        public string[] PriorityReasonCodes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public bool IsFocused { get; set; }
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
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
        public 도심마트ManagerSummarySurface ManagerSummary { get; set; } = null!;
        public 도심마트PriorityQueueSurfaceItem[] PriorityQueue { get; set; } = Array.Empty<도심마트PriorityQueueSurfaceItem>();
        public 도심마트ShelfSurfaceItem[] Shelves { get; set; } = Array.Empty<도심마트ShelfSurfaceItem>();
        public 도심마트TaskMarkerSurfaceItem[] TaskMarkers { get; set; } = Array.Empty<도심마트TaskMarkerSurfaceItem>();
        public 도심마트SourcePlanSurfaceItem[] SourcePlans { get; set; } = Array.Empty<도심마트SourcePlanSurfaceItem>();
        public 도심마트DetailPanelSurfaceItem[] Details { get; set; } = Array.Empty<도심마트DetailPanelSurfaceItem>();
    }

    public sealed class 도심마트ManagerVisualPolicy
    {
        public string Color(string queueCode)
        {
            if (queueCode == 마트관리자QueueCodes.DataAttention) return "Red";
            if (queueCode == 마트관리자QueueCodes.UrgentActions) return "Orange";
            if (queueCode == 마트관리자QueueCodes.PendingActions) return "Yellow";
            if (queueCode == 마트관리자QueueCodes.InProgress) return "Blue";
            return "Green";
        }

        public int DisplayBoxCount(int displayQuantity)
            => Math.Max(0, Math.Min(12, displayQuantity));
    }

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
            var queue = perspective.ActionQueue;
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
                ManagerSummary = Summary(perspective, context),
                PriorityQueue = queue.Select(value => QueueSurface(value, products[value.ProductWorldId], perspective, context)).ToArray(),
                Shelves = perspective.SharedWorld.Replenishments
                    .Select(value => ShelfSurface(value, QueueForShelf(queue, value.ShelfWorldId), focus, perspective, context))
                    .ToArray(),
                TaskMarkers = tasks
                    .Where(value => value.TaskKindCode == 도심마트TaskKindCodes.ShelfReplenishment
                                    && value.StateCode != 도심마트TaskStateCodes.Completed)
                    .OrderBy(value => value.StableId)
                    .Select(value => TaskSurface(value, products[value.ProductWorldId], focus, perspective, context))
                    .ToArray(),
                SourcePlans = queue
                    .SelectMany(value => value.SourcePlan.Select((segment, index) =>
                        SourcePlanSurface(value, segment, index, locations, perspective, context)))
                    .ToArray(),
                Details = FocusedQueueItems(queue, perspective.Context.FocusWorldId)
                    .Select(value => DetailSurface(value, products[value.ProductWorldId], perspective, context))
                    .ToArray(),
            };
        }

        private static 도심마트ManagerSummarySurface Summary(
            마트관리자PerspectiveWorldState source,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-manager-summary:" + source.SharedWorld.SharedWorld.StableId.Value);
            return new 도심마트ManagerSummarySurface
            {
                StableId = id,
                PresentationRevision = ItemRevision("summary", source, context,
                    source.UrgentActions.Length.ToString(CultureInfo.InvariantCulture),
                    source.PendingActions.Length.ToString(CultureInfo.InvariantCulture),
                    source.InProgress.Length.ToString(CultureInfo.InvariantCulture),
                    source.DataAttention.Length.ToString(CultureInfo.InvariantCulture),
                    source.NoActionNeededCount.ToString(CultureInfo.InvariantCulture)),
                SummaryText = "긴급 " + source.UrgentActions.Length
                              + " · 대기 " + source.PendingActions.Length
                              + " · 진행 " + source.InProgress.Length
                              + " · Data 확인 " + source.DataAttention.Length,
                RefreshIntervalSeconds = 30,
                UrgentCount = source.UrgentActions.Length,
                PendingCount = source.PendingActions.Length,
                InProgressCount = source.InProgress.Length,
                DataAttentionCount = source.DataAttention.Length,
                HiddenNoActionCount = source.NoActionNeededCount,
                ModeCode = source.Context.Mode.ToString(),
            };
        }

        private 도심마트PriorityQueueSurfaceItem QueueSurface(
            마트관리자업무QueueItem source,
            도심마트운영상품WorldNode product,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-manager-queue:" + source.ShelfWorldId.Value);
            var focused = perspective.Context.FocusWorldId.HasValue
                          && source.SourceWorldIds.Contains(perspective.Context.FocusWorldId.Value);
            return new 도심마트PriorityQueueSurfaceItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, source.SourceWorldIds),
                PresentationRevision = ItemRevision("queue", perspective, context,
                    source.ShelfWorldId.Value, source.QueueCode, source.PriorityScore.ToString(CultureInfo.InvariantCulture),
                    string.Join(",", source.PriorityReasonCodes), source.RuleRevision, focused.ToString()),
                QueueCode = source.QueueCode,
                PriorityScore = source.PriorityScore,
                PriorityReasonCodes = source.PriorityReasonCodes,
                RuleRevision = source.RuleRevision,
                TitleText = product.상품명 + " 진열",
                SummaryText = source.DisplayQuantity + "/" + source.TargetQuantity + " " + source.QuantityUnit
                              + (source.CandidateQuantity > 0 ? " · 보충 " + source.CandidateQuantity : string.Empty),
                ColorCode = visualPolicy.Color(source.QueueCode),
                IsFocused = focused,
                AllowedInteractionIntentCodes = source.AllowedInteractionIntentCodes,
            };
        }

        private 도심마트ShelfSurfaceItem ShelfSurface(
            도심마트진열보충WorldState source,
            마트관리자업무QueueItem? queue,
            ISet<WorldStableId> focus,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-shelf:" + source.ShelfWorldId.Value);
            var queueCode = queue?.QueueCode ?? 마트관리자QueueCodes.NoActionNeeded;
            var highlighted = focus.Contains(source.ShelfWorldId) || focus.Contains(source.ProductWorldId);
            var lineage = source.SourceWorldIds.Append(source.ShelfWorldId).Append(source.ProductWorldId).Distinct().ToArray();
            return new 도심마트ShelfSurfaceItem
            {
                StableId = id,
                ShelfWorldId = source.ShelfWorldId,
                Identity = new PresentationIdentityLineage(id, lineage),
                PresentationRevision = ItemRevision("shelf", perspective, context,
                    source.ShelfWorldId.Value, source.DisplayQuantity.ToString(CultureInfo.InvariantCulture),
                    source.DisplayCapacity.ToString(CultureInfo.InvariantCulture), queueCode, highlighted.ToString()),
                DisplayBoxCount = visualPolicy.DisplayBoxCount(source.DisplayQuantity),
                QuantityText = source.DisplayQuantity + "/" + source.DisplayCapacity + " " + source.QuantityUnit,
                VisualStateCode = queueCode,
                ColorCode = visualPolicy.Color(queueCode),
                IsHighlighted = highlighted,
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
                    source.StableId.Value, source.StateCode, source.Quantity.ToString(CultureInfo.InvariantCulture), highlighted.ToString()),
                StateCode = source.StateCode,
                LabelText = product.상품명 + " " + source.Quantity + " " + source.QuantityUnit,
                IsHighlighted = highlighted,
            };
        }

        private static 도심마트SourcePlanSurfaceItem SourcePlanSurface(
            마트관리자업무QueueItem queue,
            도심마트진열보충SourcePlanSegment source,
            int index,
            IReadOnlyDictionary<WorldStableId, 도심마트위치WorldNode> locations,
            마트관리자PerspectiveWorldState perspective,
            도심마트ManagerPresentationContext context)
        {
            var id = new PresentationStableId("urban-market-source-plan:" + queue.ShelfWorldId.Value + ":" + source.InventoryWorldId.Value);
            var lineage = new[] { queue.ShelfWorldId, queue.ProductWorldId, source.InventoryWorldId, source.LocationWorldId };
            return new 도심마트SourcePlanSurfaceItem
            {
                StableId = id,
                Identity = new PresentationIdentityLineage(id, lineage),
                PresentationRevision = ItemRevision("source-plan", perspective, context,
                    queue.ShelfWorldId.Value, source.InventoryWorldId.Value,
                    source.Quantity.ToString(CultureInfo.InvariantCulture), source.QuantityUnit),
                Sequence = index + 1,
                QuantityText = source.Quantity + " " + source.QuantityUnit,
                FromLabelText = locations.TryGetValue(source.LocationWorldId, out var location)
                    ? location.이름
                    : source.LocationWorldId.Value,
                ToLabelText = queue.ShelfWorldId.Value,
            };
        }

        private static 도심마트DetailPanelSurfaceItem DetailSurface(
            마트관리자업무QueueItem source,
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
                    source.ShelfWorldId.Value, source.QueueCode, string.Join(",", source.PriorityReasonCodes), source.RuleRevision),
                TitleText = product.상품명 + " 진열 상세",
                QuantityText = "진열 " + source.DisplayQuantity + "/목표 " + source.TargetQuantity + " " + source.QuantityUnit,
                ReasonText = string.Join(", ", source.PriorityReasonCodes),
                RuleText = source.RuleRevision,
                BoundaryText = "후보와 표시만 제공하며 서버 재고·작업을 변경하지 않습니다.",
            };
        }

        private static 마트관리자업무QueueItem? QueueForShelf(
            IEnumerable<마트관리자업무QueueItem> queue,
            WorldStableId shelfWorldId)
            => queue.FirstOrDefault(value => value.ShelfWorldId == shelfWorldId);

        private static IEnumerable<마트관리자업무QueueItem> FocusedQueueItems(
            IEnumerable<마트관리자업무QueueItem> queue,
            WorldStableId? focusWorldId)
        {
            if (!focusWorldId.HasValue) return Array.Empty<마트관리자업무QueueItem>();
            var items = queue.ToArray();
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
