using System;
using System.Collections.Generic;
using Ssalddel.Unity.Community;
using UnityEngine;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare
{
    public sealed class CommunityMarketSquareView : MonoBehaviour
    {
        [SerializeField] private Transform itemRoot = null!;
        [SerializeField] private CommunitySquareItemView itemTemplate = null!;
        [SerializeField] private TextMesh statusLabel = null!;
        [SerializeField] private int columns = 4;
        [SerializeField] private Vector2 spacing = new Vector2(3.5f, 3f);

        private readonly Dictionary<string, CommunitySquareItemView> items =
            new Dictionary<string, CommunitySquareItemView>(StringComparer.Ordinal);

        public int ItemCount => items.Count;

        public void Configure(Transform root, CommunitySquareItemView template, TextMesh status, int columnCount, Vector2 itemSpacing)
        {
            itemRoot = root; itemTemplate = template; statusLabel = status;
            columns = Math.Max(1, columnCount); spacing = itemSpacing;
        }

        public void ShowState(string stateCode, string message = "")
            => statusLabel.text = stateCode + (string.IsNullOrWhiteSpace(message) ? string.Empty : "\n" + message);

        public void Render(CommunityMarketSquareLoadResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.Changes != null)
            {
                foreach (var removed in result.Changes.Removed)
                {
                    if (items.TryGetValue(removed.StableId, out var view))
                    {
                        items.Remove(removed.StableId);
                        Destroy(view.gameObject);
                    }
                }
                foreach (var updated in result.Changes.Updated) RenderExisting(updated);
                foreach (var added in result.Changes.Added)
                {
                    var view = Instantiate(itemTemplate, itemRoot);
                    view.name = "SquareItem_" + added.StableId.Replace(':', '_');
                    items.Add(added.StableId, view);
                }
                Reflow(result.Snapshot?.Items ?? Array.Empty<CommunitySquareWorldItem>());
            }

            var count = result.Snapshot?.Items.Length ?? 0;
            var message = result.Error == null ? count + " public items" : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name;
            ShowState(result.StateCode, message);
        }

        public bool ValidateWiring()
            => itemRoot != null && itemTemplate != null && itemTemplate.ValidateWiring()
                && statusLabel != null && columns > 0 && spacing.x > 0f && spacing.y > 0f;

        private void Reflow(IReadOnlyList<CommunitySquareWorldItem> ordered)
        {
            for (var index = 0; index < ordered.Count; index++)
            {
                var item = ordered[index];
                if (!items.TryGetValue(item.StableId, out var view))
                    throw new InvalidOperationException("CommunitySquareItemMissing:" + item.StableId);
                var row = index / columns; var column = index % columns;
                view.Render(item, new Vector3(column * spacing.x, 0.6f, -row * spacing.y));
            }
        }

        private void RenderExisting(CommunitySquareWorldItem item)
        {
            if (!items.ContainsKey(item.StableId))
                throw new InvalidOperationException("CommunitySquareItemMissing:" + item.StableId);
        }
    }
}
