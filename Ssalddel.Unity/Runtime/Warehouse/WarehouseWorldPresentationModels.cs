using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Warehouse
{
    public static class WarehousePresentationVersions
    {
        public const string Perspective = "WarehouseManager";
        public const string VisualRule = "warehouse-primitive-visual-v1";
        public const string Contract = "warehouse-presentation-v1";
    }

    public static class WarehouseVisualStateCodes
    {
        public const string Inventory = "Inventory";
        public const string Task = "Task";
        public const string Npc = "Npc";
        public const string Handoff = "Handoff";
        public const string InboundTask = "InboundTask";
        public const string Cargo = "Cargo";
        public const string Vehicle = "Vehicle";
    }

    public sealed class WarehousePresentationItem
    {
        public string StableId { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string VisualStateCode { get; set; } = string.Empty;
        public string SocketKey { get; set; } = WarehouseLocationSocketKeys.UnassignedArea;
        public string CurrentSocketKey { get; set; } = WarehouseLocationSocketKeys.UnassignedArea;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public string[] RelatedStableIds { get; set; } = Array.Empty<string>();
        public string CanonicalRelationStableId { get; set; } = string.Empty;
        public string DetailText { get; set; } = string.Empty;
    }

    public sealed class WarehousePresentationSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public int TotalAvailableQuantity { get; set; }
        public int TotalReservedQuantity { get; set; }
        public WarehousePresentationItem[] Items { get; set; } = Array.Empty<WarehousePresentationItem>();
    }

    public sealed class WarehousePresentationChangeSet
    {
        public string[] AddedStableIds { get; set; } = Array.Empty<string>();
        public string[] UpdatedStableIds { get; set; } = Array.Empty<string>();
        public string[] RemovedStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class WarehousePresentationLoadModel
    {
        public string StateCode { get; set; } = WarehouseWorldLoadStateCodes.Idle;
        public string StatusMessage { get; set; } = string.Empty;
        public WarehousePresentationSnapshot? Snapshot { get; set; }
        public WarehousePresentationChangeSet? Changes { get; set; }
    }

    /// <summary>해석된 Warehouse World State를 View가 즉시 소비할 표현 계약으로 변환합니다.</summary>
    public sealed class WarehousePresenter
    {
        private readonly WarehouseLocationResolver locationResolver;
        private readonly WarehouseRelationResolver relationResolver;

        public WarehousePresenter(
            WarehouseLocationResolver locationResolver,
            WarehouseRelationResolver relationResolver)
        {
            this.locationResolver = locationResolver ?? throw new ArgumentNullException(nameof(locationResolver));
            this.relationResolver = relationResolver ?? throw new ArgumentNullException(nameof(relationResolver));
        }

        public WarehousePresentationLoadModel Present(WarehouseWorldLoadResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var snapshot = result.Snapshot == null ? null : PresentSnapshot(result.Snapshot);
            var statusMessage = result.Error == null
                ? $"재고 {snapshot?.TotalAvailableQuantity ?? 0} · 예약 {snapshot?.TotalReservedQuantity ?? 0}"
                : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name;

            return new WarehousePresentationLoadModel
            {
                StateCode = result.StateCode,
                StatusMessage = statusMessage,
                Snapshot = snapshot,
                Changes = result.Changes == null
                    ? null
                    : new WarehousePresentationChangeSet
                    {
                        AddedStableIds = result.Changes.Added.Select(item => item.StableId).ToArray(),
                        UpdatedStableIds = result.Changes.Updated.Select(item => item.StableId).ToArray(),
                        RemovedStableIds = result.Changes.Removed.Select(item => item.StableId).ToArray(),
                    },
            };
        }

        public WarehousePresentationSnapshot PresentSnapshot(WarehouseWorldSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var interpretationRevision = source.Lineage?.InterpretationRevision
                ?? "interpretation:legacy:" + source.Revision;
            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                interpretationRevision,
                WarehousePresentationVersions.Perspective,
                WarehousePresentationVersions.VisualRule,
                WarehousePresentationVersions.Contract);

            var items = source.Objects.Select(item => PresentItem(source, item)).ToArray();
            return new WarehousePresentationSnapshot
            {
                StableId = source.StableId,
                DataRevision = source.Revision,
                InterpretationRevision = interpretationRevision,
                PresentationRevision = presentationRevision,
                TotalAvailableQuantity = source.TotalAvailableQuantity,
                TotalReservedQuantity = source.TotalReservedQuantity,
                Items = items,
            };
        }

        private WarehousePresentationItem PresentItem(
            WarehouseWorldSnapshot snapshot,
            WarehouseWorldObject item)
        {
            var selection = relationResolver.Select(snapshot, item.StableId);
            var relatedIds = selection.Related.Select(value => value.StableId).ToArray();
            var destination = locationResolver.Resolve(item.LocationCode);
            var current = locationResolver.Resolve(item.CurrentLocationCode);
            return new WarehousePresentationItem
            {
                StableId = item.StableId,
                Kind = item.Kind,
                Title = item.Title,
                Status = item.Status,
                LabelText = item.Title + "\n" + item.Status + (item.Quantity > 0 ? " · " + item.Quantity : string.Empty),
                VisualStateCode = item.Kind,
                SocketKey = destination.SocketKey,
                CurrentSocketKey = item.Kind == "Npc" ? current.SocketKey : destination.SocketKey,
                Quantity = item.Quantity,
                ReservedQuantity = item.ReservedQuantity,
                RelatedStableIds = relatedIds,
                CanonicalRelationStableId = item.CanonicalRelationStableId,
                DetailText = Detail(item, relatedIds.Length),
            };
        }

        private static string Detail(WarehouseWorldObject item, int relatedCount)
        {
            var location = string.IsNullOrWhiteSpace(item.LocationCode) ? "미지정" : item.LocationCode;
            var quantity = item.Kind == "Inventory"
                ? $"가용 {item.Quantity} · 예약 {item.ReservedQuantity}"
                : item.Quantity > 0 ? "수량 " + item.Quantity : string.Empty;
            var boundary = item.Kind == "Inventory"
                ? "\n재고 투영값이며 물리 팔레트 수가 아님"
                : string.Empty;
            var relation = string.IsNullOrWhiteSpace(item.CanonicalRelationStableId)
                ? string.Empty
                : "\n원장 관계 " + item.CanonicalRelationStableId;
            return $"{item.Kind} · {item.Title}\n{item.Status} · 위치 {location}\n{quantity}\n연결 객체 {relatedCount}{relation}{boundary}";
        }
    }
}
