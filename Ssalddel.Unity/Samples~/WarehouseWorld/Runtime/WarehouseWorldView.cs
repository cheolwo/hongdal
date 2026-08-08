using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Warehouse;
using UnityEngine;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldView : MonoBehaviour
    {
        [SerializeField] private Transform objectRoot = null!;
        [SerializeField] private WarehouseWorldObjectView objectTemplate = null!;
        [SerializeField] private WarehouseNpcView npcTemplate = null!;
        [SerializeField] private TextMesh statusLabel = null!;
        [SerializeField] private Transform inboundDock = null!, storageZone = null!, rackZone = null!, outboundStaging = null!;
        private readonly Dictionary<string, WarehouseWorldObjectView> objects = new(StringComparer.Ordinal);
        private readonly Dictionary<string, WarehouseNpcView> npcs = new(StringComparer.Ordinal);
        public int ObjectCount => objects.Count + npcs.Count;

        public void Configure(Transform root, WarehouseWorldObjectView objectPrefab, WarehouseNpcView npcPrefab, TextMesh status,
            Transform inbound, Transform storage, Transform rack, Transform outbound)
        { objectRoot = root; objectTemplate = objectPrefab; npcTemplate = npcPrefab; statusLabel = status; inboundDock = inbound; storageZone = storage; rackZone = rack; outboundStaging = outbound; }

        public void ShowState(string state, string message = "") => statusLabel.text = state + (string.IsNullOrWhiteSpace(message) ? string.Empty : "\n" + message);
        public void Render(WarehouseWorldLoadResult result)
        {
            if (result.Changes != null)
            {
                foreach (var item in result.Changes.Removed) Remove(item.StableId);
                foreach (var item in result.Changes.Added) Add(item);
                if (result.Snapshot != null) RenderAll(result.Snapshot.Objects);
            }
            var message = result.Error == null
                ? $"재고 {result.Snapshot?.TotalAvailableQuantity ?? 0} · 예약 {result.Snapshot?.TotalReservedQuantity ?? 0}"
                : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name;
            ShowState(result.StateCode, message);
        }
        public bool ValidateWiring() => objectRoot != null && objectTemplate != null && objectTemplate.ValidateWiring()
            && npcTemplate != null && npcTemplate.ValidateWiring() && statusLabel != null && inboundDock != null && storageZone != null && rackZone != null && outboundStaging != null;

        private void Add(WarehouseWorldObject item)
        {
            if (item.Kind == "Npc") { var view = Instantiate(npcTemplate, objectRoot); view.name = "Npc_" + Safe(item.StableId); npcs.Add(item.StableId, view); }
            else { var view = Instantiate(objectTemplate, objectRoot); view.name = item.Kind + "_" + Safe(item.StableId); objects.Add(item.StableId, view); }
        }
        private void Remove(string id)
        {
            if (objects.Remove(id, out var item)) Destroy(item.gameObject);
            if (npcs.Remove(id, out var npc)) Destroy(npc.gameObject);
        }
        private void RenderAll(IReadOnlyList<WarehouseWorldObject> items)
        {
            var visualItems = items.Where(item => item.Kind != "Npc").ToArray();
            for (var index = 0; index < visualItems.Length; index++)
            {
                if (!objects.TryGetValue(visualItems[index].StableId, out var view)) throw new InvalidOperationException("WarehouseObjectMissing:" + visualItems[index].StableId);
                view.Render(visualItems[index], new Vector3((index % 4) * 2.8f, 0.6f, -(index / 4) * 2.6f));
            }
            foreach (var item in items.Where(item => item.Kind == "Npc"))
            {
                if (!npcs.TryGetValue(item.StableId, out var view)) throw new InvalidOperationException("WarehouseNpcMissing:" + item.StableId);
                view.Render(item, Waypoint(item.CurrentLocationCode), Waypoint(item.LocationCode));
            }
        }
        private Transform Waypoint(string key) => key switch
        {
            "warehouse.inbound-dock" => inboundDock, "warehouse.storage-zone" => storageZone,
            "warehouse.rack-zone" => rackZone, "warehouse.outbound-staging" => outboundStaging,
            _ => throw new InvalidOperationException("WarehouseWaypointUnknown:" + key),
        };
        private static string Safe(string value) => value.Replace(':', '_');
    }
}
