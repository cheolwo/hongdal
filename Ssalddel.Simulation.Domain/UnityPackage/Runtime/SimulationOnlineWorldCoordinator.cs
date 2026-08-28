using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Domain,
        "온라인 세계·AreaSet·파티·계정 명상 원장의 결정적 상태 전이를 소유한다.",
        StepKey = "domain.online-world",
        DependsOnStepKeys = new[] { "application.online-world" },
        ExecutionStage = SsalddelCodeExecutionStage.Tick,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 40,
        Boundary = "온라인 Simulation 상태만 소유하고 Solo 저장·운영 상태·Unity 표현을 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "온라인 세계 수명·수용 인원·파티 원자 전환과 공유 격리 불변을 소유한다.",
        Boundary = "Domain 상태 전이는 실제 네트워크 부하·Unity 플레이 또는 E7 증거가 아니다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세션권위계약)]
    public sealed class SimulationOnlineWorldCoordinator
    {
        private readonly object gate = new object();
        private readonly Dictionary<string, SimulationOnlineWorldStateSnapshot> worlds =
            new Dictionary<string, SimulationOnlineWorldStateSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationAccountMeditationSnapshot> accounts =
            new Dictionary<string, SimulationAccountMeditationSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationOnlineCommandReceiptSnapshot> receipts =
            new Dictionary<string, SimulationOnlineCommandReceiptSnapshot>(StringComparer.Ordinal);
        private long directoryRevision;

        public SimulationOnlineWorldCoordinator()
        {
            var official = CreateOfficialWorld();
            worlds.Add(official.WorldStableId, official);
            directoryRevision = 1;
        }

        public SimulationOnlineWorldCoordinator(
            SimulationOnlineWorldCheckpointSnapshot checkpoint)
        {
            if (checkpoint == null) throw new ArgumentNullException(nameof(checkpoint));
            if (!string.Equals(checkpoint.SchemaCode,
                    SimulationOnlineWorldCodes.CheckpointSchema,
                    StringComparison.Ordinal)
                || !string.Equals(checkpoint.CheckpointHashSha256,
                    CalculateCheckpointHash(checkpoint), StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineWorldCheckpointInvalid");

            directoryRevision = checkpoint.DirectoryRevision;
            foreach (var world in checkpoint.Worlds)
            {
                var clone = CloneWorld(world);
                ValidateWorldHash(clone);
                worlds.Add(clone.WorldStableId, clone);
            }
            foreach (var account in checkpoint.AccountMeditations)
            {
                var clone = CloneAccount(account);
                if (!string.Equals(clone.StateHashSha256, AccountHash(clone),
                        StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationAccountMeditationHashMismatch");
                accounts.Add(clone.AccountPlayerStableId, clone);
            }
            foreach (var receipt in checkpoint.CommandReceipts)
                receipts.Add(receipt.CommandId, CloneReceipt(receipt));

            if (!worlds.ContainsKey(
                    SimulationOnlineWorldCodes.NatureCooperationWorldStableId))
                throw new SimulationConflictException(
                    "SimulationOfficialWorldMissing");
        }

        public SimulationOnlineWorldDirectorySnapshot Directory()
        {
            lock (gate)
            {
                var result = new SimulationOnlineWorldDirectorySnapshot
                {
                    DirectoryRevision = directoryRevision,
                    Worlds = worlds.Values.OrderBy(value => value.WorldStableId,
                        StringComparer.Ordinal).Select(CloneWorld).ToArray(),
                };
                result.DirectoryHashSha256 = Sha256(string.Join("\u001e",
                    new[]
                    {
                        result.RuleRevision,
                        result.DirectoryRevision.ToString(CultureInfo.InvariantCulture),
                        string.Join("|", result.Worlds.Select(value =>
                            value.StateHashSha256)),
                    }));
                return result;
            }
        }

        public SimulationOnlineWorldStateSnapshot RequireWorld(string worldStableId)
        {
            lock (gate)
                return CloneWorld(RequireWorldCore(worldStableId));
        }

        public bool IsConnectedParticipant(string worldStableId,
            string playerStableId)
        {
            lock (gate)
                return RequireWorldCore(worldStableId).Participants.Any(value =>
                    value.PlayerStableId == Require(playerStableId,
                        "SimulationOnlinePlayerStableIdInvalid")
                    && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected);
        }

        public SimulationOnlineWorldMutationResult CreatePrivateRoom(
            string actorPlayerStableId, SimulationPrivateRoomCreateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            Require(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ClientRequestId == Guid.Empty)
                throw new SimulationContractException(
                    "SimulationPrivateRoomClientRequestIdInvalid");
            var invited = (request.InvitedPlayerStableIds ?? Array.Empty<string>())
                .Select(value => Require(value,
                    "SimulationPrivateRoomInvitePlayerInvalid"))
                .Distinct(StringComparer.Ordinal).ToArray();
            if (invited.Length is < 1 or > 3 || invited.Contains(actor,
                    StringComparer.Ordinal))
                throw new SimulationContractException(
                    "SimulationPrivateRoomInviteCountInvalid");

            var worldId = "private-room:" + request.ClientRequestId.ToString("N");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "create-private", actor, worldId,
                string.Join(",", invited.OrderBy(value => value,
                    StringComparer.Ordinal)),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(request.CommandId, actor, payloadHash,
                    worldId);
                if (duplicate != null) return duplicate;
                if (worlds.ContainsKey(worldId))
                    throw new SimulationConflictException(
                        "SimulationPrivateRoomIdentityConflict");

                var world = new SimulationOnlineWorldStateSnapshot
                {
                    WorldStableId = worldId,
                    WorldKindCode = SimulationOnlineWorldCodes.PrivateHostedRoom,
                    StateCode = SimulationOnlineWorldCodes.Active,
                    JoinPolicyCode = SimulationOnlineWorldCodes.InviteOnly,
                    OwnerPlayerStableId = actor,
                    AlwaysActive = false,
                    MaximumParticipants =
                        SimulationOnlineWorldCodes.PrivateRoomMaximumPlayers,
                    WorldRevision = 1,
                    AreaSets = new[]
                    {
                        new SimulationOnlineAreaSetStateSnapshot
                        {
                            AreaSetStableId = SimulationOnlineWorldCodes
                                .NaturePrivateAreaSetStableId,
                            AuthoritySessionStableId =
                                CalculateAuthoritySessionStableId(worldId,
                                    SimulationOnlineWorldCodes
                                        .NaturePrivateAreaSetStableId),
                            AuthorityLocationCode =
                                SimulationOnlineWorldCodes.RemoteHost,
                            SessionBindingStateCode = SimulationOnlineWorldCodes
                                .AuthoritySessionReserved,
                            SessionBindingRevision = 1,
                            PartitionRevision = 1,
                            Capacity = SimulationOnlineWorldCodes
                                .PrivateRoomMaximumPlayers,
                            ConnectedParticipants = 1,
                        },
                    },
                    Participants = new[]
                    {
                        Participant(actor, SimulationOnlineWorldCodes.Connected,
                            SimulationOnlineWorldCodes.Owner, 1),
                    }.Concat(invited.Select(value => Participant(value,
                        SimulationOnlineWorldCodes.Invited,
                        SimulationOnlineWorldCodes.Member, 1))).ToArray(),
                };
                RefreshWorld(world);
                worlds.Add(worldId, world);
                directoryRevision++;
                AddReceipt(request.CommandId, actor, payloadHash,
                    SimulationOnlineWorldCodes.Joined, world);
                return Result(true, SimulationOnlineWorldCodes.Joined, world);
            }
        }

        public SimulationOnlineWorldMutationResult Join(string actorPlayerStableId,
            SimulationOnlineWorldJoinRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var command = Require(request.CommandId, "SimulationCommandIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var areaSetId = Require(request.AreaSetStableId,
                "SimulationOnlineAreaSetStableIdInvalid");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "join", actor, worldId, areaSetId,
                request.ExpectedWorldRevision.ToString(CultureInfo.InvariantCulture),
                request.ExpectedAreaSetRevision.ToString(CultureInfo.InvariantCulture),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(command, actor, payloadHash, worldId);
                if (duplicate != null) return duplicate;
                var world = RequireWorldCore(worldId);
                var area = RequireArea(world, areaSetId);
                RequireRevision(world.WorldRevision, request.ExpectedWorldRevision);
                if (area.PartitionRevision != request.ExpectedAreaSetRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineAreaSetRevisionMismatch");

                var participant = world.Participants.SingleOrDefault(value =>
                    value.PlayerStableId == actor);
                if (world.WorldKindCode ==
                        SimulationOnlineWorldCodes.PrivateHostedRoom
                    && participant == null)
                    throw new SimulationConflictException(
                        "SimulationPrivateRoomInviteRequired");
                if (participant?.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                    throw new SimulationConflictException(
                        "SimulationOnlineParticipantAlreadyConnected");

                var queued = area.ConnectedParticipants >= area.Capacity;
                world.WorldRevision++;
                area.PartitionRevision++;
                if (participant == null)
                {
                    participant = Participant(actor,
                        queued ? SimulationOnlineWorldCodes.Waiting
                            : SimulationOnlineWorldCodes.Connected,
                        SimulationOnlineWorldCodes.Member,
                        world.WorldRevision, areaSetId);
                    world.Participants = world.Participants.Append(participant)
                        .ToArray();
                }
                else
                {
                    participant.ParticipantStateCode = queued
                        ? SimulationOnlineWorldCodes.Waiting
                        : SimulationOnlineWorldCodes.Connected;
                    participant.AreaSetStableId = areaSetId;
                    participant.LastChangedAtWorldRevision = world.WorldRevision;
                }
                if (queued) area.WaitingParticipants++;
                else area.ConnectedParticipants++;

                if (world.WorldKindCode ==
                    SimulationOnlineWorldCodes.PrivateHostedRoom)
                {
                    world.StateCode = SimulationOnlineWorldCodes.Active;
                    if (actor == world.OwnerPlayerStableId)
                    {
                        world.TemporaryLeaderPlayerStableId = string.Empty;
                        foreach (var value in world.Participants)
                            value.AuthorityRoleCode = value.PlayerStableId == actor
                                ? SimulationOnlineWorldCodes.Owner
                                : SimulationOnlineWorldCodes.Member;
                    }
                }

                RefreshWorld(world);
                directoryRevision++;
                var resultCode = queued ? SimulationOnlineWorldCodes.Queued
                    : SimulationOnlineWorldCodes.Joined;
                AddReceipt(command, actor, payloadHash, resultCode, world);
                return Result(true, resultCode, world);
            }
        }

        public SimulationOnlineWorldMutationResult Leave(string actorPlayerStableId,
            SimulationOnlineWorldLeaveRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var command = Require(request.CommandId, "SimulationCommandIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "leave", actor, worldId,
                request.ExpectedWorldRevision.ToString(CultureInfo.InvariantCulture),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(command, actor, payloadHash, worldId);
                if (duplicate != null) return duplicate;
                var world = RequireWorldCore(worldId);
                RequireRevision(world.WorldRevision, request.ExpectedWorldRevision);
                var participant = world.Participants.SingleOrDefault(value =>
                    value.PlayerStableId == actor)
                    ?? throw new SimulationConflictException(
                        "SimulationOnlineParticipantRequired");
                if (participant.ParticipantStateCode !=
                        SimulationOnlineWorldCodes.Connected
                    && participant.ParticipantStateCode !=
                        SimulationOnlineWorldCodes.Waiting)
                    throw new SimulationConflictException(
                        "SimulationOnlineParticipantNotConnected");

                var area = RequireArea(world, participant.AreaSetStableId);
                if (participant.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                    area.ConnectedParticipants--;
                else
                    area.WaitingParticipants--;
                area.PartitionRevision++;
                world.WorldRevision++;
                participant.ParticipantStateCode = world.WorldKindCode ==
                    SimulationOnlineWorldCodes.PrivateHostedRoom
                    && actor != world.OwnerPlayerStableId
                        ? SimulationOnlineWorldCodes.Invited
                        : SimulationOnlineWorldCodes.Left;
                participant.PartyStableId = string.Empty;
                participant.LastChangedAtWorldRevision = world.WorldRevision;

                foreach (var party in world.Parties.Where(value =>
                    value.MemberPlayerStableIds.Contains(actor,
                        StringComparer.Ordinal)).ToArray())
                {
                    party.MemberPlayerStableIds = party.MemberPlayerStableIds
                        .Where(value => value != actor).ToArray();
                    if (party.MemberPlayerStableIds.Length == 0)
                        world.Parties = world.Parties.Where(value =>
                            value.PartyStableId != party.PartyStableId).ToArray();
                    else
                    {
                        if (party.LeaderPlayerStableId == actor)
                            party.LeaderPlayerStableId =
                                party.MemberPlayerStableIds[0];
                        party.Revision++;
                        RefreshParty(party);
                    }
                }

                if (world.WorldKindCode ==
                    SimulationOnlineWorldCodes.PrivateHostedRoom)
                    RefreshPrivateRoomLeadership(world);
                else
                    PromoteWaitingParticipant(world, area);

                RefreshWorld(world);
                directoryRevision++;
                AddReceipt(command, actor, payloadHash,
                    SimulationOnlineWorldCodes.LeftWorld, world);
                return Result(true, SimulationOnlineWorldCodes.LeftWorld, world);
            }
        }

        public SimulationOnlineWorldMutationResult CreateParty(
            string actorPlayerStableId, SimulationOnlinePartyCreateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var command = Require(request.CommandId, "SimulationCommandIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var partyId = Require(request.PartyStableId,
                "SimulationOnlinePartyStableIdInvalid");
            var members = (request.MemberPlayerStableIds ?? Array.Empty<string>())
                .Append(actor).Select(value => Require(value,
                    "SimulationOnlinePartyMemberInvalid"))
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            if (members.Length is < 2 or > SimulationOnlineWorldCodes.PartyMaximumPlayers)
                throw new SimulationContractException(
                    "SimulationOnlinePartySizeInvalid");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "party", actor, worldId, partyId, string.Join(",", members),
                request.ExpectedWorldRevision.ToString(CultureInfo.InvariantCulture),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(command, actor, payloadHash, worldId);
                if (duplicate != null) return duplicate;
                var world = RequireWorldCore(worldId);
                RequireRevision(world.WorldRevision, request.ExpectedWorldRevision);
                if (world.Parties.Any(value => value.PartyStableId == partyId))
                    throw new SimulationConflictException(
                        "SimulationOnlinePartyAlreadyExists");
                var participants = members.Select(member => world.Participants
                    .SingleOrDefault(value => value.PlayerStableId == member
                        && value.ParticipantStateCode ==
                        SimulationOnlineWorldCodes.Connected)
                    ?? throw new SimulationConflictException(
                        "SimulationOnlinePartyMemberNotConnected")).ToArray();
                if (participants.Select(value => value.AreaSetStableId)
                    .Distinct(StringComparer.Ordinal).Count() != 1
                    || participants.Any(value =>
                        !string.IsNullOrWhiteSpace(value.PartyStableId)))
                    throw new SimulationConflictException(
                        "SimulationOnlinePartyMemberUnavailable");

                world.WorldRevision++;
                var party = new SimulationOnlinePartySnapshot
                {
                    PartyStableId = partyId,
                    WorldStableId = worldId,
                    LeaderPlayerStableId = actor,
                    MemberPlayerStableIds = members,
                    Revision = 1,
                };
                RefreshParty(party);
                world.Parties = world.Parties.Append(party).ToArray();
                foreach (var participant in participants)
                {
                    participant.PartyStableId = partyId;
                    participant.LastChangedAtWorldRevision = world.WorldRevision;
                }
                RefreshWorld(world);
                directoryRevision++;
                AddReceipt(command, actor, payloadHash,
                    SimulationOnlineWorldCodes.PartyCreated, world);
                return Result(true, SimulationOnlineWorldCodes.PartyCreated, world);
            }
        }

        public SimulationOnlineWorldMutationResult SendFixedSignal(
            string actorPlayerStableId, SimulationFixedSignalSendRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var command = Require(request.CommandId, "SimulationCommandIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var signal = Require(request.SignalCode,
                "SimulationOnlineSignalCodeInvalid");
            if (!AllowedSignals.Contains(signal, StringComparer.Ordinal))
                throw new SimulationContractException(
                    "SimulationOnlineSignalCodeInvalid");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "signal", actor, worldId, request.PartyStableId ?? string.Empty,
                signal,
                request.ExpectedWorldRevision.ToString(CultureInfo.InvariantCulture),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(command, actor, payloadHash, worldId);
                if (duplicate != null) return duplicate;
                var world = RequireWorldCore(worldId);
                RequireRevision(world.WorldRevision, request.ExpectedWorldRevision);
                var participant = world.Participants.SingleOrDefault(value =>
                    value.PlayerStableId == actor
                    && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                    ?? throw new SimulationConflictException(
                        "SimulationOnlineParticipantRequired");
                if (!string.IsNullOrWhiteSpace(request.PartyStableId)
                    && !world.Parties.Any(value =>
                        value.PartyStableId == request.PartyStableId
                        && value.MemberPlayerStableIds.Contains(actor,
                            StringComparer.Ordinal)))
                    throw new SimulationConflictException(
                        "SimulationOnlinePartyMembershipRequired");

                world.WorldRevision++;
                var entry = new SimulationFixedSignalSnapshot
                {
                    SignalStableId = "signal:" + command,
                    WorldStableId = worldId,
                    PartyStableId = request.PartyStableId?.Trim() ?? string.Empty,
                    SenderPlayerStableId = actor,
                    SignalCode = signal,
                    AreaSetStableId = participant.AreaSetStableId,
                    AppliedWorldRevision = world.WorldRevision,
                };
                world.RecentSignals = world.RecentSignals.Append(entry)
                    .TakeLast(32).ToArray();
                RefreshWorld(world);
                directoryRevision++;
                AddReceipt(command, actor, payloadHash,
                    SimulationOnlineWorldCodes.SignalRecorded, world);
                return Result(true,
                    SimulationOnlineWorldCodes.SignalRecorded, world);
            }
        }

        public SimulationOnlineWorldMutationResult TransferAreaSet(
            string actorPlayerStableId,
            SimulationOnlineAreaSetTransferRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actor = Require(actorPlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var command = Require(request.CommandId, "SimulationCommandIdInvalid");
            var worldId = Require(request.WorldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var targetAreaSetId = Require(request.TargetAreaSetStableId,
                "SimulationOnlineAreaSetStableIdInvalid");
            var payloadHash = Sha256(string.Join("|", new[]
            {
                "transfer-area-set", actor, worldId, targetAreaSetId,
                request.ExpectedWorldRevision.ToString(CultureInfo.InvariantCulture),
                request.ExpectedSourceAreaSetRevision.ToString(
                    CultureInfo.InvariantCulture),
                request.ExpectedTargetAreaSetRevision.ToString(
                    CultureInfo.InvariantCulture),
            }));

            lock (gate)
            {
                var duplicate = Duplicate(command, actor, payloadHash, worldId);
                if (duplicate != null) return duplicate;
                var world = RequireWorldCore(worldId);
                RequireRevision(world.WorldRevision, request.ExpectedWorldRevision);
                var actorParticipant = world.Participants.SingleOrDefault(value =>
                    value.PlayerStableId == actor
                    && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                    ?? throw new SimulationConflictException(
                        "SimulationOnlineParticipantRequired");
                var sourceArea = RequireArea(world,
                    actorParticipant.AreaSetStableId);
                var targetArea = RequireArea(world, targetAreaSetId);
                if (sourceArea.AreaSetStableId == targetArea.AreaSetStableId)
                    throw new SimulationContractException(
                        "SimulationOnlineAreaSetTransferTargetUnchanged");
                if (sourceArea.PartitionRevision !=
                        request.ExpectedSourceAreaSetRevision
                    || targetArea.PartitionRevision !=
                        request.ExpectedTargetAreaSetRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineAreaSetRevisionMismatch");

                SimulationOnlineParticipantSnapshot[] moving;
                if (string.IsNullOrWhiteSpace(actorParticipant.PartyStableId))
                {
                    moving = new[] { actorParticipant };
                }
                else
                {
                    var party = world.Parties.Single(value =>
                        value.PartyStableId == actorParticipant.PartyStableId);
                    if (party.LeaderPlayerStableId != actor)
                        throw new SimulationConflictException(
                            "SimulationOnlinePartyLeaderRequired");
                    moving = party.MemberPlayerStableIds.Select(member =>
                        world.Participants.SingleOrDefault(value =>
                            value.PlayerStableId == member
                            && value.ParticipantStateCode ==
                            SimulationOnlineWorldCodes.Connected
                            && value.AreaSetStableId ==
                            sourceArea.AreaSetStableId)
                        ?? throw new SimulationConflictException(
                            "SimulationOnlinePartyAreaSetHandoverNotReady"))
                        .ToArray();
                }

                if (targetArea.ConnectedParticipants + moving.Length >
                    targetArea.Capacity)
                    throw new SimulationConflictException(
                        "SimulationOnlineAreaSetPartyCapacityInsufficient");

                world.WorldRevision++;
                sourceArea.PartitionRevision++;
                targetArea.PartitionRevision++;
                sourceArea.ConnectedParticipants -= moving.Length;
                targetArea.ConnectedParticipants += moving.Length;
                foreach (var participant in moving)
                {
                    participant.AreaSetStableId = targetArea.AreaSetStableId;
                    participant.LastChangedAtWorldRevision = world.WorldRevision;
                }
                RefreshWorld(world);
                directoryRevision++;
                AddReceipt(command, actor, payloadHash,
                    SimulationOnlineWorldCodes.AreaSetTransferred, world);
                return Result(true,
                    SimulationOnlineWorldCodes.AreaSetTransferred, world);
            }
        }

        public SimulationOnlineWorldMutationResult ApplyVerifiedMeditation(
            SimulationVerifiedMeditationContributionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var player = Require(request.PlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var source = Require(request.SourceActionRecordStableId,
                "SimulationOnlineActionRecordStableIdInvalid");
            var rule = Require(request.RuleRevision,
                "SimulationMeditationRuleRevisionInvalid");
            if (request.MeditationExperienceMilli <= 0
                || request.MeditationExperienceMilli > 1000)
                throw new SimulationContractException(
                    "SimulationMeditationContributionInvalid");

            lock (gate)
            {
                var world = RequireWorldCore(request.WorldStableId);
                if (world.WorldKindCode == SimulationOnlineWorldCodes.SoloLocalWorld
                    || !world.Participants.Any(value =>
                        value.PlayerStableId == player
                        && value.ParticipantStateCode ==
                        SimulationOnlineWorldCodes.Connected))
                    throw new SimulationConflictException(
                        "SimulationOnlineVerifiedParticipantRequired");
                var contributionId = "account-meditation:" + Sha256(string.Join("|",
                    new[] { player, source, rule }));
                if (!accounts.TryGetValue(player, out var account))
                {
                    account = new SimulationAccountMeditationSnapshot
                    {
                        AccountPlayerStableId = player,
                        MeditationStageCode = Simulation분야단계Codes.미경험,
                    };
                    accounts.Add(player, account);
                }
                var existing = account.Contributions.SingleOrDefault(value =>
                    value.ContributionStableId == contributionId);
                if (existing != null)
                {
                    if (existing.WorldStableId != request.WorldStableId
                        || existing.MeditationExperienceMilli !=
                            request.MeditationExperienceMilli
                        || existing.SourceActionWorldRevision !=
                            request.SourceActionWorldRevision)
                        throw new SimulationConflictException(
                            "SimulationMeditationContributionConflict");
                    return Result(false,
                        SimulationOnlineWorldCodes.MeditationApplied, world,
                        account);
                }
                if (request.SourceActionWorldRevision <= 0
                    || request.ExpectedOnlineWorldRevision !=
                    world.WorldRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineContributionRevisionInvalid");

                account.Revision++;
                account.Contributions = account.Contributions.Append(
                    new SimulationAccountMeditationContributionSnapshot
                    {
                        ContributionStableId = contributionId,
                        AccountPlayerStableId = player,
                        WorldStableId = request.WorldStableId,
                        SourceActionRecordStableId = source,
                        MeditationExperienceMilli =
                            request.MeditationExperienceMilli,
                        SourceActionWorldRevision =
                            request.SourceActionWorldRevision,
                        AppliedOnlineWorldRevision = world.WorldRevision,
                        RuleRevision = rule,
                    }).OrderBy(value => value.ContributionStableId,
                        StringComparer.Ordinal).ToArray();
                RefreshAccount(account);
                return Result(true,
                    SimulationOnlineWorldCodes.MeditationApplied, world,
                    account);
            }
        }

        public SimulationOnlineWorldMutationResult ApplyVerifiedObjectiveContribution(
            SimulationVerifiedObjectiveContributionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var player = Require(request.PlayerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            var source = Require(request.SourceActionRecordStableId,
                "SimulationOnlineActionRecordStableIdInvalid");
            if (request.ContributionUnits <= 0
                || request.ContributionUnits > 100)
                throw new SimulationContractException(
                    "SimulationOnlineObjectiveContributionInvalid");

            lock (gate)
            {
                var world = RequireWorldCore(request.WorldStableId);
                if (world.WorldKindCode !=
                    SimulationOnlineWorldCodes.OfficialPersistentWorld)
                    throw new SimulationConflictException(
                        "SimulationOfficialWorldRequired");
                if (!world.Participants.Any(value =>
                    value.PlayerStableId == player
                    && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected))
                    throw new SimulationConflictException(
                        "SimulationOnlineParticipantRequired");
                if (request.AppliedWorldRevision <= 0
                    || request.AppliedWorldRevision > world.WorldRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineContributionRevisionInvalid");
                var objective = world.Objectives.SingleOrDefault(value =>
                    value.ObjectiveStableId == request.ObjectiveStableId)
                    ?? throw new SimulationNotFoundException(
                        "SimulationOnlineObjectiveNotFound");
                var contributionId = "objective-contribution:" + Sha256(
                    string.Join("|", new[]
                    {
                        request.WorldStableId, request.ObjectiveStableId,
                        player, source,
                    }));
                if (objective.ContributionStableIds.Contains(contributionId,
                        StringComparer.Ordinal))
                    return Result(false,
                        SimulationOnlineWorldCodes.ObjectiveContributionApplied,
                        world);

                world.WorldRevision++;
                objective.Revision++;
                objective.CurrentContributionUnits = checked(
                    objective.CurrentContributionUnits + request.ContributionUnits);
                objective.ContributionStableIds = objective.ContributionStableIds
                    .Append(contributionId).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray();
                if (objective.CurrentContributionUnits >=
                    objective.RequiredContributionUnits)
                    objective.StateCode = "Completed";
                RefreshObjective(objective);
                RefreshWorld(world);
                directoryRevision++;
                return Result(true,
                    SimulationOnlineWorldCodes.ObjectiveContributionApplied,
                    world);
            }
        }

        public SimulationAccountMeditationSnapshot AccountMeditation(
            string playerStableId)
        {
            var player = Require(playerStableId,
                "SimulationOnlinePlayerStableIdInvalid");
            lock (gate)
                return accounts.TryGetValue(player, out var account)
                    ? CloneAccount(account)
                    : EmptyAccount(player);
        }

        public SimulationOnlineWorldCheckpointSnapshot CaptureCheckpoint()
        {
            lock (gate)
            {
                var checkpoint = new SimulationOnlineWorldCheckpointSnapshot
                {
                    DirectoryRevision = directoryRevision,
                    Worlds = worlds.Values.OrderBy(value => value.WorldStableId,
                        StringComparer.Ordinal).Select(CloneWorld).ToArray(),
                    AccountMeditations = accounts.Values.OrderBy(value =>
                        value.AccountPlayerStableId, StringComparer.Ordinal)
                        .Select(CloneAccount).ToArray(),
                    CommandReceipts = receipts.Values.OrderBy(value =>
                        value.CommandId, StringComparer.Ordinal)
                        .Select(CloneReceipt).ToArray(),
                };
                checkpoint.CheckpointHashSha256 =
                    CalculateCheckpointHash(checkpoint);
                return checkpoint;
            }
        }

        public static string CalculateCheckpointHash(
            SimulationOnlineWorldCheckpointSnapshot checkpoint)
            => Sha256(string.Join("\u001e", new[]
            {
                checkpoint.SchemaCode ?? string.Empty,
                checkpoint.DirectoryRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", (checkpoint.Worlds ?? Array.Empty<
                    SimulationOnlineWorldStateSnapshot>()).OrderBy(value =>
                    value.WorldStableId, StringComparer.Ordinal).Select(value =>
                    value.StateHashSha256)),
                string.Join("|", (checkpoint.AccountMeditations ?? Array.Empty<
                    SimulationAccountMeditationSnapshot>()).OrderBy(value =>
                    value.AccountPlayerStableId, StringComparer.Ordinal)
                    .Select(value => value.StateHashSha256)),
                string.Join("|", (checkpoint.CommandReceipts ?? Array.Empty<
                    SimulationOnlineCommandReceiptSnapshot>()).OrderBy(value =>
                    value.CommandId, StringComparer.Ordinal).Select(value =>
                    string.Join("~", value.CommandId, value.ActorPlayerStableId,
                        value.PayloadHashSha256, value.ResultCode,
                        value.WorldStableId,
                        value.ResultingWorldRevision.ToString(
                            CultureInfo.InvariantCulture)))),
            }));

        private static readonly string[] AllowedSignals =
        {
            SimulationOnlineWorldCodes.NeedHelp,
            SimulationOnlineWorldCodes.GatherHere,
            SimulationOnlineWorldCodes.ThreatFound,
            SimulationOnlineWorldCodes.ResourceFound,
            SimulationOnlineWorldCodes.ObjectiveReady,
        };

        private static SimulationOnlineWorldStateSnapshot CreateOfficialWorld()
        {
            var world = new SimulationOnlineWorldStateSnapshot
            {
                WorldStableId = SimulationOnlineWorldCodes
                    .NatureCooperationWorldStableId,
                WorldKindCode = SimulationOnlineWorldCodes
                    .OfficialPersistentWorld,
                StateCode = SimulationOnlineWorldCodes.Active,
                JoinPolicyCode = SimulationOnlineWorldCodes.PublicAuthenticated,
                AlwaysActive = true,
                MaximumParticipants = SimulationOnlineWorldCodes
                    .OfficialAreaSetCapacity * SimulationOnlineWorldCodes
                    .NatureOfficialAreaSetStableIds.Length,
                WorldRevision = 1,
                AreaSets = SimulationOnlineWorldCodes.NatureOfficialAreaSetStableIds
                    .Select(value => new SimulationOnlineAreaSetStateSnapshot
                    {
                        AreaSetStableId = value,
                        AuthoritySessionStableId =
                            CalculateAuthoritySessionStableId(
                                SimulationOnlineWorldCodes
                                    .NatureCooperationWorldStableId, value),
                        AuthorityLocationCode =
                            SimulationOnlineWorldCodes.RemoteHost,
                        SessionBindingStateCode = SimulationOnlineWorldCodes
                            .AuthoritySessionReserved,
                        SessionBindingRevision = 1,
                        PartitionRevision = 1,
                        Capacity = SimulationOnlineWorldCodes
                            .OfficialAreaSetCapacity,
                    }).ToArray(),
                Objectives = new[]
                {
                    new SimulationCooperativeObjectiveSnapshot
                    {
                        ObjectiveStableId = SimulationOnlineWorldCodes
                            .NatureCooperationObjectiveStableId,
                        WorldStableId = SimulationOnlineWorldCodes
                            .NatureCooperationWorldStableId,
                        StateCode = SimulationOnlineWorldCodes.Active,
                        RequiredContributionUnits = 1000,
                        Revision = 1,
                    },
                },
            };
            RefreshObjective(world.Objectives[0]);
            RefreshWorld(world);
            return world;
        }

        private static SimulationOnlineParticipantSnapshot Participant(
            string player, string state, string role, long revision,
            string? areaSet = null) => new SimulationOnlineParticipantSnapshot
            {
                PlayerStableId = player,
                ParticipantStateCode = state,
                AreaSetStableId = areaSet ?? SimulationOnlineWorldCodes
                    .NaturePrivateAreaSetStableId,
                AuthorityRoleCode = role,
                JoinedAtWorldRevision = revision,
                LastChangedAtWorldRevision = revision,
            };

        private SimulationOnlineWorldStateSnapshot RequireWorldCore(
            string worldStableId)
        {
            var id = Require(worldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            return worlds.TryGetValue(id, out var world) ? world
                : throw new SimulationNotFoundException(
                    "SimulationOnlineWorldNotFound");
        }

        private static SimulationOnlineAreaSetStateSnapshot RequireArea(
            SimulationOnlineWorldStateSnapshot world, string areaSetStableId)
            => world.AreaSets.SingleOrDefault(value =>
                    value.AreaSetStableId == areaSetStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationOnlineAreaSetNotFound");

        private static void RequireRevision(long actual, long expected)
        {
            if (actual != expected)
                throw new SimulationConflictException(
                    "SimulationOnlineWorldRevisionMismatch");
        }

        private SimulationOnlineWorldMutationResult? Duplicate(string commandId,
            string actor, string payloadHash, string worldId)
        {
            if (!receipts.TryGetValue(commandId, out var receipt)) return null;
            if (receipt.ActorPlayerStableId != actor
                || receipt.PayloadHashSha256 != payloadHash
                || receipt.WorldStableId != worldId)
                throw new SimulationConflictException(
                    "SimulationOnlineCommandPayloadConflict");
            return Result(false, receipt.ResultCode, RequireWorldCore(worldId));
        }

        private void AddReceipt(string commandId, string actor,
            string payloadHash, string resultCode,
            SimulationOnlineWorldStateSnapshot world)
            => receipts.Add(commandId, new SimulationOnlineCommandReceiptSnapshot
            {
                CommandId = commandId,
                ActorPlayerStableId = actor,
                PayloadHashSha256 = payloadHash,
                ResultCode = resultCode,
                WorldStableId = world.WorldStableId,
                ResultingWorldRevision = world.WorldRevision,
            });

        private static void RefreshPrivateRoomLeadership(
            SimulationOnlineWorldStateSnapshot world)
        {
            var connected = world.Participants.Where(value =>
                    value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Connected)
                .OrderBy(value => value.JoinedAtWorldRevision)
                .ThenBy(value => value.PlayerStableId, StringComparer.Ordinal)
                .ToArray();
            if (connected.Length == 0)
            {
                world.StateCode = SimulationOnlineWorldCodes.Suspended;
                world.TemporaryLeaderPlayerStableId = string.Empty;
                return;
            }
            world.StateCode = SimulationOnlineWorldCodes.Active;
            var ownerConnected = connected.Any(value =>
                value.PlayerStableId == world.OwnerPlayerStableId);
            world.TemporaryLeaderPlayerStableId = ownerConnected
                ? string.Empty : connected[0].PlayerStableId;
            foreach (var participant in world.Participants)
                participant.AuthorityRoleCode = participant.PlayerStableId ==
                    world.OwnerPlayerStableId
                    ? SimulationOnlineWorldCodes.Owner
                    : participant.PlayerStableId ==
                        world.TemporaryLeaderPlayerStableId
                        ? SimulationOnlineWorldCodes.TemporaryLeader
                        : SimulationOnlineWorldCodes.Member;
        }

        private static void PromoteWaitingParticipant(
            SimulationOnlineWorldStateSnapshot world,
            SimulationOnlineAreaSetStateSnapshot area)
        {
            if (area.ConnectedParticipants >= area.Capacity) return;
            var waiting = world.Participants.Where(value =>
                    value.AreaSetStableId == area.AreaSetStableId
                    && value.ParticipantStateCode ==
                    SimulationOnlineWorldCodes.Waiting)
                .OrderBy(value => value.JoinedAtWorldRevision)
                .ThenBy(value => value.PlayerStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (waiting == null) return;
            waiting.ParticipantStateCode = SimulationOnlineWorldCodes.Connected;
            waiting.LastChangedAtWorldRevision = world.WorldRevision;
            area.WaitingParticipants--;
            area.ConnectedParticipants++;
        }

        private static SimulationOnlineWorldMutationResult Result(bool applied,
            string resultCode, SimulationOnlineWorldStateSnapshot world,
            SimulationAccountMeditationSnapshot? account = null)
            => new SimulationOnlineWorldMutationResult
            {
                Applied = applied,
                ResultCode = resultCode,
                World = CloneWorld(world),
                AccountMeditation = account == null ? null : CloneAccount(account),
            };

        private static void RefreshWorld(SimulationOnlineWorldStateSnapshot world)
        {
            foreach (var area in world.AreaSets) RefreshArea(area);
            foreach (var party in world.Parties) RefreshParty(party);
            foreach (var objective in world.Objectives) RefreshObjective(objective);
            world.StateHashSha256 = WorldHash(world);
        }

        private static void RefreshArea(SimulationOnlineAreaSetStateSnapshot area)
            => area.StateHashSha256 = Sha256(string.Join("|", new[]
            {
                area.AreaSetStableId,
                area.AuthoritySessionStableId,
                area.AuthorityLocationCode,
                area.SessionBindingStateCode,
                area.SessionBindingRevision.ToString(
                    CultureInfo.InvariantCulture),
                area.PartitionRevision.ToString(CultureInfo.InvariantCulture),
                area.Capacity.ToString(CultureInfo.InvariantCulture),
                area.ConnectedParticipants.ToString(CultureInfo.InvariantCulture),
                area.WaitingParticipants.ToString(CultureInfo.InvariantCulture),
            }));

        private static void RefreshParty(SimulationOnlinePartySnapshot party)
            => party.StateHashSha256 = Sha256(string.Join("|", new[]
            {
                party.PartyStableId, party.WorldStableId,
                party.LeaderPlayerStableId,
                party.Revision.ToString(CultureInfo.InvariantCulture),
                string.Join(",", party.MemberPlayerStableIds.OrderBy(value =>
                    value, StringComparer.Ordinal)),
            }));

        private static void RefreshObjective(
            SimulationCooperativeObjectiveSnapshot objective)
            => objective.StateHashSha256 = Sha256(string.Join("|", new[]
            {
                objective.ObjectiveStableId, objective.WorldStableId,
                objective.StateCode,
                objective.RequiredContributionUnits.ToString(
                    CultureInfo.InvariantCulture),
                objective.CurrentContributionUnits.ToString(
                    CultureInfo.InvariantCulture),
                objective.Revision.ToString(CultureInfo.InvariantCulture),
                string.Join(",", objective.ContributionStableIds.OrderBy(value =>
                    value, StringComparer.Ordinal)),
            }));

        private static void RefreshAccount(
            SimulationAccountMeditationSnapshot account)
        {
            account.MeditationExperienceMilli = account.Contributions.Sum(value =>
                value.MeditationExperienceMilli);
            account.MeditationProficiency = checked((int)(
                account.MeditationExperienceMilli /
                Simulation집중판정Codes.MilliPerPoint));
            account.MeditationStageCode = account.MeditationProficiency switch
            {
                >= 12 => Simulation분야단계Codes.숙련,
                >= 5 => Simulation분야단계Codes.익숙함,
                >= 1 => Simulation분야단계Codes.기초,
                _ => Simulation분야단계Codes.미경험,
            };
            account.StateHashSha256 = AccountHash(account);
        }

        private static string WorldHash(SimulationOnlineWorldStateSnapshot world)
            => Sha256(string.Join("\u001e", new[]
            {
                SimulationOnlineWorldCodes.RuleRevision,
                world.WorldStableId, world.WorldKindCode, world.StateCode,
                world.JoinPolicyCode, world.OwnerPlayerStableId,
                world.TemporaryLeaderPlayerStableId,
                world.AlwaysActive ? "1" : "0",
                world.MaximumParticipants.ToString(CultureInfo.InvariantCulture),
                world.WorldRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", world.AreaSets.OrderBy(value =>
                    value.AreaSetStableId, StringComparer.Ordinal).Select(value =>
                    value.StateHashSha256)),
                string.Join("|", world.Participants.OrderBy(value =>
                    value.PlayerStableId, StringComparer.Ordinal).Select(value =>
                    string.Join("~", value.PlayerStableId,
                        value.ParticipantStateCode, value.AreaSetStableId,
                        value.PartyStableId, value.AuthorityRoleCode,
                        value.JoinedAtWorldRevision.ToString(
                            CultureInfo.InvariantCulture),
                        value.LastChangedAtWorldRevision.ToString(
                            CultureInfo.InvariantCulture)))),
                string.Join("|", world.Parties.OrderBy(value =>
                    value.PartyStableId, StringComparer.Ordinal).Select(value =>
                    value.StateHashSha256)),
                string.Join("|", world.Objectives.OrderBy(value =>
                    value.ObjectiveStableId, StringComparer.Ordinal).Select(value =>
                    value.StateHashSha256)),
                string.Join("|", world.RecentSignals.Select(value =>
                    string.Join("~", value.SignalStableId, value.WorldStableId,
                        value.PartyStableId, value.SenderPlayerStableId,
                        value.SignalCode, value.AreaSetStableId,
                        value.AppliedWorldRevision.ToString(
                            CultureInfo.InvariantCulture)))),
            }));

        private static string AccountHash(
            SimulationAccountMeditationSnapshot account)
            => Sha256(string.Join("\u001e", new[]
            {
                account.AccountPlayerStableId,
                account.Revision.ToString(CultureInfo.InvariantCulture),
                account.MeditationExperienceMilli.ToString(
                    CultureInfo.InvariantCulture),
                account.MeditationProficiency.ToString(
                    CultureInfo.InvariantCulture),
                account.MeditationStageCode,
                string.Join("|", account.Contributions.OrderBy(value =>
                    value.ContributionStableId, StringComparer.Ordinal).Select(value =>
                    string.Join("~", value.ContributionStableId,
                        value.AccountPlayerStableId, value.WorldStableId,
                        value.SourceActionRecordStableId,
                        value.MeditationExperienceMilli.ToString(
                            CultureInfo.InvariantCulture),
                        value.SourceActionWorldRevision.ToString(
                            CultureInfo.InvariantCulture),
                        value.AppliedOnlineWorldRevision.ToString(
                            CultureInfo.InvariantCulture), value.RuleRevision))),
            }));

        private static void ValidateWorldHash(
            SimulationOnlineWorldStateSnapshot world)
        {
            if (!string.Equals(world.StateHashSha256, WorldHash(world),
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineWorldHashMismatch");
        }

        private static SimulationAccountMeditationSnapshot EmptyAccount(
            string player) => new SimulationAccountMeditationSnapshot
            {
                AccountPlayerStableId = player,
                MeditationStageCode = Simulation분야단계Codes.미경험,
                StateHashSha256 = AccountHash(
                    new SimulationAccountMeditationSnapshot
                    {
                        AccountPlayerStableId = player,
                        MeditationStageCode = Simulation분야단계Codes.미경험,
                    }),
            };

        public static SimulationOnlineWorldStateSnapshot CloneWorld(
            SimulationOnlineWorldStateSnapshot source)
            => new SimulationOnlineWorldStateSnapshot
            {
                WorldStableId = source.WorldStableId,
                WorldKindCode = source.WorldKindCode,
                StateCode = source.StateCode,
                JoinPolicyCode = source.JoinPolicyCode,
                OwnerPlayerStableId = source.OwnerPlayerStableId,
                TemporaryLeaderPlayerStableId =
                    source.TemporaryLeaderPlayerStableId,
                AlwaysActive = source.AlwaysActive,
                MaximumParticipants = source.MaximumParticipants,
                WorldRevision = source.WorldRevision,
                AreaSets = source.AreaSets.Select(value =>
                    new SimulationOnlineAreaSetStateSnapshot
                    {
                        AreaSetStableId = value.AreaSetStableId,
                        AuthoritySessionStableId =
                            value.AuthoritySessionStableId,
                        AuthorityLocationCode = value.AuthorityLocationCode,
                        SessionBindingStateCode = value.SessionBindingStateCode,
                        SessionBindingRevision = value.SessionBindingRevision,
                        PartitionRevision = value.PartitionRevision,
                        Capacity = value.Capacity,
                        ConnectedParticipants = value.ConnectedParticipants,
                        WaitingParticipants = value.WaitingParticipants,
                        StateHashSha256 = value.StateHashSha256,
                    }).ToArray(),
                Participants = source.Participants.Select(value =>
                    new SimulationOnlineParticipantSnapshot
                    {
                        PlayerStableId = value.PlayerStableId,
                        ParticipantStateCode = value.ParticipantStateCode,
                        AreaSetStableId = value.AreaSetStableId,
                        PartyStableId = value.PartyStableId,
                        AuthorityRoleCode = value.AuthorityRoleCode,
                        JoinedAtWorldRevision = value.JoinedAtWorldRevision,
                        LastChangedAtWorldRevision =
                            value.LastChangedAtWorldRevision,
                    }).ToArray(),
                Parties = source.Parties.Select(value =>
                    new SimulationOnlinePartySnapshot
                    {
                        PartyStableId = value.PartyStableId,
                        WorldStableId = value.WorldStableId,
                        LeaderPlayerStableId = value.LeaderPlayerStableId,
                        MemberPlayerStableIds = value.MemberPlayerStableIds.ToArray(),
                        Revision = value.Revision,
                        StateHashSha256 = value.StateHashSha256,
                    }).ToArray(),
                Objectives = source.Objectives.Select(value =>
                    new SimulationCooperativeObjectiveSnapshot
                    {
                        ObjectiveStableId = value.ObjectiveStableId,
                        WorldStableId = value.WorldStableId,
                        StateCode = value.StateCode,
                        RequiredContributionUnits =
                            value.RequiredContributionUnits,
                        CurrentContributionUnits =
                            value.CurrentContributionUnits,
                        ContributionStableIds =
                            value.ContributionStableIds.ToArray(),
                        Revision = value.Revision,
                        StateHashSha256 = value.StateHashSha256,
                    }).ToArray(),
                RecentSignals = source.RecentSignals.Select(value =>
                    new SimulationFixedSignalSnapshot
                    {
                        SignalStableId = value.SignalStableId,
                        WorldStableId = value.WorldStableId,
                        PartyStableId = value.PartyStableId,
                        SenderPlayerStableId = value.SenderPlayerStableId,
                        SignalCode = value.SignalCode,
                        AreaSetStableId = value.AreaSetStableId,
                        AppliedWorldRevision = value.AppliedWorldRevision,
                    }).ToArray(),
                StateHashSha256 = source.StateHashSha256,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        public static string CalculateAuthoritySessionStableId(
            string worldStableId, string areaSetStableId)
        {
            var world = Require(worldStableId,
                "SimulationOnlineWorldStableIdInvalid");
            var area = Require(areaSetStableId,
                "SimulationOnlineAreaSetStableIdInvalid");
            return "simulation-session:" + Sha256(string.Join("|",
                new[] { "online-authority-session", world, area,
                    SimulationOnlineWorldCodes.RuleRevision }))
                .Substring(0, 32);
        }

        public static SimulationAccountMeditationSnapshot CloneAccount(
            SimulationAccountMeditationSnapshot source)
            => new SimulationAccountMeditationSnapshot
            {
                AccountPlayerStableId = source.AccountPlayerStableId,
                Revision = source.Revision,
                MeditationExperienceMilli = source.MeditationExperienceMilli,
                MeditationProficiency = source.MeditationProficiency,
                MeditationStageCode = source.MeditationStageCode,
                Contributions = source.Contributions.Select(value =>
                    new SimulationAccountMeditationContributionSnapshot
                    {
                        ContributionStableId = value.ContributionStableId,
                        AccountPlayerStableId = value.AccountPlayerStableId,
                        WorldStableId = value.WorldStableId,
                        SourceActionRecordStableId =
                            value.SourceActionRecordStableId,
                        MeditationExperienceMilli =
                            value.MeditationExperienceMilli,
                        SourceActionWorldRevision =
                            value.SourceActionWorldRevision,
                        AppliedOnlineWorldRevision =
                            value.AppliedOnlineWorldRevision,
                        RuleRevision = value.RuleRevision,
                    }).ToArray(),
                StateHashSha256 = source.StateHashSha256,
            };

        private static SimulationOnlineCommandReceiptSnapshot CloneReceipt(
            SimulationOnlineCommandReceiptSnapshot source)
            => new SimulationOnlineCommandReceiptSnapshot
            {
                CommandId = source.CommandId,
                ActorPlayerStableId = source.ActorPlayerStableId,
                PayloadHashSha256 = source.PayloadHashSha256,
                ResultCode = source.ResultCode,
                WorldStableId = source.WorldStableId,
                ResultingWorldRevision = source.ResultingWorldRevision,
            };

        private static string Require(string? value, string errorCode)
            => !string.IsNullOrWhiteSpace(value) ? value.Trim()
                : throw new SimulationContractException(errorCode);

        private static string Sha256(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(bytes).Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
