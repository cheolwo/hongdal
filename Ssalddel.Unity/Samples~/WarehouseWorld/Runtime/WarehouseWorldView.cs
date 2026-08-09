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
        [SerializeField] private TextMesh detailLabel = null!;
        [SerializeField] private GameObject detailPanelRoot = null!;
        [SerializeField] private Transform inboundDock = null!, storageZone = null!, rackZone = null!, outboundStaging = null!, approach = null!, staffEntry = null!, inspectionZone = null!, vehicleExit = null!, unassignedArea = null!;
        private readonly Dictionary<string, WarehouseWorldObjectView> objects = new(StringComparer.Ordinal);
        private readonly Dictionary<string, WarehouseNpcView> npcs = new(StringComparer.Ordinal);
        private WarehousePresentationSnapshot? currentSnapshot;
        private string selectedStableId = string.Empty;
        public int ObjectCount => objects.Count + npcs.Count;

        public void Configure(Transform root, WarehouseWorldObjectView objectPrefab, WarehouseNpcView npcPrefab, TextMesh status,
            TextMesh detail, GameObject detailPanel, Transform inbound, Transform storage, Transform rack, Transform outbound,
            Transform inboundApproach, Transform workerEntry, Transform inspection, Transform exit, Transform unassigned)
        { objectRoot = root; objectTemplate = objectPrefab; npcTemplate = npcPrefab; statusLabel = status; detailLabel = detail; detailPanelRoot = detailPanel; inboundDock = inbound; storageZone = storage; rackZone = rack; outboundStaging = outbound; approach = inboundApproach; staffEntry = workerEntry; inspectionZone = inspection; vehicleExit = exit; unassignedArea = unassigned; }

        public void ShowState(string state, string message = "") => statusLabel.text = state + (string.IsNullOrWhiteSpace(message) ? string.Empty : "\n" + message);
        public void Render(WarehousePresentationLoadModel result)
        {
            if (result.Changes != null)
            {
                foreach (var stableId in result.Changes.RemovedStableIds) Remove(stableId);
                if (result.Snapshot != null)
                {
                    var index = result.Snapshot.Items.ToDictionary(item => item.StableId, StringComparer.Ordinal);
                    foreach (var stableId in result.Changes.AddedStableIds)
                    {
                        if (!index.TryGetValue(stableId, out var item))
                            throw new InvalidOperationException("WarehousePresentationItemMissing:" + stableId);
                        Add(item);
                    }
                }
            }
            if (result.Snapshot != null)
            {
                currentSnapshot = result.Snapshot;
                RenderAll(result.Snapshot.Items);
                RestoreSelection();
            }
            ShowState(result.StateCode, result.StatusMessage);
        }
        public bool ValidateWiring() => objectRoot != null && objectTemplate != null && objectTemplate.ValidateWiring()
            && npcTemplate != null && npcTemplate.ValidateWiring() && statusLabel != null && detailLabel != null && detailPanelRoot != null
            && inboundDock != null && storageZone != null && rackZone != null && outboundStaging != null
            && approach != null && staffEntry != null && inspectionZone != null && vehicleExit != null && unassignedArea != null;

        private void Add(WarehousePresentationItem item)
        {
            if (item.Kind == "Npc") { var view = Instantiate(npcTemplate, objectRoot); view.name = "Npc_" + Safe(item.StableId); view.BindSelection(Select); npcs.Add(item.StableId, view); }
            else { var view = Instantiate(objectTemplate, objectRoot); view.name = item.Kind + "_" + Safe(item.StableId); view.BindSelection(Select); objects.Add(item.StableId, view); }
        }
        private void Remove(string id)
        {
            if (objects.Remove(id, out var item)) Destroy(item.gameObject);
            if (npcs.Remove(id, out var npc)) Destroy(npc.gameObject);
        }
        private void RenderAll(IReadOnlyList<WarehousePresentationItem> items)
        {
            var visualItems = items.Where(item => item.Kind != "Npc").ToArray();
            var socketCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var index = 0; index < visualItems.Length; index++)
            {
                if (!objects.TryGetValue(visualItems[index].StableId, out var view)) throw new InvalidOperationException("WarehouseObjectMissing:" + visualItems[index].StableId);
                var anchor = Waypoint(visualItems[index].SocketKey);
                socketCounts.TryGetValue(visualItems[index].SocketKey, out var socketIndex);
                socketCounts[visualItems[index].SocketKey] = socketIndex + 1;
                view.Render(visualItems[index], objectRoot.InverseTransformPoint(anchor.position) + Offset(socketIndex));
            }
            foreach (var item in items.Where(item => item.Kind == "Npc"))
            {
                if (!npcs.TryGetValue(item.StableId, out var view)) throw new InvalidOperationException("WarehouseNpcMissing:" + item.StableId);
                view.Render(item, Waypoint(item.CurrentSocketKey), Waypoint(item.SocketKey));
            }
        }
        private Transform Waypoint(string key) => key switch
        {
            "warehouse.inbound-dock" => inboundDock, "warehouse.storage-zone" => storageZone,
            "warehouse.rack-zone" => rackZone, "warehouse.outbound-staging" => outboundStaging,
            "warehouse.approach" => approach, "warehouse.staff-entry" => staffEntry,
            "warehouse.inspection-zone" => inspectionZone, "warehouse.vehicle-exit" => vehicleExit,
            "warehouse.unassigned-area" => unassignedArea,
            _ => unassignedArea,
        };
        private void Select(string stableId)
        {
            if (currentSnapshot == null) return;
            selectedStableId = stableId;
            var selected = currentSnapshot.Items.FirstOrDefault(item => item.StableId == stableId);
            if (selected == null) throw new InvalidOperationException("WarehousePresentationSelectionUnknown:" + stableId);
            var related = new HashSet<string>(selected.RelatedStableIds, StringComparer.Ordinal);
            foreach (var pair in objects) pair.Value.SetSelectionState(pair.Key == stableId, related.Contains(pair.Key));
            foreach (var pair in npcs) pair.Value.SetSelectionState(pair.Key == stableId, related.Contains(pair.Key));
            detailLabel.text = selected.DetailText;
            detailPanelRoot.SetActive(true);
        }
        private void RestoreSelection()
        {
            if (string.IsNullOrEmpty(selectedStableId)) { detailPanelRoot.SetActive(false); return; }
            if (currentSnapshot!.Items.All(item => item.StableId != selectedStableId)) { selectedStableId = string.Empty; detailPanelRoot.SetActive(false); ClearSelection(); return; }
            Select(selectedStableId);
        }
        private void ClearSelection()
        {
            foreach (var view in objects.Values) view.SetSelectionState(false, false);
            foreach (var view in npcs.Values) view.SetSelectionState(false, false);
        }
        private static Vector3 Offset(int index) => new((index % 3) * 1.2f, .6f, (index / 3) * 1.1f);
        private static string Safe(string value) => value.Replace(':', '_');
    }
}
