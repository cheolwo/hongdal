using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationHexagramCampaignCodes
    {
        public const string SchemaVersion = "simulation-hexagram-campaign.v1";
        public const string RuleRevision = "hexagram-campaign-reset.r1";
        public const string RetryWorldInteractionId =
            "WI-STORY-HEXAGRAM-CAMPAIGN-RETRY";
        public const string ZhunStableId = "HEX-03-ZHUN";
        public const string Active = "Active";
        public const string FreeRoam = "FreeRoam";
        public const string Completed = "Completed";
        public const string RecoverableSetback = "RecoverableSetback";
        public const string CampaignFailure = "CampaignFailure";
        public const string HansLost = "HansLost";
        public const string HansFarmFullyLost = "HansFarmFullyLost";
        public const string Injury = "Injury";
        public const string Delay = "Delay";
        public const string PartialFacilityDamage = "PartialFacilityDamage";
        public const string ResourceLoss = "ResourceLoss";
    }

    public sealed class SimulationHexagramCampaignEnterRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string HexagramStableId { get; set; } = string.Empty;
        // 이야기에서 정한 단계 수. 누락된 기존 요청은 종전 6단계를 유지한다.
        public int StoryStageCount { get; set; } = 6;
        public string[] LineWorldInteractionIds { get; set; } =
            Array.Empty<string>();
    }

    public sealed class SimulationHexagramCampaignLineCompleteRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public int ExpectedLineOrdinal { get; set; }
    }

    public sealed class SimulationHexagramCampaignSetbackRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string SetbackReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationHexagramCampaignFailureRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string FailureReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationHexagramCampaignCompleteRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
    }

    public sealed class SimulationHexagramCampaignEventSnapshot
    {
        public string EventCode { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public int AttemptOrdinal { get; set; }
        public int LineOrdinal { get; set; }
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
    }

    public sealed class SimulationHexagramCampaignStateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationHexagramCampaignCodes.SchemaVersion;
        public string RuleRevision { get; set; } =
            SimulationHexagramCampaignCodes.RuleRevision;
        public string CampaignStateCode { get; set; } = string.Empty;
        public string HexagramStableId { get; set; } = string.Empty;
        public int CurrentLineOrdinal { get; set; }
        // CurrentLineOrdinal은 호환 이름이며 효가 아닌 이야기 단계로 해석한다.
        public int StoryStageCount { get; set; } = 6;
        public int AttemptOrdinal { get; set; }
        public int AttemptVariationSeed { get; set; }
        public string EntrySaveStableId { get; set; } = string.Empty;
        public long EntryWorldRevision { get; set; }
        public string[] TemporaryWorldInteractionIds { get; set; } =
            Array.Empty<string>();
        public string[] PermanentlyUnlockedWorldInteractionIds { get; set; } =
            Array.Empty<string>();
        public SimulationHexagramCampaignEventSnapshot[] Events { get; set; } =
            Array.Empty<SimulationHexagramCampaignEventSnapshot>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
