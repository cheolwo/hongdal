using Ssalddel.Unity.Warehouse;
using UnityEngine;
using UnityEngine.AI;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseNpcView : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent = null!;
        [SerializeField] private Animator animator = null!;
        [SerializeField] private TextMesh label = null!;
        public string StableId { get; private set; } = string.Empty;
        public void Configure(NavMeshAgent navAgent, Animator npcAnimator, TextMesh text) { agent = navAgent; animator = npcAnimator; label = text; }
        public void Render(WarehouseWorldObject npc, Transform current, Transform destination)
        {
            if (string.IsNullOrEmpty(StableId))
            {
                if (agent.isOnNavMesh) agent.Warp(current.position); else transform.position = current.position;
            }
            StableId = npc.StableId; label.text = npc.Title + "\n" + npc.Status;
            if (agent.isOnNavMesh) agent.SetDestination(destination.position);
            if (animator != null) animator.SetBool("IsMoving", agent.isOnNavMesh && agent.remainingDistance > agent.stoppingDistance);
            gameObject.SetActive(true);
        }
        public bool ValidateWiring() => agent != null && label != null;
    }
}
