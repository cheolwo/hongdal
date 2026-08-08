using System;
using System.Collections.Generic;
using Ssalddel.Unity.PublicData;
using UnityEngine;

namespace Ssalddel.Unity.Samples.PublicDataHall
{
    public sealed class PublicDataHallView : MonoBehaviour
    {
        [SerializeField]
        private Transform markerRoot = null!;

        [SerializeField]
        private PublicObservationMarkerView markerTemplate = null!;

        [SerializeField]
        private TextMesh statusLabel = null!;

        [SerializeField]
        private Vector2 worldSize = new Vector2(18f, 10f);

        private readonly Dictionary<string, PublicObservationMarkerView> markers =
            new Dictionary<string, PublicObservationMarkerView>(StringComparer.Ordinal);

        public int MarkerCount => markers.Count;

        public void Configure(
            Transform root,
            PublicObservationMarkerView template,
            TextMesh label,
            Vector2 size)
        {
            markerRoot = root;
            markerTemplate = template;
            statusLabel = label;
            worldSize = size;
        }

        public void ShowState(string stateCode, string message = "")
        {
            statusLabel.text = stateCode + (string.IsNullOrWhiteSpace(message) ? string.Empty : "\n" + message);
        }

        public void Render(PublicDataHallLoadResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.Changes != null)
            {
                foreach (var observation in result.Changes.Removed)
                {
                    if (markers.TryGetValue(observation.StableId, out var marker))
                    {
                        markers.Remove(observation.StableId);
                        Destroy(marker.gameObject);
                    }
                }

                foreach (var observation in result.Changes.Updated)
                {
                    RenderExisting(observation);
                }

                foreach (var observation in result.Changes.Added)
                {
                    var marker = Instantiate(markerTemplate, markerRoot);
                    marker.name = "Observation_" + observation.StableId.Replace(':', '_');
                    markers.Add(observation.StableId, marker);
                    marker.Render(observation, Project(observation.Latitude, observation.Longitude));
                }
            }

            var count = result.Snapshot?.Observations.Length ?? 0;
            var message = result.Error == null
                ? count + " observations"
                : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name;
            ShowState(result.StateCode, message);
        }

        public bool ValidateWiring()
        {
            return markerRoot != null
                && markerTemplate != null
                && markerTemplate.ValidateWiring()
                && statusLabel != null
                && worldSize.x > 0f
                && worldSize.y > 0f;
        }

        private void RenderExisting(PublicWorldMapObservation observation)
        {
            if (!markers.TryGetValue(observation.StableId, out var marker))
            {
                throw new InvalidOperationException("PublicObservationMarkerMissing:" + observation.StableId);
            }

            marker.Render(observation, Project(observation.Latitude, observation.Longitude));
        }

        private Vector3 Project(double latitude, double longitude)
        {
            var x = (float)((longitude + 180d) / 360d - 0.5d) * worldSize.x;
            var z = (float)((latitude + 90d) / 180d - 0.5d) * worldSize.y;
            return new Vector3(x, 0.35f, z);
        }
    }
}
