using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private Simulation행위기록LedgerSnapshot? actionManifestationLedgerState;
        private Simulation플레이어분야ProfileSnapshot? playerDomainProfileState;
        private readonly System.Collections.Generic.Dictionary<string,
            Simulation플레이어분야ProfileSnapshot> playerDomainProfileStates =
            new System.Collections.Generic.Dictionary<string,
                Simulation플레이어분야ProfileSnapshot>(StringComparer.Ordinal);

        internal (Simulation행위발현Record 기록,
            Simulation분야성장적용Snapshot 성장적용,
            Simulation명상성장적용Snapshot 명상성장적용, bool 재사용)
            AppendActionManifestationAndProgression(
                Simulation행위발현Record draft,
                Simulation집중판정ResultSnapshot? focusResult = null)
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
            var meditationFamilyBinding =
                Simulation기본명상WiFamilyCatalog.Resolve(
                    draft.WorldInteractionId);
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
                                    Simulation행위결과분류Codes.취소
                                && !(string.Equals(draft.WorldInteractionId,
                                        SimulationNatureSurvivalCodes
                                            .BeginHarvestWorldInteractionId,
                                        StringComparison.Ordinal)
                                    && draft.PrimaryOutcomeCode.EndsWith(
                                        ":TaskStarted",
                                        StringComparison.Ordinal));
            if (canApplyField)
                draft.변화의미Codes = (draft.변화의미Codes
                        ?? Array.Empty<string>())
                    .Concat(new[]
                    {
                        Simulation행위변화의미Codes.플레이어진척변경,
                    }).Distinct(StringComparer.Ordinal).ToArray();

            var canApplyMeditation = focusResult != null
                && focusResult.명상경험증가Milli > 0
                && meditationFamilyBinding.결속상태Code ==
                    Simulation명상WiFamilyCodes.Bound
                && string.Equals(draft.TriggerSourceCode,
                    SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    StringComparison.Ordinal)
                && string.Equals(draft.ActorStableId, playerStableId,
                    StringComparison.Ordinal)
                && draft.결과분류Code != Simulation행위결과분류Codes.취소;
            if (canApplyMeditation)
            {
                var actionRecordStableId = Simulation행위발현Ledger
                    .CalculateRecordStableId(draft);
                var contributionStableId =
                    Simulation플레이어분야Engine
                        .CalculateMeditationContributionStableId(
                            focusResult!.ChallengeStableId,
                            actionRecordStableId, focusResult.RuleRevision);
                draft.변화의미Codes = (draft.변화의미Codes
                        ?? Array.Empty<string>())
                    .Concat(new[]
                    {
                        Simulation행위변화의미Codes.플레이어명상변경,
                        Simulation행위변화의미Codes.플레이어회복변경,
                    }).Distinct(StringComparer.Ordinal).ToArray();
                draft.SourceReferenceIds = (draft.SourceReferenceIds
                        ?? Array.Empty<string>())
                    .Concat(new[]
                    {
                        focusResult.ChallengeStableId,
                        focusResult.SourceStableId,
                        contributionStableId,
                    }).Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal).ToArray();
            }

            var ledger = actionManifestationLedgerState == null
                ? new Simulation행위발현Ledger(draft.WorldStableId)
                : Simulation행위발현Ledger.Restore(
                    actionManifestationLedgerState);
            var beforeCount = ledger.Snapshot().TailRecords.Length;
            var record = ledger.Append(draft);
            var reused = ledger.Snapshot().TailRecords.Length == beforeCount;

            var existingProfile = playerDomainProfileStates.TryGetValue(
                    playerStableId, out var actorProfile)
                ? actorProfile
                : playerDomainProfileState != null
                  && string.Equals(playerDomainProfileState.PlayerStableId,
                      playerStableId, StringComparison.Ordinal)
                    ? playerDomainProfileState
                    : null;
            var playerDomain = existingProfile == null
                ? new Simulation플레이어분야Engine(playerStableId, catalog)
                : Simulation플레이어분야Engine.Restore(
                    existingProfile, catalog);
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

            var meditationBeforeRevision = playerDomain.Snapshot().Revision;
            var meditation = new Simulation명상성장적용Snapshot
            {
                PlayerStableId = playerStableId,
                BeforeProfileRevision = meditationBeforeRevision,
                AfterProfileRevision = meditationBeforeRevision,
            };
            if (reused)
            {
                meditation.상태Code = Simulation집중판정Codes.Reused;
                meditation.사유Code = "IdempotentCommandReused";
            }
            else if (focusResult == null)
            {
                meditation.상태Code = Simulation집중판정Codes.NotApplicable;
                meditation.사유Code = "FocusEvidenceNotProvided";
            }
            else if (focusResult.명상경험증가Milli <= 0)
            {
                meditation.상태Code = Simulation집중판정Codes.NotApplicable;
                meditation.사유Code = "FocusResultHasNoReward";
            }
            else if (!canApplyMeditation)
            {
                meditation.상태Code = Simulation집중판정Codes.NotApplicable;
                meditation.사유Code = meditationFamilyBinding.결속상태Code !=
                    Simulation명상WiFamilyCodes.Bound
                    ? "MeditationWiFamilyNotBound:" +
                      meditationFamilyBinding.사유Code
                    : "FocusActionNotPlayerDriven";
            }
            else
            {
                focusResult.SourceActionRecordStableId = record.행위기록StableId;
                focusResult.AppliedWorldRevision = record.AfterWorldRevision;
                playerDomain.ApplyMeditation(new Simulation명상숙련기여Request
                {
                    PlayerStableId = playerStableId,
                    행위기록 = record,
                    집중판정결과 = focusResult,
                });
                var contribution = playerDomain.Snapshot().명상기여기록들
                    .Single(value => string.Equals(value.ChallengeStableId,
                        focusResult.ChallengeStableId,
                        StringComparison.Ordinal)
                        && string.Equals(value.SourceActionRecordStableId,
                            record.행위기록StableId,
                            StringComparison.Ordinal));
                meditation.상태Code = Simulation집중판정Codes.Applied;
                meditation.ContributionStableId = contribution.ContributionStableId;
                meditation.명상경험증가Milli = focusResult.명상경험증가Milli;
                meditation.회복증가Milli = focusResult.회복증가Milli;
                meditation.AfterProfileRevision = playerDomain.Snapshot().Revision;
                ApplyNatureMindImpactForPlayer(playerStableId,
                    "mind-impact:focus:" + focusResult.ChallengeStableId +
                    ":recovery",
                    SimulationNatureMindCodes.FocusTimingCompleted,
                    focusResult.SourceStableId,
                    SimulationNatureMindCodes.RecoveryAxis,
                    focusResult.회복증가Milli /
                    (decimal)Simulation집중판정Codes.MilliPerPoint,
                    record.AppliedWorldTick);
            }

            actionManifestationLedgerState = ledger.Snapshot();
            var updatedProfile = playerDomain.Snapshot();
            playerDomainProfileStates[playerStableId] = updatedProfile;
            if (playerDomainProfileState == null
                || string.Equals(playerDomainProfileState.PlayerStableId,
                    playerStableId, StringComparison.Ordinal))
                playerDomainProfileState = updatedProfile;
            return (record, new Simulation분야성장적용Snapshot
            {
                상태Code = status,
                사유Code = reason,
                PlayerStableId = playerStableId,
                BeforeProfileRevision = beforeProfileRevision,
                AfterProfileRevision = updatedProfile.Revision,
            }, meditation, reused);
        }

        internal (Simulation행위발현Record? 기록,
            Simulation분야성장적용Snapshot 성장적용,
            Simulation명상성장적용Snapshot 명상성장적용)
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
            }, new Simulation명상성장적용Snapshot
            {
                상태Code = record == null
                    ? Simulation집중판정Codes.NotApplicable
                    : Simulation집중판정Codes.Reused,
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

        public Simulation행위기록Page QueryActionManifestations(
            Simulation행위기록Query query)
        {
            lock (gate)
            {
                if (actionManifestationLedgerState == null)
                    return new Simulation행위기록Page
                    {
                        NextCursor = query.Cursor
                            ?? new Simulation행위기록Cursor(),
                    };
                return Simulation행위발현Ledger.Restore(
                    actionManifestationLedgerState).Query(query);
            }
        }

        public Simulation플레이어분야ProfileSnapshot? GetPlayerDomainProfile()
        {
            lock (gate)
                return SimulationSaveReplayCloner.ClonePlayerDomainProfile(
                    playerDomainProfileState);
        }

        public Simulation플레이어분야ProfileSnapshot? GetPlayerDomainProfile(
            string playerStableId)
        {
            lock (gate)
            {
                if (string.IsNullOrWhiteSpace(playerStableId)) return null;
                if (playerDomainProfileStates.TryGetValue(
                        playerStableId.Trim(), out var profile))
                    return SimulationSaveReplayCloner.ClonePlayerDomainProfile(
                        profile);
                return playerDomainProfileState != null
                    && string.Equals(playerDomainProfileState.PlayerStableId,
                        playerStableId.Trim(), StringComparison.Ordinal)
                    ? SimulationSaveReplayCloner.ClonePlayerDomainProfile(
                        playerDomainProfileState)
                    : null;
            }
        }

        internal Simulation플레이어분야ProfileSnapshot[]
            GetPlayerDomainProfiles()
        {
            var profiles = playerDomainProfileStates.Values.ToList();
            if (playerDomainProfileState != null
                && profiles.All(value => !string.Equals(value.PlayerStableId,
                    playerDomainProfileState.PlayerStableId,
                    StringComparison.Ordinal)))
                profiles.Add(playerDomainProfileState);
            return profiles.OrderBy(value => value.PlayerStableId,
                    StringComparer.Ordinal)
                .Select(value => SimulationSaveReplayCloner
                    .ClonePlayerDomainProfile(value)!).ToArray();
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
            playerDomainProfileStates[playerDomainProfileState!.PlayerStableId]
                = playerDomainProfileState;
        }

        internal void RestoreAdditionalPlayerDomainProfiles(
            Simulation플레이어분야ProfileSnapshot[]? profiles)
        {
            foreach (var profile in profiles
                ?? Array.Empty<Simulation플레이어분야ProfileSnapshot>())
            {
                var restored = Simulation플레이어분야Engine.Restore(profile)
                    .Snapshot();
                playerDomainProfileStates[restored.PlayerStableId] = restored;
            }
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
