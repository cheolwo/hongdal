using System;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Perspectives;
using Ssalddel.Unity.Samples.NpcMovement;
using UnityEngine;

namespace Ssalddel.Unity.Samples.UrbanLogisticsCenter
{
    public sealed class 도심물류센터View : MonoBehaviour
    {
        [SerializeField]
        private LogisticsRoleTargetView[] roleTargets = Array.Empty<LogisticsRoleTargetView>();

        [SerializeField]
        private LogisticsInteractionPanelView interactionPanel = null!;

        [SerializeField]
        private ZoneNpcMovementController npcMovementController = null!;

        public void Configure(
            LogisticsRoleTargetView[] targets,
            LogisticsInteractionPanelView panel,
            ZoneNpcMovementController movementController)
        {
            roleTargets = targets ?? Array.Empty<LogisticsRoleTargetView>();
            interactionPanel = panel;
            npcMovementController = movementController;
        }

        public IRolePerspectiveTarget[] GetRoleTargets()
        {
            var values = new IRolePerspectiveTarget[roleTargets.Length];
            for (var index = 0; index < roleTargets.Length; index++)
            {
                values[index] = roleTargets[index];
            }

            return values;
        }

        public IRoleInteractionSink GetInteractionSink()
        {
            return interactionPanel;
        }

        public string[] ApplyNpcMovement(NpcMovementSnapshot snapshot)
        {
            return npcMovementController.ApplySnapshots(new[] { snapshot });
        }

        public bool ValidateWiring()
        {
            if (roleTargets == null
                || roleTargets.Length != 3
                || interactionPanel == null
                || npcMovementController == null
                || !interactionPanel.ValidateWiring()
                || !npcMovementController.ValidateWiring())
            {
                return false;
            }

            foreach (var target in roleTargets)
            {
                if (target == null || !target.ValidateWiring())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
