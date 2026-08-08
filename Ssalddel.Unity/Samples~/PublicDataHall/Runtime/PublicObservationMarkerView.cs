using Ssalddel.Unity.PublicData;
using UnityEngine;

namespace Ssalddel.Unity.Samples.PublicDataHall
{
    public sealed class PublicObservationMarkerView : MonoBehaviour
    {
        [SerializeField]
        private Renderer markerRenderer = null!;

        [SerializeField]
        private TextMesh titleLabel = null!;

        public string StableId { get; private set; } = string.Empty;

        public void Configure(Renderer renderer, TextMesh label)
        {
            markerRenderer = renderer;
            titleLabel = label;
        }

        public void Render(PublicWorldMapObservation observation, Vector3 worldPosition)
        {
            StableId = observation.StableId;
            transform.localPosition = worldPosition;
            titleLabel.text = observation.Title + "\n" + observation.SourceName;
            markerRenderer.enabled = true;
            gameObject.SetActive(true);
        }

        public bool ValidateWiring()
        {
            return markerRenderer != null && titleLabel != null;
        }
    }
}
