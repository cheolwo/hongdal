using Ssalddel.Unity.Warehouse;
using UnityEngine;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldObjectView : MonoBehaviour
    {
        [SerializeField] private Renderer visual = null!;
        [SerializeField] private TextMesh label = null!;
        public string StableId { get; private set; } = string.Empty;
        public void Configure(Renderer renderer, TextMesh text) { visual = renderer; label = text; }
        public void Render(WarehouseWorldObject item, Vector3 position)
        {
            StableId = item.StableId; transform.localPosition = position;
            label.text = item.Title + "\n" + item.Status + (item.Quantity > 0 ? " · " + item.Quantity : string.Empty);
            visual.material.color = item.Kind == "Inventory" ? new Color(0.72f, 0.48f, 0.22f) : new Color(0.25f, 0.55f, 0.72f);
            gameObject.SetActive(true);
        }
        public bool ValidateWiring() => visual != null && label != null;
    }
}
