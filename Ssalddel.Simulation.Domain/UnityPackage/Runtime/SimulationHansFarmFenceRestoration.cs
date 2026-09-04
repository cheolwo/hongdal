using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationHansFarmFenceRestorationSnapshot?
            natureHansFarmFenceRestoration;

        private void InitializeHansFarmFenceRestoration(
            SimulationNatureSurvivalInitialStateRequest request)
        {
            natureHansFarmFenceRestoration = null;
            if (!SimulationNatureSurvivalCodes.IsR6(request.ProfileRevision)
                || !request.HansFarmFenceRestorationEnabled)
                return;

            natureHansFarmFenceRestoration =
                new SimulationHansFarmFenceRestorationSnapshot
                {
                    RequiredTimberQuantity =
                        NatureSurvivalRules.HansFarmFenceRepairTimberCost,
                    Segments = new[]
                    {
                        HansFenceSegment(1, -2.4d, 2.2d, 90d),
                        HansFenceSegment(2, 0d, 2.2d, 82d),
                        HansFenceSegment(3, 2.4d, 2.2d, 96d),
                    },
                };
        }

        private static SimulationHansFarmFenceSegmentSnapshot HansFenceSegment(
            int index, double localX, double localZ, double yawDegrees)
            => new SimulationHansFarmFenceSegmentSnapshot
            {
                SegmentStableId =
                    $"fence-segment:hans-farm:first-incident:{index:00}",
                StateCode = SimulationNatureSurvivalCodes.Damaged,
                LocalX = localX,
                LocalZ = localZ,
                YawDegrees = yawDegrees,
            };

        private void AppendHansFarmFencePreviewReasons(string action,
            string targetStableId, ICollection<string> reasons)
        {
            if (!IsNatureR6 || natureHansFarmFenceRestoration == null)
            {
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                return;
            }

            if (action == SimulationNatureSurvivalCodes.AcquireHansBrokenAxe)
            {
                if (!string.Equals(targetStableId,
                        SimulationNatureSurvivalCodes.HansBrokenAxePickupStableId,
                        StringComparison.Ordinal)
                    || natureHansFarmFenceRestoration.BrokenAxeStateCode !=
                        SimulationNatureSurvivalCodes.Available)
                    reasons.Add(
                        SimulationNatureSurvivalCodes.HansBrokenAxeUnavailable);
                return;
            }

            if (!string.Equals(targetStableId,
                    SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId,
                    StringComparison.Ordinal))
                reasons.Add(
                    SimulationNatureSurvivalCodes.HansFarmFenceTargetInvalid);
            if (!natureHansFarmFenceRestoration.PlayerCarriesBrokenAxe)
                reasons.Add(
                    SimulationNatureSurvivalCodes.HansBrokenAxeUnavailable);
            if (natureHansFarmFenceRestoration.FenceStateCode ==
                SimulationNatureSurvivalCodes.Repaired)
                reasons.Add(
                    SimulationNatureSurvivalCodes.HansFarmFenceAlreadyRepaired);
            if (NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.TimberItemCode) <
                NatureSurvivalRules.HansFarmFenceRepairTimberCost)
                reasons.Add(SimulationNatureSurvivalCodes.TimberInsufficient);
            if (natureActiveWork != null)
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
        }

        private void ApplyHansFarmFenceAction(string action)
        {
            if (natureHansFarmFenceRestoration == null)
                throw new SimulationConflictException(
                    SimulationNatureSurvivalCodes.ActionBlocked);

            if (action == SimulationNatureSurvivalCodes.AcquireHansBrokenAxe)
            {
                AddNaturePlayerItem(
                    natureSurvivalCreationState!.PlayerStableId,
                    SimulationNatureSurvivalCodes.HansBrokenAxeItemCode,
                    "한스 농장의 부러진 손도끼", 1);
                natureHansFarmFenceRestoration.BrokenAxeStateCode =
                    SimulationNatureSurvivalCodes.Carried;
                natureHansFarmFenceRestoration.PlayerCarriesBrokenAxe = true;
                CompleteLatestWorldInteractionManifestation(
                    SimulationNatureSurvivalCodes
                        .AcquireHansBrokenAxeWorldInteractionId,
                    new[] { "effect:nature:hans-broken-axe-acquired" },
                    new[] { "HansBrokenAxeCarried" }, Revision + 1L);
                return;
            }

            ConsumeNaturePlayerItem(
                SimulationNatureSurvivalCodes.TimberItemCode,
                NatureSurvivalRules.HansFarmFenceRepairTimberCost);
            natureHansFarmFenceRestoration.FenceStateCode =
                SimulationNatureSurvivalCodes.Repaired;
            foreach (var segment in natureHansFarmFenceRestoration.Segments)
            {
                segment.StateCode = SimulationNatureSurvivalCodes.Repaired;
                segment.YawDegrees = 90d;
            }
            natureHansFarmFenceRestoration.NextChoiceAvailable = true;
            CompleteLatestWorldInteractionManifestation(
                SimulationNatureSurvivalCodes
                    .RepairHansFarmFenceWorldInteractionId,
                new[] { "effect:nature:hans-fence-repaired" },
                new[]
                {
                    "HansFarmFenceRepaired",
                    SimulationNatureSurvivalCodes
                        .HansFarmLifeOrTravelChoiceAvailable,
                }, Revision + 1L);
        }

        private SimulationHansFarmFenceRestorationSnapshot?
            CreateHansFarmFenceRestorationSnapshot()
            => CloneHansFarmFenceRestoration(natureHansFarmFenceRestoration);

        internal static SimulationHansFarmFenceRestorationSnapshot?
            CloneHansFarmFenceRestoration(
                SimulationHansFarmFenceRestorationSnapshot? source)
            => source == null ? null
                : new SimulationHansFarmFenceRestorationSnapshot
                {
                    FenceAggregateStableId = source.FenceAggregateStableId,
                    H2StableId = source.H2StableId,
                    H1StableId = source.H1StableId,
                    FenceStateCode = source.FenceStateCode,
                    BrokenAxePickupStableId = source.BrokenAxePickupStableId,
                    BrokenAxeStateCode = source.BrokenAxeStateCode,
                    PlayerCarriesBrokenAxe = source.PlayerCarriesBrokenAxe,
                    RequiredTimberQuantity = source.RequiredTimberQuantity,
                    NextChoiceAvailable = source.NextChoiceAvailable,
                    Segments = source.Segments.Select(value =>
                        new SimulationHansFarmFenceSegmentSnapshot
                        {
                            SegmentStableId = value.SegmentStableId,
                            StateCode = value.StateCode,
                            LocalX = value.LocalX,
                            LocalZ = value.LocalZ,
                            YawDegrees = value.YawDegrees,
                        }).ToArray(),
                };
    }
}
