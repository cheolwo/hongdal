using System;
using Ssalddel.Unity.Warehouse;
using UnityEngine;
using UnityEngine.AI;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseNpcView : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent = null!;
        [SerializeField] private Animator animator = null!;
        [SerializeField] private Renderer visual = null!;
        [SerializeField] private TextMesh label = null!;
        private Action<string>? selected;
        private readonly Color baseColor = new(.35f, .65f, .35f);
        public string StableId { get; private set; } = string.Empty;
        public void Configure(NavMeshAgent navAgent, Animator npcAnimator, Renderer renderer, TextMesh text) { agent = navAgent; animator = npcAnimator; visual = renderer; label = text; }
        public void BindSelection(Action<string> selection) => selected = selection;
        public void Render(WarehousePresentationItem npc, Transform current, Transform destination)
        {
            if (string.IsNullOrEmpty(StableId))
            {
                if (agent.isOnNavMesh) agent.Warp(current.position); else transform.position = current.position;
            }
            StableId = npc.StableId; label.text = npc.LabelText;
            visual.material.color = baseColor;
            if (agent.isOnNavMesh) agent.SetDestination(destination.position);
            if (animator != null) animator.SetBool("IsMoving", agent.isOnNavMesh && agent.remainingDistance > agent.stoppingDistance);
            gameObject.SetActive(true);
        }
        public void SetSelectionState(bool isSelected, bool isRelated)
            => visual.material.color = isSelected ? new Color(1f, .82f, .18f) : isRelated ? new Color(.25f, .9f, .9f) : baseColor;
        private void OnMouseDown() { if (!string.IsNullOrEmpty(StableId)) selected?.Invoke(StableId); }
        public bool ValidateWiring() => agent != null && visual != null && label != null;
    }
}
