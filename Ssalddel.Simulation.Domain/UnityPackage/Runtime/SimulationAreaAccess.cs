using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, AreaAccessEntryState> areaAccessEntries = new(StringComparer.Ordinal);
        private bool areaAccessConfigured;
        private string currentAreaSetStableId = string.Empty;

        private void InitializeAreaAccess(bool configured)
        {
            areaAccessConfigured = configured;
            if (!configured) return;
            currentAreaSetStableId = SimulationAreaAccessCodes.FarmAreaSet;
            areaAccessEntries.Add(SimulationAreaAccessCodes.FarmAreaSet,
                new AreaAccessEntryState
                {
                    PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                    AreaSetStableId = SimulationAreaAccessCodes.FarmAreaSet,
                    AccessStateCode = SimulationAreaAccessCodes.Entered,
                    GrantedAtWorldRevision = 0,
                    Revision = 1,
                    GrantedByEvidenceIds = new[] { "evidence:scenario-start:farm" },
                });
            areaAccessEntries.Add(SimulationAreaAccessCodes.HubAreaSet,
                new AreaAccessEntryState
                {
                    PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                    AreaSetStableId = SimulationAreaAccessCodes.HubAreaSet,
                    AccessStateCode = SimulationAreaAccessCodes.Locked,
                    Revision = 1,
                });
        }

        public SimulationPlayerAreaAccessStateSnapshot GetPlayerAreaAccess(string playerStableId)
        {
            RequireStableId(playerStableId, "SimulationAreaAccessPlayerIdInvalid");
            lock (gate)
            {
                EnsureAreaAccessConfigured();
                if (playerStableId.Trim() != SimulationAreaAccessCodes.PlayerOwner)
                    throw new SimulationNotFoundException("SimulationAreaAccessPlayerNotFound");
                return CreateAreaAccessStateSnapshot();
            }
        }

        public SimulationAreaTraversalPreviewSnapshot PreviewAreaTraversal(SimulationAreaTraversalPreviewRequest request)
        {
            ValidateAreaTraversalPreview(request);
            lock (gate)
            {
                EnsureAreaAccessConfigured();
                return CreateAreaTraversalPreview(request, true);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmAreaTraversal(SimulationAreaTraversalConfirmRequest request)
        {
            ValidateAreaTraversalConfirm(request);
            lock (gate)
            {
                EnsureAreaAccessConfigured();
                var input = new SimulationAreaTraversalPreviewRequest
                {
                    ExpectedRevision = request.ExpectedRevision,
                    PlayerStableId = request.PlayerStableId,
                    TargetAreaSetStableId = request.TargetAreaSetStableId,
                    ConnectorStableId = request.ConnectorStableId,
                };
                var preview = CreateAreaTraversalPreview(input, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId.Trim(),
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateAreaTraversalDecision(input, preview),
                });
            }
        }

        private SimulationAreaTraversalPreviewSnapshot CreateAreaTraversalPreview(
            SimulationAreaTraversalPreviewRequest request, bool includeRevisionBlock)
        {
            var entry = areaAccessEntries[SimulationAreaAccessCodes.HubAreaSet];
            var blocks = new List<string>();
            if (request.PlayerStableId != SimulationAreaAccessCodes.PlayerOwner)
                blocks.Add("SimulationAreaAccessPlayerNotFound");
            if (request.TargetAreaSetStableId != SimulationAreaAccessCodes.HubAreaSet
                || request.ConnectorStableId != SimulationAreaAccessCodes.FarmToHubConnector)
                blocks.Add("SimulationAreaAccessConnectorUnavailable");
            if (entry.AccessStateCode == SimulationAreaAccessCodes.Locked)
                blocks.Add("SimulationAreaAccessEvidenceMissing");
            if (entry.AccessStateCode == SimulationAreaAccessCodes.Entered)
                blocks.Add("SimulationAreaAlreadyEntered");
            if (includeRevisionBlock && request.ExpectedRevision != Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            return new SimulationAreaTraversalPreviewSnapshot
            {
                BaseRevision = Revision,
                PlayerStableId = request.PlayerStableId,
                FromAreaSetStableId = currentAreaSetStableId,
                TargetAreaSetStableId = request.TargetAreaSetStableId,
                ConnectorStableId = request.ConnectorStableId,
                AccessStateCode = entry.AccessStateCode,
                EvidenceIds = entry.GrantedByEvidenceIds.ToArray(),
                NewWorldInteractionIds = new[] { SimulationAreaAccessCodes.HubManufacturingWorldInteraction },
                DurationTicks = 1,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
                PreviewHashSha256 = Sha256(string.Join("\u001e", new[]
                {
                    Revision.ToString(CultureInfo.InvariantCulture), request.PlayerStableId,
                    currentAreaSetStableId, request.TargetAreaSetStableId,
                    request.ConnectorStableId, entry.AccessHashSha256, string.Join("|", blocks),
                })),
            };
        }

        private SimulationDecisionPreviewRequest CreateAreaTraversalDecision(
            SimulationAreaTraversalPreviewRequest request, SimulationAreaTraversalPreviewSnapshot preview)
            => new()
            {
                DecisionStableId = "decision:area-traversal:" + request.PlayerStableId + ":farm-to-hub",
                DecisionTypeCode = SimulationAreaAccessCodes.PlayerAreaTraversal,
                ActorStableId = request.PlayerStableId,
                TargetStableIds = new[] { request.TargetAreaSetStableId },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = SimulationAreaAccessCodes.PlayerAreaTraversal,
                        TargetLedgerStableId = request.TargetAreaSetStableId,
                        Delta = 1m, AfterValue = 1m, UnitCode = "area-entry",
                        SourceStableIds = preview.EvidenceIds,
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = preview.BlockingReasonCodes,
                SourceStableIds = preview.EvidenceIds,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:area-traversal:" + request.PlayerStableId + ":farm-to-hub",
                    TaskTypeCode = SimulationAreaAccessCodes.PlayerAreaTraversalTask,
                    FacilityStableId = request.ConnectorStableId,
                    ActionCode = SimulationAreaAccessCodes.PlayerAreaTraversal,
                    AssignedActorStableId = request.PlayerStableId,
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "traveler",
                    DurationTicks = preview.DurationTicks,
                    InputLotStableIds = new[] { request.ConnectorStableId },
                    OutputCandidateCodes = preview.NewWorldInteractionIds,
                    SourceStableIds = preview.EvidenceIds,
                },
            };

        private void RefreshAllAreaAccessEvidence()
        {
            if (!areaAccessConfigured) return;
            var hub = areaAccessEntries[SimulationAreaAccessCodes.HubAreaSet];
            if (hub.AccessStateCode != SimulationAreaAccessCodes.Locked) return;
            var allocation = harvestLotAllocations.Values
                .Where(value => value.ChoiceCode == SimulationHarvestDispositionChoiceCodes.CooperativeShipment
                    && value.StateCode == SimulationHarvestLotAllocationStateCodes.Applied)
                .OrderBy(value => value.AllocationStableId, StringComparer.Ordinal).FirstOrDefault();
            if (allocation == null) return;
            hub.AccessStateCode = SimulationAreaAccessCodes.Granted;
            hub.GrantedAtWorldRevision = Revision;
            hub.GrantedByEvidenceIds = new[]
            {
                SimulationAreaAccessCodes.FarmHubShipmentEvidence,
                allocation.AllocationStableId,
            };
            hub.Revision++;
        }

        private void ObserveAreaAccessTaskCompletion(SimulationTaskSnapshot task, int completedTick)
        {
            if (!areaAccessConfigured || task.ActionCode != SimulationAreaAccessCodes.PlayerAreaTraversal)
                return;
            var hub = areaAccessEntries[SimulationAreaAccessCodes.HubAreaSet];
            if (hub.AccessStateCode == SimulationAreaAccessCodes.Entered) return;
            hub.AccessStateCode = SimulationAreaAccessCodes.Entered;
            hub.GrantedAtWorldRevision = Revision;
            hub.Revision++;
            currentAreaSetStableId = SimulationAreaAccessCodes.HubAreaSet;
        }

        private SimulationPlayerAreaAccessStateSnapshot CreateAreaAccessStateSnapshot()
            => new()
            {
                RuleRevision = areaAccessConfigured ? SimulationAreaAccessCodes.RuleRevision : string.Empty,
                WorldRevision = Revision,
                WorldTick = CurrentTick,
                CurrentAreaSetStableId = currentAreaSetStableId,
                AccessEntries = areaAccessEntries.Values.OrderBy(value => value.AreaSetStableId, StringComparer.Ordinal)
                    .Select(CreateAreaAccessEntrySnapshot).ToArray(),
                MutatesStaticHDefinitions = false,
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private SimulationPlayerAreaAccessSnapshot CreateAreaAccessEntrySnapshot(AreaAccessEntryState entry)
        {
            var availableWi = entry.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet
                && entry.AccessStateCode == SimulationAreaAccessCodes.Entered
                ? new[] { SimulationAreaAccessCodes.HubManufacturingWorldInteraction }
                : Array.Empty<string>();
            var sourceHash = entry.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet
                ? SimulationAreaAccessCodes.FarmToHubSourceHHashSha256
                : "5f71ebf9128ec719d07a06748b6b6eb71c1d69093fa76089dd60d37ad7a262aa";
            var hash = Sha256(string.Join("\u001e", new[]
            {
                SimulationAreaAccessCodes.RuleRevision, entry.PlayerStableId, entry.AreaSetStableId,
                entry.AccessStateCode, string.Join("|", entry.GrantedByEvidenceIds),
                entry.GrantedAtWorldRevision.ToString(CultureInfo.InvariantCulture), sourceHash,
                string.Join("|", availableWi), entry.Revision.ToString(CultureInfo.InvariantCulture),
            }));
            entry.AccessHashSha256 = hash;
            return new SimulationPlayerAreaAccessSnapshot
            {
                PlayerStableId = entry.PlayerStableId,
                AreaSetStableId = entry.AreaSetStableId,
                AccessLevelCode = SimulationAreaAccessCodes.Permanent,
                AccessStateCode = entry.AccessStateCode,
                GrantedByEvidenceIds = entry.GrantedByEvidenceIds.ToArray(),
                GrantedAtWorldRevision = entry.GrantedAtWorldRevision,
                RevocationPolicyCode = "PermanentUnlessExplicitEffect",
                SourceHDefinitionHashSha256 = sourceHash,
                AvailableWorldInteractionIds = availableWi,
                Revision = entry.Revision,
                AccessHashSha256 = hash,
            };
        }

        private void EnsureAreaAccessConfigured()
        {
            if (!areaAccessConfigured)
                throw new SimulationNotFoundException("SimulationAreaAccessNotConfigured");
        }

        private static void ValidateAreaTraversalPreview(SimulationAreaTraversalPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.PlayerStableId, "SimulationAreaAccessPlayerIdInvalid");
            RequireStableId(request.TargetAreaSetStableId, "SimulationAreaAccessTargetInvalid");
            RequireStableId(request.ConnectorStableId, "SimulationAreaAccessConnectorInvalid");
        }

        private static void ValidateAreaTraversalConfirm(SimulationAreaTraversalConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            ValidateAreaTraversalPreview(new SimulationAreaTraversalPreviewRequest
            {
                ExpectedRevision = request.ExpectedRevision,
                PlayerStableId = request.PlayerStableId,
                TargetAreaSetStableId = request.TargetAreaSetStableId,
                ConnectorStableId = request.ConnectorStableId,
            });
        }

        internal static SimulationPlayerAreaAccessStateSnapshot CloneAreaAccessState(
            SimulationPlayerAreaAccessStateSnapshot? source)
        {
            source ??= new SimulationPlayerAreaAccessStateSnapshot();
            return new SimulationPlayerAreaAccessStateSnapshot
            {
                RuleRevision = source.RuleRevision,
                WorldRevision = source.WorldRevision,
                WorldTick = source.WorldTick,
                CurrentAreaSetStableId = source.CurrentAreaSetStableId,
                AccessEntries = (source.AccessEntries ?? Array.Empty<SimulationPlayerAreaAccessSnapshot>())
                    .Select(value => new SimulationPlayerAreaAccessSnapshot
                    {
                        PlayerStableId = value.PlayerStableId,
                        AreaSetStableId = value.AreaSetStableId,
                        AccessLevelCode = value.AccessLevelCode,
                        AccessStateCode = value.AccessStateCode,
                        GrantedByEvidenceIds = value.GrantedByEvidenceIds.ToArray(),
                        GrantedAtWorldRevision = value.GrantedAtWorldRevision,
                        RevocationPolicyCode = value.RevocationPolicyCode,
                        SourceHDefinitionHashSha256 = value.SourceHDefinitionHashSha256,
                        AvailableWorldInteractionIds = value.AvailableWorldInteractionIds.ToArray(),
                        Revision = value.Revision,
                        AccessHashSha256 = value.AccessHashSha256,
                    }).ToArray(),
                MutatesStaticHDefinitions = source.MutatesStaticHDefinitions,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
        }

        private sealed class AreaAccessEntryState
        {
            public string PlayerStableId { get; set; } = string.Empty;
            public string AreaSetStableId { get; set; } = string.Empty;
            public string AccessStateCode { get; set; } = string.Empty;
            public string[] GrantedByEvidenceIds { get; set; } = Array.Empty<string>();
            public long GrantedAtWorldRevision { get; set; }
            public long Revision { get; set; }
            public string AccessHashSha256 { get; set; } = string.Empty;
        }
    }
}
