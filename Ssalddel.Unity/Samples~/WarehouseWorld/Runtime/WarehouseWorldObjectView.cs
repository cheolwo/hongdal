using System;
using Ssalddel.Unity.Warehouse;
using UnityEngine;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldObjectView : MonoBehaviour
    {
        [SerializeField] private Renderer visual = null!;
        [SerializeField] private TextMesh label = null!;
        private Action<string>? selected;
        private Color baseColor;
        public string StableId { get; private set; } = string.Empty;
        public void Configure(Renderer renderer, TextMesh text) { visual = renderer; label = text; }
        public void BindSelection(Action<string> selection) => selected = selection;
        public void Render(WarehousePresentationItem item, Vector3 position)
        {
            StableId = item.StableId; transform.localPosition = position;
            label.text = item.LabelText;
            baseColor = item.VisualStateCode switch
            {
                WarehouseVisualStateCodes.Inventory => new Color(0.72f, 0.48f, 0.22f),
                WarehouseVisualStateCodes.Cargo => new Color(0.92f, 0.62f, 0.18f),
                WarehouseVisualStateCodes.Vehicle => new Color(0.24f, 0.3f, 0.38f),
                WarehouseVisualStateCodes.Handoff => new Color(0.68f, 0.32f, 0.72f),
                WarehouseVisualStateCodes.InboundTask => new Color(0.25f, 0.72f, 0.7f),
                _ => new Color(0.25f, 0.55f, 0.72f),
            };
            visual.material.color = baseColor;
            gameObject.SetActive(true);
        }
        public void SetSelectionState(bool isSelected, bool isRelated)
            => visual.material.color = isSelected ? new Color(1f, .82f, .18f) : isRelated ? new Color(.25f, .9f, .9f) : baseColor;
        private void OnMouseDown() { if (!string.IsNullOrEmpty(StableId)) selected?.Invoke(StableId); }
        public bool ValidateWiring() => visual != null && label != null;
    }
}
