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

        public void Render(CommunitySquarePresentationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Changes != null)
            {
                foreach (var stableId in model.Changes.RemovedStableIds)
                {
                    if (items.TryGetValue(stableId, out var view))
                    {
                        items.Remove(stableId);
                        Destroy(view.gameObject);
                    }
                }
                foreach (var updated in model.Changes.Updated) RenderExisting(updated);
                foreach (var added in model.Changes.Added)
                {
                    var view = Instantiate(itemTemplate, itemRoot);
                    view.name = "SquareItem_" + added.StableId.Replace(':', '_');
                    items.Add(added.StableId, view);
                }
                Reflow(model.Items);
            }

            ShowState(model.StateCode, model.StatusMessage);
        }

        public bool ValidateWiring()
            => itemRoot != null && itemTemplate != null && itemTemplate.ValidateWiring()
                && statusLabel != null && columns > 0 && spacing.x > 0f && spacing.y > 0f;

        private void Reflow(IReadOnlyList<CommunitySquareItemPresentationModel> ordered)
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

        private void RenderExisting(CommunitySquareItemPresentationModel item)
        {
            if (!items.ContainsKey(item.StableId))
                throw new InvalidOperationException("CommunitySquareItemMissing:" + item.StableId);
        }
    }
}
