using System;
using UnityEngine;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class InteractionSocket : MonoBehaviour
    {
        [SerializeField]
        private Collider targetCollider = null!;

        public event Action? Selected;

        public void Configure(Collider value)
        {
            targetCollider = value;
        }

        public bool ValidateWiring()
        {
            return targetCollider != null;
        }

        private void OnMouseDown()
        {
            Selected?.Invoke();
        }
    }
}
