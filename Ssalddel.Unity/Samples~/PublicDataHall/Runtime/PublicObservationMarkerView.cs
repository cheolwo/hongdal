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

        public void Render(PublicMapMarkerPresentationItem observation, Vector3 worldPosition)
        {
            StableId = observation.StableId.Value;
            transform.localPosition = worldPosition;
            titleLabel.text = observation.LabelText;
            markerRenderer.enabled = true;
            gameObject.SetActive(true);
        }

        [System.Obsolete("Use Render(PublicMapMarkerPresentationItem, Vector3).")]
        public void Render(PublicObservationPresentationModel observation, Vector3 worldPosition)
        {
            StableId = observation.StableId;
            transform.localPosition = worldPosition;
            titleLabel.text = observation.MarkerLabelText;
            markerRenderer.enabled = true;
            gameObject.SetActive(true);
        }

        public bool ValidateWiring()
        {
            return markerRenderer != null && titleLabel != null;
        }
    }
}
