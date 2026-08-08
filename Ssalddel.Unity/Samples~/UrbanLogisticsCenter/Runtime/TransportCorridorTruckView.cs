using System;
using Ssalddel.Unity.Samples.NpcMovement;
using Ssalddel.Unity.Transport;
using UnityEngine;
using UnityEngine.AI;

namespace Ssalddel.Unity.Samples.UrbanLogisticsCenter
{
    public sealed class TransportCorridorTruckView : MonoBehaviour, ITruckMovementTarget
    {
        [SerializeField]
        private string truckStableId = string.Empty;

        [SerializeField]
        private NavMeshAgent agent = null!;

        [SerializeField]
        private Animator animator = null!;

        [SerializeField]
        private ZoneNpcWaypointRegistry waypointRegistry = null!;

        [SerializeField]
        private Transform cargoVisualRoot = null!;

        [SerializeField]
        private TextMesh statusLabel = null!;

        public string TruckStableId => truckStableId;

        public void Configure(
            string stableId,
            NavMeshAgent navAgent,
            Animator truckAnimator,
            ZoneNpcWaypointRegistry registry,
            Transform cargoRoot,
            TextMesh label)
        {
            truckStableId = stableId?.Trim() ?? string.Empty;
            agent = navAgent;
            animator = truckAnimator;
            waypointRegistry = registry;
            cargoVisualRoot = cargoRoot;
            statusLabel = label;
        }

        public void ApplyTruckMovement(TruckMovementSnapshot movement)
        {
            if (!string.Equals(movement.StableId, truckStableId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("TruckStableIdMismatch");
            }

            if (!waypointRegistry.TryResolve(movement.CurrentNodeKey, out var current)
                || !waypointRegistry.TryResolve(movement.DestinationNodeKey, out var destination))
            {
                throw new InvalidOperationException("TransportCorridorWaypointMissing");
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                if (agent.isOnNavMesh)
                {
                    agent.Warp(current.position);
                }
                else
                {
                    transform.position = current.position;
                }
            }

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(destination.position);
            }

            if (animator != null)
            {
                animator.SetBool("IsMoving", agent.isOnNavMesh);
            }

            cargoVisualRoot.gameObject.SetActive(true);
            statusLabel.text = movement.CargoStableId + "\n" + movement.CurrentNodeKey + " → " + movement.DestinationNodeKey;
        }

        public void Hide()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            gameObject.SetActive(false);
        }

        public bool ValidateWiring()
        {
            return !string.IsNullOrWhiteSpace(truckStableId)
                && agent != null
                && waypointRegistry != null
                && waypointRegistry.ValidateWiring()
                && cargoVisualRoot != null
                && statusLabel != null;
        }
    }
}
