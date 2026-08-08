using Ssalddel.Unity.Community;
using UnityEngine;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare
{
    public sealed class CommunitySquareItemView : MonoBehaviour
    {
        [SerializeField] private Renderer visual = null!;
        [SerializeField] private TextMesh titleLabel = null!;
        [SerializeField] private TextMesh detailLabel = null!;

        public string StableId { get; private set; } = string.Empty;

        public void Configure(Renderer itemVisual, TextMesh title, TextMesh detail)
        {
            visual = itemVisual;
            titleLabel = title;
            detailLabel = detail;
        }

        public void Render(CommunitySquareWorldItem item, Vector3 localPosition)
        {
            StableId = item.StableId;
            transform.localPosition = localPosition;
            titleLabel.text = item.Title;
            detailLabel.text = item.Status + (item.Count > 0 ? " · " + item.Count : string.Empty);
            visual.material.color = ColorFor(item.Kind);
            gameObject.SetActive(true);
        }

        public bool ValidateWiring() => visual != null && titleLabel != null && detailLabel != null;

        private static Color ColorFor(string kind)
        {
            switch (kind)
            {
                case "Board": return new Color(0.20f, 0.43f, 0.32f);
                case "Post": return new Color(0.91f, 0.55f, 0.24f);
                case "Activity": return new Color(0.28f, 0.55f, 0.75f);
                case "Ledger": return new Color(0.52f, 0.38f, 0.66f);
                default: return Color.gray;
            }
        }
    }
}
