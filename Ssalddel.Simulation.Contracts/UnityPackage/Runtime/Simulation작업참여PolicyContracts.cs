using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation작업참여PolicyCodes
    {
        public const string RuleRevision = "work-participation-policy.r1";
        public const string MetadataOnly = "MetadataOnly";

        public const string SoloFriendly = "SoloFriendly";
        public const string CollaborationHelpful = "CollaborationHelpful";
        public const string PhysicallyBlocked = "PhysicallyBlocked";

        public const string LargeArea = "LargeArea";
        public const string SteepSlope = "SteepSlope";
        public const string EmbeddedRock = "EmbeddedRock";
        public const string DrainageProblem = "DrainageProblem";
        public const string DistantWaterSource = "DistantWaterSource";

        public const string TimeBurden = "TimeBurden";
        public const string FatigueBurden = "FatigueBurden";
        public const string ToolDurabilityBurden = "ToolDurabilityBurden";
        public const string InjuryRiskBurden = "InjuryRiskBurden";
        public const string CurrentToolCannotPerform =
            "CurrentToolCannotPerform";

        public const string LightAssistance = "LightAssistance";
        public const string StateChangingAssistance =
            "StateChangingAssistance";
        public const string ProfessionalWork = "ProfessionalWork";

        public const string WeedClearing = "WeedClearing";
        public const string DroppedWorkItemTidying =
            "DroppedWorkItemTidying";
        public const string ShortDistanceCarry = "ShortDistanceCarry";
        public const string ConfirmedTaskSupport = "ConfirmedTaskSupport";
        public const string ResourceConsumption = "ResourceConsumption";
        public const string HarvestOrDisposal = "HarvestOrDisposal";
        public const string TerrainMutation = "TerrainMutation";
        public const string ConstructionOrDemolition =
            "ConstructionOrDemolition";
        public const string NewTaskConfirmation = "NewTaskConfirmation";
        public const string SkilledLongDurationWork =
            "SkilledLongDurationWork";

        public const string DefaultAutoAllowed = "DefaultAutoAllowed";
        public const string ExplicitConfirmRequired =
            "ExplicitConfirmRequired";
        public const string PreDelegationOrConfirmRequired =
            "PreDelegationOrConfirmRequired";

        public const string ReciprocityContributionLedger =
            "ReciprocityContributionLedger";
        public const string PreAgreedCompensation =
            "PreAgreedCompensation";
    }

    public sealed class Simulation작업부담평가Request
    {
        public bool IsLargeArea { get; set; }
        public bool CurrentToolCanPerform { get; set; } = true;
        public string[] DifficultyCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation작업부담평가Snapshot
    {
        public string WorkloadCode { get; set; } = string.Empty;
        public bool CanAttemptSolo { get; set; }
        public bool CollaborationRecommended { get; set; }
        public bool ProgressPreservedOnPause { get; set; } = true;
        public string[] ActiveBurdenCodes { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation작업도움권한RuleSnapshot
    {
        public string AssistanceActionCode { get; set; } = string.Empty;
        public string AssistanceClassCode { get; set; } = string.Empty;
        public string DefaultPermissionCode { get; set; } = string.Empty;
        public bool PlayerMayDisableAutoHelp { get; set; }
        public bool RequiresAuthorityCommandRecord { get; set; } = true;
        public bool MayMutatePlayerPlanOrOwnedWorldState { get; set; }
    }

    public sealed class Simulation작업보답RuleSnapshot
    {
        public string AssistanceClassCode { get; set; } = string.Empty;
        public string SettlementCode { get; set; } = string.Empty;
        public bool CompensationAgreementRequiredBeforeWork { get; set; }
        public bool ContributionRecordRequired { get; set; } = true;
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "플레이어·NPC 작업 참여의 Solo 가능 범위, 도움 권한과 호혜·전문 보수 경계를 정의한다.",
        Boundary = "비실행 정책 계약이며 WI Confirm, 작업 배정, 관계 수치, Save 또는 Unity 표현을 변경하지 않는다.")]
    public sealed class Simulation작업참여PolicyCatalogSnapshot
    {
        public string RuleRevision { get; set; }
            = Simulation작업참여PolicyCodes.RuleRevision;
        public string ExecutionModeCode { get; set; }
            = Simulation작업참여PolicyCodes.MetadataOnly;
        public bool IsExecutable { get; set; }
        public bool OwnsPreviewConfirmTaskEffect { get; set; }
        public Simulation작업도움권한RuleSnapshot[] AssistanceRules
        {
            get;
            set;
        } = Array.Empty<Simulation작업도움권한RuleSnapshot>();
        public Simulation작업보답RuleSnapshot[] CompensationRules
        {
            get;
            set;
        } = Array.Empty<Simulation작업보답RuleSnapshot>();
        public string[] ReusedSystemRefs { get; set; } = Array.Empty<string>();
        public string CatalogHashSha256 { get; set; } = string.Empty;
    }
}
