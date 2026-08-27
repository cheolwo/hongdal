using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private Simulation행위기록LedgerSnapshot? actionManifestationLedgerState;
        private Simulation플레이어분야ProfileSnapshot? playerDomainProfileState;

        internal (Simulation행위발현Record 기록,
            Simulation분야성장적용Snapshot 성장적용, bool 재사용)
            AppendActionManifestationAndProgression(
                Simulation행위발현Record draft)
        {
            if (draft == null)
                throw new SimulationContractException(
                    "SimulationActionManifestationRecordRequired");

            var catalog = Simulation기본플레이어분야Catalog.Create();
            var binding = catalog.Wi결속들.SingleOrDefault(value =>
                string.Equals(value.WorldInteractionId,
                    draft.WorldInteractionId, StringComparison.Ordinal))
                ?? throw new SimulationContractException(
                    "SimulationPlayerDomainWiBindingMissing");
            var playerStableId = RequireActionPlayerStableId(draft);
            var canApplyField = string.Equals(draft.TriggerSourceCode,
                                    SimulationWorldInteractionTriggerSourceCodes
                                        .PlayerDriven,
                                    StringComparison.Ordinal)
                                && string.Equals(draft.ActorStableId,
                                    playerStableId, StringComparison.Ordinal)
                                && (binding.기여방식Code ==
                                    Simulation분야기여방식Codes.PlayerDirect
                                    || binding.기여방식Code ==
                                    Simulation분야기여방식Codes.PlayerOrOperation)
                                && draft.결과분류Code !=
                                    Simulation행위결과분류Codes.취소;
            if (canApplyField)
                draft.변화의미Codes = (draft.변화의미Codes
                        ?? Array.Empty<string>())
                    .Concat(new[]
                    {
                        Simulation행위변화의미Codes.플레이어진척변경,
                    }).Distinct(StringComparer.Ordinal).ToArray();

            var ledger = actionManifestationLedgerState == null
                ? new Simulation행위발현Ledger(draft.WorldStableId)
                : Simulation행위발현Ledger.Restore(
                    actionManifestationLedgerState);
            var beforeCount = ledger.Snapshot().TailRecords.Length;
            var record = ledger.Append(draft);
            var reused = ledger.Snapshot().TailRecords.Length == beforeCount;

            var playerDomain = playerDomainProfileState == null
                ? new Simulation플레이어분야Engine(playerStableId, catalog)
                : Simulation플레이어분야Engine.Restore(
                    playerDomainProfileState, catalog);
            var beforeProfileRevision = playerDomain.Snapshot().Revision;
            string status;
            string reason;
            if (reused)
            {
                status = Simulation분야성장적용상태Codes.Reused;
                reason = "IdempotentCommandReused";
            }
            else if (canApplyField)
            {
                playerDomain.ApplyField(new Simulation현장숙련기여Request
                {
                    PlayerStableId = playerStableId,
                    행위기록 = record,
                });
                status = Simulation분야성장적용상태Codes.Applied;
                reason = string.Empty;
            }
            else
            {
                status = Simulation분야성장적용상태Codes.NotApplicable;
                reason = PlayerProgressNotApplicableReason(draft, binding);
            }

            actionManifestationLedgerState = ledger.Snapshot();
            playerDomainProfileState = playerDomain.Snapshot();
            return (record, new Simulation분야성장적용Snapshot
            {
                상태Code = status,
                사유Code = reason,
                PlayerStableId = playerStableId,
                BeforeProfileRevision = beforeProfileRevision,
                AfterProfileRevision = playerDomainProfileState.Revision,
            }, reused);
        }

        internal (Simulation행위발현Record? 기록,
            Simulation분야성장적용Snapshot 성장적용)
            FindActionManifestation(string commandId)
        {
            var record = actionManifestationLedgerState?.TailRecords
                .SingleOrDefault(value => string.Equals(value.CommandId,
                    commandId, StringComparison.Ordinal));
            return (record, new Simulation분야성장적용Snapshot
            {
                상태Code = record == null
                    ? Simulation분야성장적용상태Codes.NotApplicable
                    : Simulation분야성장적용상태Codes.Reused,
                사유Code = record == null
                    ? "LegacyCommandWithoutActionRecord"
                    : "IdempotentCommandReused",
                PlayerStableId = playerDomainProfileState?.PlayerStableId
                    ?? string.Empty,
                BeforeProfileRevision = playerDomainProfileState?.Revision ?? 0,
                AfterProfileRevision = playerDomainProfileState?.Revision ?? 0,
            });
        }

        public Simulation행위기록LedgerSnapshot? GetActionManifestationLedger()
        {
            lock (gate)
                return SimulationSaveReplayCloner.CloneActionManifestationLedger(
                    actionManifestationLedgerState);
        }

        public Simulation플레이어분야ProfileSnapshot? GetPlayerDomainProfile()
        {
            lock (gate)
                return SimulationSaveReplayCloner.ClonePlayerDomainProfile(
                    playerDomainProfileState);
        }

        internal void RestoreActionManifestationAndPlayerDomainState(
            Simulation행위기록LedgerSnapshot? ledger,
            Simulation플레이어분야ProfileSnapshot? profile)
        {
            if (ledger == null || profile == null)
                throw new SimulationContractException(
                    "SimulationActionManifestationSaveStateIncomplete");

            // 복원 생성자는 schema, chain, catalog revision과 hash를 함께 검증한다.
            actionManifestationLedgerState = SimulationSaveReplayCloner
                .CloneActionManifestationLedger(
                    Simulation행위발현Ledger.Restore(ledger).Snapshot());
            playerDomainProfileState = SimulationSaveReplayCloner
                .ClonePlayerDomainProfile(
                    Simulation플레이어분야Engine.Restore(profile).Snapshot());
        }

        private static string RequireActionPlayerStableId(
            Simulation행위발현Record record)
        {
            var value = string.IsNullOrWhiteSpace(record.InitiatorStableId)
                ? record.ActorStableId
                : record.InitiatorStableId;
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(
                    "SimulationPlayerDomainPlayerInvalid");
            return value.Trim();
        }

        private static string PlayerProgressNotApplicableReason(
            Simulation행위발현Record record,
            SimulationWI분야결속Definition binding)
        {
            if (record.결과분류Code == Simulation행위결과분류Codes.취소)
                return "CancelledActionHasNoProgress";
            if (!string.Equals(record.TriggerSourceCode,
                    SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    StringComparison.Ordinal))
                return "TriggerSourceIsNotPlayerDriven";
            if (!string.Equals(record.ActorStableId,
                    RequireActionPlayerStableId(record),
                    StringComparison.Ordinal))
                return "ActorIsNotPlayer";
            if (binding.기여방식Code ==
                Simulation분야기여방식Codes.OperationOnly)
                return "RequiresDelegationCompletionReviewLineage";
            if (binding.기여방식Code ==
                Simulation분야기여방식Codes.LearningOnly)
                return "RequiresApprovedLearningEvidence";
            return string.IsNullOrWhiteSpace(binding.NoPlayerProgressReason)
                ? "WorldInteractionHasNoPlayerProgressBinding"
                : binding.NoPlayerProgressReason;
        }
    }
}
