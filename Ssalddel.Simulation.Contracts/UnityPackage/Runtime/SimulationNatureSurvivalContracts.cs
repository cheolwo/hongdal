using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationNatureSurvivalCodes
    {
        public const string ProfileRevision = "nature-survival.realtime.r1";
        public const string AreaSetStableId = "area-set:sim:pyeongchang:nature-home.v1";
        public const string HomeH3StableId = "h3-candidate:nature-home-encounter-defense";
        public const string HomeH2StableId = "h2-candidate:nature-home-core";
        public const string HarvestH2StableId = "h2-candidate:nature-encounter-route";
        public const string SafeClearingH1StableId = "h1-stock:nature-trailhead";
        public const string CabinSiteH1StableId = "h1-stock:nature-shelter";
        public const string AxeItemCode = "tool:axe.basic";
        public const string AxePickupStableId = "pickup:nature-safe-clearing:basic-axe";
        public const string TimberItemCode = "material:timber-log";
        public const string UnitEach = "EA";
        public const string SkeletonPlaceholderCode = "placeholder:synty-generic-skeleton";

        public const string BeginHarvest = "BeginHarvest";
        public const string AcquireAxe = "AcquireAxe";
        public const string PlaceCabinBlueprint = "PlaceCabinBlueprint";
        public const string BeginCabinBuild = "BeginCabinBuild";
        public const string ResolveEncounter = "ResolveEncounter";
        public const string EnterCabin = "EnterCabin";
        public const string LeaveCabin = "LeaveCabin";
        public const string CancelActiveWork = "CancelActiveWork";

        public const string AcquireAxeWorldInteractionId = "WI-NATURE-05";
        public const string BeginHarvestWorldInteractionId = "WI-NATURE-06";
        public const string PlaceCabinBlueprintWorldInteractionId = "WI-NATURE-07";
        public const string BeginCabinBuildWorldInteractionId = "WI-NATURE-08";
        public const string EnterCabinWorldInteractionId = "WI-NATURE-09";
        public const string LeaveCabinWorldInteractionId = "WI-NATURE-10";
        public const string ResolveEncounterWorldInteractionId = "WI-NATURE-11";
        public const string CancelActiveWorkWorldInteractionId = "WI-NATURE-12";
        public const string Fight = "Fight";
        public const string Retreat = "Retreat";
        public const string None = "None";
        public const string Harvest = "Harvest";
        public const string CabinBuild = "CabinBuild";
        public const string Standing = "Standing";
        public const string Stump = "Stump";
        public const string Planned = "Planned";
        public const string Building = "Building";
        public const string Completed = "Completed";
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Menu = "Menu";
        public const string ApplicationInactive = "ApplicationInactive";

        public const string Disabled = "SimulationNatureSurvivalDisabled";
        public const string ExpectedRevisionMismatch = "SimulationExpectedRevisionMismatch";
        public const string ActionBlocked = "SimulationNatureSurvivalActionBlocked";
        public const string ResourceNodeNotFound = "SimulationNatureResourceNodeNotFound";
        public const string ResourceNodeUnavailable = "SimulationNatureResourceNodeUnavailable";
        public const string AxeRequired = "SimulationNatureAxeRequired";
        public const string TimberInsufficient = "SimulationNatureTimberInsufficient";
        public const string CabinBlueprintRequired = "SimulationNatureCabinBlueprintRequired";
        public const string EncounterNotPending = "SimulationNatureEncounterNotPending";
        public const string CommandPayloadConflict = "SimulationCommandPayloadConflict";
        public const string DurationExceeded = "SimulationDurationExceeded";
        public const string ActiveWorkRequired = "SimulationNatureActiveWorkRequired";
        public const string SpatialEvidenceUnavailable =
            "SimulationNatureSpatialEvidenceUnavailable";

        /// <summary>
        /// 플레이어가 명시적으로 선택하고 권위 상태를 바꾸는 Nature 생존 행동을
        /// 정식 WI 식별자로 정규화한다. 시간 경과와 작업 진행은 WI가 아니다.
        /// </summary>
        public static string WorldInteractionIdForAction(string actionCode)
            => actionCode switch
            {
                AcquireAxe => AcquireAxeWorldInteractionId,
                BeginHarvest => BeginHarvestWorldInteractionId,
                PlaceCabinBlueprint => PlaceCabinBlueprintWorldInteractionId,
                BeginCabinBuild => BeginCabinBuildWorldInteractionId,
                EnterCabin => EnterCabinWorldInteractionId,
                LeaveCabin => LeaveCabinWorldInteractionId,
                ResolveEncounter => ResolveEncounterWorldInteractionId,
                CancelActiveWork => CancelActiveWorkWorldInteractionId,
                _ => string.Empty,
            };

        public static string ActualE5SpatialStableId(string worldInteractionId)
            => "spatial:actual-e5:" + (worldInteractionId ?? string.Empty)
                .Trim().ToLowerInvariant();
    }

    public sealed class SimulationNatureSurvivalInitialStateRequest
    {
        public string ProfileRevision { get; set; } = SimulationNatureSurvivalCodes.ProfileRevision;
        public string PlayerStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = SimulationNatureSurvivalCodes.AreaSetStableId;
        public string H3StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH3StableId;
        public string SpawnH2StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH2StableId;
        public string SpawnH1StableId { get; set; } = SimulationNatureSurvivalCodes.SafeClearingH1StableId;
        public decimal InventoryCapacityUnits { get; set; } = 24m;
        public bool StartsWithAxe { get; set; } = true;
        public SimulationNatureResourceNodeInitialStateRequest[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeInitialStateRequest>();
    }

    public sealed class SimulationNatureResourceNodeInitialStateRequest
    {
        public string ResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = SimulationNatureSurvivalCodes.HarvestH2StableId;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
    }

    public sealed class SimulationNatureSurvivalActionPreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
    }

    public sealed class SimulationNatureSurvivalCommandRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
    }

    /// <summary>
    /// 현재 입력 프레임을 결정적 정수 초로 환산한 명령이다. 실제 벽시계 timestamp를
    /// 저장하지 않으므로 종료 중 경과 시간은 따라잡지 않는다.
    /// </summary>
    public sealed class SimulationNatureSurvivalClockAdvanceRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public int ElapsedRealtimeSeconds { get; set; }
        public bool WorkInputHeld { get; set; }
        public string PauseReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureSurvivalActionPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public int RequiredTimberQuantity { get; set; }
        public int AvailableTimberQuantity { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing;
        public string[] SpatialEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class SimulationNatureSurvivalStateSnapshot
    {
        public bool IsEnabled { get; set; }
        public string ProfileRevision { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string H3StableId { get; set; } = string.Empty;
        public string CurrentH2StableId { get; set; } = string.Empty;
        public string CurrentH1StableId { get; set; } = string.Empty;
        public int CycleIndex { get; set; }
        public int ElapsedSecondsInCycle { get; set; }
        public string ClockPhaseCode { get; set; } = string.Empty;
        public bool ClockPaused { get; set; }
        public string PauseReasonCode { get; set; } = string.Empty;
        public bool HasAxe { get; set; }
        public int TimberQuantity { get; set; }
        public int NoiseEventCount { get; set; }
        public bool PlayerInsideCabin { get; set; }
        public SimulationNatureResourceNodeSnapshot[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeSnapshot>();
        public SimulationNatureActiveWorkSnapshot? ActiveWork { get; set; }
        public SimulationNatureCabinSnapshot Cabin { get; set; }
            = new SimulationNatureCabinSnapshot();
        public SimulationNatureEncounterSnapshot? Encounter { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureResourceNodeSnapshot
    {
        public string ResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public string StateCode { get; set; } = SimulationNatureSurvivalCodes.Standing;
        public int RegrowsAtCycleIndex { get; set; } = -1;
    }

    public sealed class SimulationNatureActiveWorkSnapshot
    {
        public string WorkKindCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public int RequiredWorkSeconds { get; set; }
        public int CompletedWorkSeconds { get; set; }
    }

    public sealed class SimulationNatureCabinSnapshot
    {
        public string CabinStableId { get; set; } = "facility:nature-cabin";
        public string H2StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH2StableId;
        public string H1StableId { get; set; } = SimulationNatureSurvivalCodes.CabinSiteH1StableId;
        public string StateCode { get; set; } = SimulationNatureSurvivalCodes.Planned;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
        public int ReservedTimberQuantity { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public int StorageCapacity { get; set; }
        public bool RecoveryAvailable { get; set; }
        public bool DefenseAvailable { get; set; }
    }

    public sealed class SimulationNatureEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ThreatPresentationCode { get; set; }
            = SimulationNatureSurvivalCodes.SkeletonPlaceholderCode;
        public int TriggeredCycleIndex { get; set; }
        public string ResolutionCode { get; set; } = string.Empty;
        public bool CabinDefenseApplied { get; set; }
    }
}
