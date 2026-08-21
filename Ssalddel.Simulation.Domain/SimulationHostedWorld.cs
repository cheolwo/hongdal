using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private string hostedSessionStableId = string.Empty;
        private string hostedSessionModeCode = SimulationHostedWorldCodes.Solo;
        private long hostedCreatedAtWorldRevision;
        private long hostedPermissionRevision;
        private readonly Dictionary<string, SimulationHostedWorldParticipantSnapshot>
            hostedParticipants = new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationHostedWorldPermissionGrantSnapshot>
            hostedPermissionGrants = new(StringComparer.Ordinal);
        private readonly List<SimulationHostedWorldAuditSnapshot> hostedAuditTrail = new();

        private void InitializeHostedWorld()
        {
            hostedParticipants.Add(SimulationHostedWorldCodes.Solo,
                new SimulationHostedWorldParticipantSnapshot
                {
                    PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                    ParticipantStateCode = SimulationHostedWorldCodes.Active,
                    CurrentAreaSetStableId = SimulationAreaAccessCodes.FarmAreaSet,
                    JoinedAtWorldRevision = 0,
                });
        }

        public SimulationHostedWorldStateSnapshot GetHostedWorldState()
        {
            lock (gate) return CreateHostedWorldSnapshot();
        }

        public SimulationHostedWorldPreviewSnapshot PreviewOpenHostedWorld(
            SimulationHostedWorldOpenPreviewRequest request)
        {
            ValidateOpenRequest(request);
            lock (gate) return CreateOpenPreview(request, true);
        }

        public 경영SimulationSessionSnapshot ConfirmOpenHostedWorld(
            SimulationHostedWorldOpenConfirmRequest request)
        {
            ValidateOpenRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var preview = CreateOpenPreview(request, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = HostedDecision(preview,
                        new[] { request.OwnerPlayerStableId, request.InvitedGuestPlayerStableId },
                        "world:" + SessionStableId),
                });
            }
        }

        public SimulationHostedWorldPreviewSnapshot PreviewJoinHostedWorld(
            SimulationHostedWorldJoinPreviewRequest request)
        {
            ValidateJoinRequest(request);
            lock (gate) return CreateJoinPreview(request, true);
        }

        public 경영SimulationSessionSnapshot ConfirmJoinHostedWorld(
            SimulationHostedWorldJoinConfirmRequest request)
        {
            ValidateJoinRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var preview = CreateJoinPreview(request, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = HostedDecision(preview,
                        new[] { request.GuestPlayerStableId }, hostedSessionStableId),
                });
            }
        }

        public SimulationHostedWorldPreviewSnapshot PreviewHostedGuestAction(
            SimulationHostedGuestActionPreviewRequest request)
        {
            ValidateGuestActionRequest(request);
            lock (gate) return CreateGuestActionPreview(request, true);
        }

        public 경영SimulationSessionSnapshot ConfirmHostedGuestAction(
            SimulationHostedGuestActionConfirmRequest request)
        {
            ValidateGuestActionRequest(request);
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            lock (gate)
            {
                var preview = CreateGuestActionPreview(request, false);
                return ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = HostedDecision(preview, new[]
                    {
                        request.GuestPlayerStableId, request.ScopeStableId,
                        request.CapabilityCode, request.TargetStableId,
                    }, request.TargetStableId),
                });
            }
        }

        private SimulationHostedWorldPreviewSnapshot CreateOpenPreview(
            SimulationHostedWorldOpenPreviewRequest request, bool includeRevision)
        {
            var blocks = new List<string>();
            if (hostedSessionModeCode != SimulationHostedWorldCodes.Solo)
                blocks.Add("SimulationHostedWorldAlreadyOpen");
            if (request.OwnerPlayerStableId != SimulationAreaAccessCodes.PlayerOwner)
                blocks.Add("SimulationHostedWorldOwnerMismatch");
            if (request.InvitedGuestPlayerStableId == request.OwnerPlayerStableId)
                blocks.Add("SimulationHostedWorldGuestMustDiffer");
            if (includeRevision && request.ExpectedRevision != Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            return HostedPreview(SimulationHostedWorldCodes.OpenHostedWorld,
                request.OwnerPlayerStableId, request.InvitedGuestPlayerStableId,
                "world:" + SessionStableId, SimulationHostedWorldCodes.Interact,
                SimulationHostedWorldCodes.Allow, SimulationHostedWorldCodes.Confirm,
                blocks);
        }

        private SimulationHostedWorldPreviewSnapshot CreateJoinPreview(
            SimulationHostedWorldJoinPreviewRequest request, bool includeRevision)
        {
            var blocks = new List<string>();
            if (hostedSessionModeCode != SimulationHostedWorldCodes.HostedMultiplayer)
                blocks.Add("SimulationHostedWorldNotOpen");
            if (!hostedParticipants.Values.Any(value =>
                    value.PlayerStableId == request.GuestPlayerStableId
                    && value.ParticipantStateCode == SimulationHostedWorldCodes.Invited))
                blocks.Add("SimulationHostedWorldInviteRequired");
            if (includeRevision && request.ExpectedRevision != Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            return HostedPreview(SimulationHostedWorldCodes.JoinHostedWorld,
                request.GuestPlayerStableId, request.GuestPlayerStableId,
                hostedSessionStableId, SimulationHostedWorldCodes.Interact,
                SimulationHostedWorldCodes.Allow, SimulationHostedWorldCodes.Confirm,
                blocks);
        }

        private SimulationHostedWorldPreviewSnapshot CreateGuestActionPreview(
            SimulationHostedGuestActionPreviewRequest request, bool includeRevision)
        {
            var blocks = new List<string>();
            if (hostedSessionModeCode != SimulationHostedWorldCodes.HostedMultiplayer)
                blocks.Add("SimulationHostedWorldNotOpen");
            if (!hostedParticipants.Values.Any(value =>
                    value.PlayerStableId == request.GuestPlayerStableId
                    && value.ParticipantStateCode == SimulationHostedWorldCodes.Active))
                blocks.Add("SimulationHostedWorldParticipantNotActive");
            var grant = FindHostedGrant(request.GuestPlayerStableId,
                request.ScopeStableId, request.CapabilityCode);
            var grantState = grant?.GrantStateCode ?? SimulationHostedWorldCodes.Deny;
            var risk = grant?.ActionRiskPolicyCode ?? SimulationHostedWorldCodes.Deny;
            if (grantState != SimulationHostedWorldCodes.Allow)
                blocks.Add("SimulationHostedPermissionDenied");
            if (risk == SimulationHostedWorldCodes.HostApproval)
                blocks.Add("SimulationHostedHostApprovalRequired");
            if (includeRevision && request.ExpectedRevision != Revision)
                blocks.Add("SimulationExpectedRevisionMismatch");
            return HostedPreview(SimulationHostedWorldCodes.HostedGuestAction,
                request.GuestPlayerStableId, request.GuestPlayerStableId,
                request.ScopeStableId, request.CapabilityCode, grantState, risk, blocks);
        }

        private SimulationHostedWorldPreviewSnapshot HostedPreview(string action,
            string actor, string target, string scope, string capability,
            string grant, string risk, List<string> blocks)
            => new()
            {
                BaseRevision = Revision,
                ActionCode = action,
                ActorPlayerStableId = actor,
                TargetPlayerStableId = target,
                ScopeStableId = scope,
                CapabilityCode = capability,
                GrantStateCode = grant,
                ActionRiskPolicyCode = risk,
                DurationTicks = 1,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
                PreviewHashSha256 = Sha256(string.Join("\u001e", new[]
                {
                    Revision.ToString(CultureInfo.InvariantCulture), action, actor,
                    target, scope, capability, grant, risk, string.Join("|", blocks),
                })),
            };

        private SimulationDecisionPreviewRequest HostedDecision(
            SimulationHostedWorldPreviewSnapshot preview, string[] inputIds,
            string targetStableId) => new()
            {
                DecisionStableId = "decision:hosted:" + preview.ActionCode.ToLowerInvariant()
                    + ":" + preview.ActorPlayerStableId,
                DecisionTypeCode = preview.ActionCode,
                ActorStableId = preview.ActorPlayerStableId,
                TargetStableIds = new[] { targetStableId },
                ExpectedCosts = Array.Empty<SimulationValueProjection>(),
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = preview.ActionCode,
                        TargetLedgerStableId = targetStableId,
                        Delta = 1m, AfterValue = 1m, UnitCode = "hosted-state",
                        SourceStableIds = new[] { SimulationHostedWorldCodes.PolicySource },
                    },
                },
                Uncertainties = Array.Empty<string>(),
                BlockReasonCodes = preview.BlockingReasonCodes,
                SourceStableIds = new[] { SimulationHostedWorldCodes.PolicySource },
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:hosted:" + preview.ActionCode.ToLowerInvariant()
                        + ":" + preview.ActorPlayerStableId,
                    TaskTypeCode = preview.ActionCode + "Task",
                    FacilityStableId = targetStableId,
                    ActionCode = preview.ActionCode,
                    AssignedActorStableId = preview.ActorPlayerStableId,
                    AssignedCapacity = 1m,
                    AssignedCapacityUnitCode = "participant",
                    DurationTicks = preview.DurationTicks,
                    InputLotStableIds = inputIds,
                    OutputCandidateCodes = new[] { preview.ActionCode + "Completed" },
                    SourceStableIds = new[] { SimulationHostedWorldCodes.PolicySource },
                },
            };

        private void ObserveHostedWorldTaskCompletion(SimulationTaskSnapshot task,
            int completedWorldTick)
        {
            if (task.ActionCode == SimulationHostedWorldCodes.OpenHostedWorld)
            {
                var owner = task.AssignedActorStableId;
                var guest = task.InputLotStableIds.Single(value =>
                    !string.Equals(value, owner, StringComparison.Ordinal));
                hostedSessionModeCode = SimulationHostedWorldCodes.HostedMultiplayer;
                hostedSessionStableId = "hosted-session:" + SessionStableId.Substring(
                    "simulation-session:".Length);
                hostedCreatedAtWorldRevision = Revision;
                EnsureHostedProtectionCheckpoint(task.TaskStableId);
                hostedParticipants.Add(guest,
                    new SimulationHostedWorldParticipantSnapshot
                    {
                        PlayerStableId = guest,
                        ParticipantStateCode = SimulationHostedWorldCodes.Invited,
                        CurrentAreaSetStableId = SimulationAreaAccessCodes.FarmAreaSet,
                    });
                AddHostedAudit(task, SimulationHostedWorldCodes.HostedSessionOpened,
                    owner, guest, "world:" + SessionStableId, string.Empty,
                    completedWorldTick);
            }
            else if (task.ActionCode == SimulationHostedWorldCodes.JoinHostedWorld)
            {
                var guest = task.AssignedActorStableId;
                var participant = hostedParticipants[guest];
                participant.ParticipantStateCode = SimulationHostedWorldCodes.Active;
                participant.JoinedAtWorldRevision = Revision;
                AddDefaultHostedGuestGrants(guest);
                AddHostedAudit(task, SimulationHostedWorldCodes.HostedGuestJoined,
                    guest, guest, hostedSessionStableId, string.Empty,
                    completedWorldTick);
            }
            else if (task.ActionCode == SimulationHostedWorldCodes.HostedGuestAction)
            {
                var guest = task.AssignedActorStableId;
                var scope = task.InputLotStableIds.Single(value =>
                    value.StartsWith("area-set:", StringComparison.Ordinal));
                var capability = task.InputLotStableIds.Single(value =>
                    string.Equals(value, SimulationHostedWorldCodes.Observe,
                        StringComparison.Ordinal)
                    || string.Equals(value, SimulationHostedWorldCodes.Interact,
                        StringComparison.Ordinal)
                    || string.Equals(value, SimulationHostedWorldCodes.PerformWork,
                        StringComparison.Ordinal)
                    || string.Equals(value, SimulationHostedWorldCodes.Build,
                        StringComparison.Ordinal)
                    || string.Equals(value, SimulationHostedWorldCodes.Demolish,
                        StringComparison.Ordinal));
                AddHostedAudit(task,
                    capability == SimulationHostedWorldCodes.PerformWork
                        ? SimulationHostedWorldCodes.HostedGuestWorkCompleted
                        : task.ActionCode + "Completed",
                    guest, guest, scope, capability, completedWorldTick);
            }
        }

        private void AddDefaultHostedGuestGrants(string guest)
        {
            AddHostedGrant(guest, SimulationAreaAccessCodes.FarmAreaSet,
                SimulationHostedWorldCodes.Observe, SimulationHostedWorldCodes.Allow,
                SimulationHostedWorldCodes.Direct);
            AddHostedGrant(guest, SimulationAreaAccessCodes.FarmAreaSet,
                SimulationHostedWorldCodes.Interact, SimulationHostedWorldCodes.Allow,
                SimulationHostedWorldCodes.Direct);
            AddHostedGrant(guest, SimulationAreaAccessCodes.FarmAreaSet,
                SimulationHostedWorldCodes.PerformWork, SimulationHostedWorldCodes.Allow,
                SimulationHostedWorldCodes.Confirm);
            AddHostedGrant(guest, SimulationAreaAccessCodes.FarmAreaSet,
                SimulationHostedWorldCodes.Build, SimulationHostedWorldCodes.Deny,
                SimulationHostedWorldCodes.Confirm);
            AddHostedGrant(guest, SimulationAreaAccessCodes.FarmAreaSet,
                SimulationHostedWorldCodes.Demolish, SimulationHostedWorldCodes.Deny,
                SimulationHostedWorldCodes.HostApproval);
        }

        private void AddHostedGrant(string guest, string scope, string capability,
            string state, string risk)
        {
            hostedPermissionRevision++;
            var key = guest + "|" + scope + "|" + capability;
            hostedPermissionGrants.Add(key,
                new SimulationHostedWorldPermissionGrantSnapshot
                {
                    TargetPlayerStableId = guest,
                    ScopeStableId = scope,
                    CapabilityCode = capability,
                    GrantStateCode = state,
                    ActionRiskPolicyCode = risk,
                    GrantedByPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                    Revision = hostedPermissionRevision,
                    GrantHashSha256 = Sha256(key + "|" + state + "|" + risk
                        + "|" + hostedPermissionRevision),
                });
        }

        private SimulationHostedWorldPermissionGrantSnapshot? FindHostedGrant(
            string guest, string scope, string capability)
            => hostedPermissionGrants.TryGetValue(guest + "|" + scope + "|" + capability,
                out var grant) ? grant : null;

        private void AddHostedAudit(SimulationTaskSnapshot task, string effectType,
            string changedBy, string target, string scope, string capability, int tick)
            => hostedAuditTrail.Add(new SimulationHostedWorldAuditSnapshot
            {
                EffectStableId = "hosted-audit:" + task.TaskStableId,
                EffectTypeCode = effectType,
                ChangedByPlayerStableId = changedBy,
                TargetPlayerStableId = target,
                ScopeStableId = scope,
                CapabilityCode = capability,
                AppliedWorldTick = tick,
            });

        private SimulationHostedWorldStateSnapshot CreateHostedWorldSnapshot()
        {
            var participants = hostedParticipants.Values.OrderBy(value =>
                value.PlayerStableId, StringComparer.Ordinal).Select(CloneHostedParticipant).ToArray();
            var grants = hostedPermissionGrants.Values.OrderBy(value => value.TargetPlayerStableId,
                    StringComparer.Ordinal).ThenBy(value => value.CapabilityCode,
                    StringComparer.Ordinal).Select(CloneHostedGrant).ToArray();
            var audits = hostedAuditTrail.Select(CloneHostedAudit).ToArray();
            var hash = Sha256(string.Join("\u001e", new[]
            {
                SimulationHostedWorldCodes.RuleRevision, hostedSessionStableId,
                SessionStableId, SimulationAreaAccessCodes.PlayerOwner, hostedSessionModeCode,
                hostedCreatedAtWorldRevision.ToString(CultureInfo.InvariantCulture),
                hostedPermissionRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", participants.Select(value => value.PlayerStableId + "~" + value.ParticipantStateCode)),
                string.Join("|", grants.Select(value => value.GrantHashSha256)),
                string.Join("|", audits.Select(value => value.EffectStableId)),
            }));
            return new SimulationHostedWorldStateSnapshot
            {
                WorldRevision = Revision,
                WorldTick = CurrentTick,
                HostedSessionStableId = hostedSessionStableId,
                WorldStableId = SessionStableId,
                OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                SessionModeCode = hostedSessionModeCode,
                Participants = participants,
                PermissionGrants = grants,
                AuditTrail = audits,
                PermissionRevision = hostedPermissionRevision,
                CreatedAtWorldRevision = hostedCreatedAtWorldRevision,
                HostLossBlocksMutation = true,
                EscPausesWorld = false,
                SessionHashSha256 = hash,
            };
        }

        internal static SimulationHostedWorldStateSnapshot CloneHostedWorldState(
            SimulationHostedWorldStateSnapshot? source)
        {
            source ??= new SimulationHostedWorldStateSnapshot();
            return new SimulationHostedWorldStateSnapshot
            {
                WorldRevision = source.WorldRevision,
                WorldTick = source.WorldTick,
                HostedSessionStableId = source.HostedSessionStableId,
                WorldStableId = source.WorldStableId,
                OwnerPlayerStableId = source.OwnerPlayerStableId,
                SessionModeCode = source.SessionModeCode,
                JoinPolicyCode = source.JoinPolicyCode,
                DefaultGuestPermissionProfileCode = source.DefaultGuestPermissionProfileCode,
                Participants = source.Participants.Select(CloneHostedParticipant).ToArray(),
                PermissionGrants = source.PermissionGrants.Select(CloneHostedGrant).ToArray(),
                AuditTrail = source.AuditTrail.Select(CloneHostedAudit).ToArray(),
                PermissionRevision = source.PermissionRevision,
                CreatedAtWorldRevision = source.CreatedAtWorldRevision,
                HostLossBlocksMutation = source.HostLossBlocksMutation,
                EscPausesWorld = source.EscPausesWorld,
                SessionHashSha256 = source.SessionHashSha256,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
        }

        private static SimulationHostedWorldParticipantSnapshot CloneHostedParticipant(
            SimulationHostedWorldParticipantSnapshot value) => new()
        {
            PlayerStableId = value.PlayerStableId,
            ParticipantStateCode = value.ParticipantStateCode,
            CurrentAreaSetStableId = value.CurrentAreaSetStableId,
            JoinedAtWorldRevision = value.JoinedAtWorldRevision,
        };

        private static SimulationHostedWorldPermissionGrantSnapshot CloneHostedGrant(
            SimulationHostedWorldPermissionGrantSnapshot value) => new()
        {
            TargetPlayerStableId = value.TargetPlayerStableId,
            ScopeStableId = value.ScopeStableId,
            CapabilityCode = value.CapabilityCode,
            GrantStateCode = value.GrantStateCode,
            ActionRiskPolicyCode = value.ActionRiskPolicyCode,
            GrantedByPlayerStableId = value.GrantedByPlayerStableId,
            Revision = value.Revision,
            GrantHashSha256 = value.GrantHashSha256,
        };

        private static SimulationHostedWorldAuditSnapshot CloneHostedAudit(
            SimulationHostedWorldAuditSnapshot value) => new()
        {
            EffectStableId = value.EffectStableId,
            EffectTypeCode = value.EffectTypeCode,
            ChangedByPlayerStableId = value.ChangedByPlayerStableId,
            TargetPlayerStableId = value.TargetPlayerStableId,
            ScopeStableId = value.ScopeStableId,
            CapabilityCode = value.CapabilityCode,
            AppliedWorldTick = value.AppliedWorldTick,
        };

        private static void ValidateOpenRequest(SimulationHostedWorldOpenPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.OwnerPlayerStableId, "SimulationHostedOwnerInvalid");
            RequireStableId(request.InvitedGuestPlayerStableId, "SimulationHostedGuestInvalid");
        }

        private static void ValidateJoinRequest(SimulationHostedWorldJoinPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.GuestPlayerStableId, "SimulationHostedGuestInvalid");
        }

        private static void ValidateGuestActionRequest(SimulationHostedGuestActionPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.GuestPlayerStableId, "SimulationHostedGuestInvalid");
            RequireStableId(request.ScopeStableId, "SimulationHostedScopeInvalid");
            RequireStableId(request.CapabilityCode, "SimulationHostedCapabilityInvalid");
            RequireStableId(request.TargetStableId, "SimulationHostedTargetInvalid");
        }
    }
}
