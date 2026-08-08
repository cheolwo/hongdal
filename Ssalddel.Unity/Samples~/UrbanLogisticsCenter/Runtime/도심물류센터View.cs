using System;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Perspectives;
using Ssalddel.Unity.Samples.NpcMovement;
using Ssalddel.Unity.Transport;
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

        [SerializeField]
        private TransportCorridorTruckView corridorTruck = null!;

        public void Configure(
            LogisticsRoleTargetView[] targets,
            LogisticsInteractionPanelView panel,
            ZoneNpcMovementController movementController,
            TransportCorridorTruckView truck)
        {
            roleTargets = targets ?? Array.Empty<LogisticsRoleTargetView>();
            interactionPanel = panel;
            npcMovementController = movementController;
            corridorTruck = truck;
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

        public void ApplyTransportCorridor(TransportCorridorSnapshot? snapshot, TruckMovementApplicator applicator)
        {
            if (snapshot == null)
            {
                corridorTruck.Hide();
                return;
            }

            applicator.Apply(snapshot, corridorTruck);
        }

        public bool ValidateWiring()
        {
            if (roleTargets == null
                || roleTargets.Length != 3
                || interactionPanel == null
                || npcMovementController == null
                || corridorTruck == null
                || !interactionPanel.ValidateWiring()
                || !npcMovementController.ValidateWiring()
                || !corridorTruck.ValidateWiring())
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
