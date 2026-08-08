using System;
using UnityEngine;

namespace Ssalddel.Unity.Samples.UrbanMarket
{
    public sealed class InteractionSocket : MonoBehaviour
    {
        [SerializeField]
        private Collider interactionCollider = null!;

        public event Action? Selected;

        public void Configure(Collider targetCollider)
        {
            interactionCollider = targetCollider;
        }

        public bool ValidateWiring()
        {
            return interactionCollider != null;
        }

        private void OnMouseDown()
        {
            if (interactionCollider != null && interactionCollider.enabled)
            {
                Selected?.Invoke();
            }
        }
    }
}
