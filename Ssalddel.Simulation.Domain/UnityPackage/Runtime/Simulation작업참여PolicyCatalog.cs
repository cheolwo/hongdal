using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "작업 면적·현장 난도·도구 가능성과 도움 행위 종류를 공통 작업 참여 정책으로 결정적으로 판정한다.",
        Boundary = "판정 결과만 반환하며 Farm 작업, NPC 이동, 기여 원장 또는 WorldRevision을 직접 변경하지 않는다.")]
    public static class Simulation작업참여PolicyCatalog
    {
        private static readonly string[] CollaborationBurdenCodes =
        {
            Simulation작업참여PolicyCodes.TimeBurden,
            Simulation작업참여PolicyCodes.FatigueBurden,
            Simulation작업참여PolicyCodes.ToolDurabilityBurden,
            Simulation작업참여PolicyCodes.InjuryRiskBurden,
        };

        public static Simulation작업참여PolicyCatalogSnapshot Create()
        {
            var assistanceRules = new[]
            {
                Light(Simulation작업참여PolicyCodes.WeedClearing),
                Light(Simulation작업참여PolicyCodes.DroppedWorkItemTidying),
                Light(Simulation작업참여PolicyCodes.ShortDistanceCarry),
                Light(Simulation작업참여PolicyCodes.ConfirmedTaskSupport),
                StateChanging(Simulation작업참여PolicyCodes.ResourceConsumption),
                StateChanging(Simulation작업참여PolicyCodes.HarvestOrDisposal),
                StateChanging(Simulation작업참여PolicyCodes.TerrainMutation),
                StateChanging(
                    Simulation작업참여PolicyCodes.ConstructionOrDemolition),
                StateChanging(
                    Simulation작업참여PolicyCodes.NewTaskConfirmation),
                Professional(
                    Simulation작업참여PolicyCodes.SkilledLongDurationWork),
            }.OrderBy(value => value.AssistanceActionCode,
                StringComparer.Ordinal).ToArray();
            var compensationRules = new[]
            {
                new Simulation작업보답RuleSnapshot
                {
                    AssistanceClassCode =
                        Simulation작업참여PolicyCodes.LightAssistance,
                    SettlementCode = Simulation작업참여PolicyCodes
                        .ReciprocityContributionLedger,
                    CompensationAgreementRequiredBeforeWork = false,
                },
                new Simulation작업보답RuleSnapshot
                {
                    AssistanceClassCode =
                        Simulation작업참여PolicyCodes.ProfessionalWork,
                    SettlementCode = Simulation작업참여PolicyCodes
                        .PreAgreedCompensation,
                    CompensationAgreementRequiredBeforeWork = true,
                },
            }.OrderBy(value => value.AssistanceClassCode,
                StringComparer.Ordinal).ToArray();
            var reusedSystemRefs = new[]
            {
                nameof(SimulationCoopContributionSnapshot),
                nameof(SimulationFarmWorkPreviewSnapshot),
                nameof(SimulationNpcTaskAssignmentSnapshot),
                nameof(SimulationNpcWorkRecordSnapshot),
            }.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var snapshot = new Simulation작업참여PolicyCatalogSnapshot
            {
                AssistanceRules = assistanceRules,
                CompensationRules = compensationRules,
                ReusedSystemRefs = reusedSystemRefs,
            };
            snapshot.CatalogHashSha256 = Hash(snapshot);
            return snapshot;
        }

        public static Simulation작업부담평가Snapshot AssessWorkload(
            Simulation작업부담평가Request request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "WorkParticipationAssessmentRequestRequired");
            var difficulties = (request.DifficultyCodes
                    ?? Array.Empty<string>())
                .Select(value => RequireDifficulty(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!request.CurrentToolCanPerform)
                return new Simulation작업부담평가Snapshot
                {
                    WorkloadCode =
                        Simulation작업참여PolicyCodes.PhysicallyBlocked,
                    CanAttemptSolo = false,
                    CollaborationRecommended = true,
                    BlockReasonCodes = new[]
                    {
                        Simulation작업참여PolicyCodes
                            .CurrentToolCannotPerform,
                    },
                };
            var collaborationHelpful = request.IsLargeArea
                || difficulties.Length > 0;
            return new Simulation작업부담평가Snapshot
            {
                WorkloadCode = collaborationHelpful
                    ? Simulation작업참여PolicyCodes.CollaborationHelpful
                    : Simulation작업참여PolicyCodes.SoloFriendly,
                CanAttemptSolo = true,
                CollaborationRecommended = collaborationHelpful,
                ActiveBurdenCodes = collaborationHelpful
                    ? CollaborationBurdenCodes.ToArray()
                    : Array.Empty<string>(),
            };
        }

        public static Simulation작업도움권한RuleSnapshot ResolveAssistance(
            string assistanceActionCode)
        {
            var actionCode = Require(assistanceActionCode,
                "WorkParticipationAssistanceActionCodeInvalid");
            var rule = Create().AssistanceRules.SingleOrDefault(value =>
                string.Equals(value.AssistanceActionCode, actionCode,
                    StringComparison.Ordinal));
            return rule ?? throw new SimulationContractException(
                "WorkParticipationAssistanceActionUnknown");
        }

        public static Simulation작업보답RuleSnapshot ResolveCompensation(
            string assistanceClassCode)
        {
            var classCode = Require(assistanceClassCode,
                "WorkParticipationAssistanceClassCodeInvalid");
            var rule = Create().CompensationRules.SingleOrDefault(value =>
                string.Equals(value.AssistanceClassCode, classCode,
                    StringComparison.Ordinal));
            return rule ?? throw new SimulationContractException(
                "WorkParticipationAssistanceClassUnknown");
        }

        private static Simulation작업도움권한RuleSnapshot Light(
            string actionCode) => new Simulation작업도움권한RuleSnapshot
        {
            AssistanceActionCode = actionCode,
            AssistanceClassCode =
                Simulation작업참여PolicyCodes.LightAssistance,
            DefaultPermissionCode =
                Simulation작업참여PolicyCodes.DefaultAutoAllowed,
            PlayerMayDisableAutoHelp = true,
        };

        private static Simulation작업도움권한RuleSnapshot StateChanging(
            string actionCode) => new Simulation작업도움권한RuleSnapshot
        {
            AssistanceActionCode = actionCode,
            AssistanceClassCode =
                Simulation작업참여PolicyCodes.StateChangingAssistance,
            DefaultPermissionCode =
                Simulation작업참여PolicyCodes.ExplicitConfirmRequired,
            PlayerMayDisableAutoHelp = true,
            MayMutatePlayerPlanOrOwnedWorldState = true,
        };

        private static Simulation작업도움권한RuleSnapshot Professional(
            string actionCode) => new Simulation작업도움권한RuleSnapshot
        {
            AssistanceActionCode = actionCode,
            AssistanceClassCode =
                Simulation작업참여PolicyCodes.ProfessionalWork,
            DefaultPermissionCode = Simulation작업참여PolicyCodes
                .PreDelegationOrConfirmRequired,
            PlayerMayDisableAutoHelp = true,
            MayMutatePlayerPlanOrOwnedWorldState = true,
        };

        private static string RequireDifficulty(string value)
        {
            var code = Require(value,
                "WorkParticipationDifficultyCodeInvalid");
            return code switch
            {
                Simulation작업참여PolicyCodes.SteepSlope => code,
                Simulation작업참여PolicyCodes.EmbeddedRock => code,
                Simulation작업참여PolicyCodes.DrainageProblem => code,
                Simulation작업참여PolicyCodes.DistantWaterSource => code,
                _ => throw new SimulationContractException(
                    "WorkParticipationDifficultyCodeUnknown"),
            };
        }

        private static string Require(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
            return value.Trim();
        }

        private static string Hash(
            Simulation작업참여PolicyCatalogSnapshot snapshot)
        {
            var canonical = string.Join("\n", new[]
            {
                snapshot.RuleRevision,
                snapshot.ExecutionModeCode,
                string.Join(";", snapshot.AssistanceRules.Select(value =>
                    string.Join("|", value.AssistanceActionCode,
                        value.AssistanceClassCode,
                        value.DefaultPermissionCode,
                        value.PlayerMayDisableAutoHelp,
                        value.RequiresAuthorityCommandRecord,
                        value.MayMutatePlayerPlanOrOwnedWorldState))),
                string.Join(";", snapshot.CompensationRules.Select(value =>
                    string.Join("|", value.AssistanceClassCode,
                        value.SettlementCode,
                        value.CompensationAgreementRequiredBeforeWork,
                        value.ContributionRecordRequired))),
                string.Join(";", snapshot.ReusedSystemRefs),
            });
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
