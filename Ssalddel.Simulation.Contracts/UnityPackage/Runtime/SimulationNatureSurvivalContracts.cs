using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationNatureSurvivalCodes
    {
        public const string ProfileRevisionR1 = "nature-survival.realtime.r1";
        public const string ProfileRevisionR2 = "nature-survival.realtime.r2";
        public const string ProfileRevisionR3 = "nature-survival.realtime.r3";
        public const string ProfileRevisionR4 = "nature-survival.realtime.r4";
        public const string ProfileRevisionR5 = "nature-survival.realtime.r5";
        public const string ProfileRevision = ProfileRevisionR1;
        public const string AreaSetStableId = "area-set:sim:pyeongchang:nature-home.v1";
        public const string HomeH3StableId = "h3-candidate:nature-home-encounter-defense";
        public const string HomeH2StableId = "h2-candidate:nature-home-core";
        public const string HarvestH2StableId = "h2-candidate:nature-encounter-route";
        public const string SafeClearingH1StableId = "h1-stock:nature-trailhead";
        public const string CabinSiteH1StableId = "h1-stock:nature-shelter";
        public const string AxeItemCode = "tool:axe.basic";
        public const string AxePickupStableId = "pickup:nature-safe-clearing:basic-axe";
        public const string TimberItemCode = "material:timber-log";
        public const string RebuildPartItemCode = "material:rebuild-part";
        public const string NatureFieldSupplyPackItemCode =
            "supply:nature-field-pack";
        public const string CabinStorageContainerStableId = "container:nature-cabin:storage";
        public const string CabinStorageTimberStackStableId =
            "item-stack:nature-cabin:timber";
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
        public const string StoreAtCabin = "StoreAtCabin";
        public const string SleepInCabin = "SleepInCabin";
        public const string SelectExpansionPlan = "SelectExpansionPlan";
        public const string BeginBuildingConstruction =
            Simulation영역건물발전Codes.BeginBuildingConstruction;
        public const string PrepareFieldSupply = "PrepareFieldSupply";
        public const string PrepareFieldSupplyDelegated =
            "PrepareFieldSupplyDelegated";
        public const string CollectDroppedTimber = "CollectDroppedTimber";

        public const string AcquireAxeWorldInteractionId = "WI-NATURE-05";
        public const string BeginHarvestWorldInteractionId = "WI-NATURE-06";
        public const string PlaceCabinBlueprintWorldInteractionId = "WI-NATURE-07";
        public const string BeginCabinBuildWorldInteractionId = "WI-NATURE-08";
        public const string EnterCabinWorldInteractionId = "WI-NATURE-09";
        public const string LeaveCabinWorldInteractionId = "WI-NATURE-10";
        public const string ResolveEncounterWorldInteractionId = "WI-NATURE-11";
        public const string CancelActiveWorkWorldInteractionId = "WI-NATURE-12";
        public const string StoreAtCabinWorldInteractionId = "WI-NATURE-13";
        public const string SleepInCabinWorldInteractionId = "WI-NATURE-14";
        public const string SelectExpansionPlanWorldInteractionId = "WI-NATURE-15";
        public const string PrepareFieldSupplyWorldInteractionId = "WI-NATURE-16";
        public const string PrepareFieldSupplyDelegatedWorldInteractionId =
            "WI-NATURE-17";
        public const string CollectDroppedTimberWorldInteractionId =
            "WI-NATURE-18";
        public const string Fight = "Fight";
        public const string Retreat = "Retreat";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";
        public const string CombatActive = "CombatActive";
        public const string Sleeping = "Sleeping";
        public const string Workbench = "Workbench";
        public const string StorageRack = "StorageRack";
        public const string Palisade = "Palisade";
        public const string None = "None";
        public const string Harvest = "Harvest";
        public const string CabinBuild = "CabinBuild";
        public const string FieldSupplyCraft = "FieldSupplyCraft";
        public const string FieldSupplyNpcCraft = "FieldSupplyNpcCraft";
        public const string UseFieldSupplyPack = "UseFieldSupplyPack";
        public const int FieldSupplyTimberCost = 2;
        public const int FieldSupplyRebuildPartCost = 1;
        public const int FieldSupplyCraftSeconds = 4;
        public const string Standing = "Standing";
        public const string Stump = "Stump";
        public const string Planned = "Planned";
        public const string Building = "Building";
        public const string Completed = "Completed";
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Menu = "Menu";
        public const string ApplicationInactive = "ApplicationInactive";
        public const string DroppedTimberAvailable = "Available";
        public const string DroppedTimberCollected = "Collected";

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
        public const string CabinRequired = "SimulationNatureCabinRequired";
        public const string CabinAccessRequired = "SimulationNatureCabinAccessRequired";
        public const string CabinStorageFull = "SimulationNatureCabinStorageFull";
        public const string TimberNotCarried = "SimulationNatureTimberNotCarried";
        public const string NightRequired = "SimulationNatureNightRequired";
        public const string CombatActiveClockFrozen =
            "SimulationNatureCombatActiveClockFrozen";
        public const string ExpansionPlanInvalid = "SimulationNatureExpansionPlanInvalid";
        public const string ExpansionPlanAlreadySelected =
            "SimulationNatureExpansionPlanAlreadySelected";
        public const string SpatialEvidenceUnavailable =
            "SimulationNatureSpatialEvidenceUnavailable";
        public const string WorkbenchRequired =
            "SimulationNatureWorkbenchRequired";
        public const string FieldSupplyTimberInsufficient =
            "SimulationNatureFieldSupplyTimberInsufficient";
        public const string FieldSupplyRebuildPartInsufficient =
            "SimulationNatureFieldSupplyRebuildPartInsufficient";
        public const string FieldSupplyPackRequired =
            "SimulationNatureFieldSupplyPackRequired";
        public const string ExpeditionAlreadyPrepared =
            "SimulationNatureExpeditionAlreadyPrepared";
        public const string NpcRoutineNatureRevisionRequired =
            "SimulationNpcRoutineNatureRevisionRequired";
        public const string FieldSupplyAlreadyAvailable =
            "SimulationNatureFieldSupplyAlreadyAvailable";
        public const string DroppedTimberNotFound =
            "SimulationNatureDroppedTimberNotFound";
        public const string DroppedTimberUnavailable =
            "SimulationNatureDroppedTimberUnavailable";

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
                StoreAtCabin => StoreAtCabinWorldInteractionId,
                SleepInCabin => SleepInCabinWorldInteractionId,
                SelectExpansionPlan => SelectExpansionPlanWorldInteractionId,
                BeginBuildingConstruction =>
                    Simulation영역건물발전Codes.ConstructionWorldInteractionId,
                PrepareFieldSupply => PrepareFieldSupplyWorldInteractionId,
                PrepareFieldSupplyDelegated =>
                    PrepareFieldSupplyDelegatedWorldInteractionId,
                CollectDroppedTimber => CollectDroppedTimberWorldInteractionId,
                _ => string.Empty,
            };

        public static string PlayerActivityTrackCodeForAction(string actionCode)
            => actionCode switch
            {
                AcquireAxe or BeginHarvest or ResolveEncounter or
                    CollectDroppedTimber =>
                    Simulation플레이어활동경로Codes.FieldExpedition,
                StoreAtCabin or SleepInCabin or SelectExpansionPlan =>
                    Simulation플레이어활동경로Codes.AreaOperation,
                PlaceCabinBlueprint or BeginCabinBuild or
                    BeginBuildingConstruction or PrepareFieldSupply =>
                    Simulation플레이어활동경로Codes.AreaManufacturing,
                PrepareFieldSupplyDelegated =>
                    Simulation플레이어활동경로Codes.AreaOperation,
                _ => Simulation플레이어활동경로Codes.AreaOperation,
            };

        public static string PlayerFlowCodeForAction(string actionCode)
            => actionCode switch
            {
                BeginHarvest or ResolveEncounter =>
                    Simulation플레이흐름Codes.발산,
                PlaceCabinBlueprint or BeginCabinBuild or
                    BeginBuildingConstruction or PrepareFieldSupply =>
                    Simulation플레이흐름Codes.순환연결부,
                _ => Simulation플레이흐름Codes.수렴,
            };

        public static string NextPlayerFlowCodeForAction(string actionCode)
            => PlayerFlowCodeForAction(actionCode) ==
                Simulation플레이흐름Codes.발산
                ? Simulation플레이흐름Codes.수렴
                : Simulation플레이흐름Codes.발산;

        public static string CycleHandoffCodeForAction(string actionCode)
            => PlayerFlowCodeForAction(actionCode) ==
                Simulation플레이흐름Codes.발산
                ? Simulation플레이흐름인계Codes.발산에서수렴
                : Simulation플레이흐름인계Codes.수렴에서발산;

        public static bool IsR2(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR2,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR3,
                    StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                    StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                    StringComparison.Ordinal);

        public static bool IsR3(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR3,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                   StringComparison.Ordinal);

        public static bool IsR4(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                   StringComparison.Ordinal);

        public static bool IsR5(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                StringComparison.Ordinal);

        public static string ActualE5SpatialStableId(string worldInteractionId)
            => "spatial:actual-e5:" + (worldInteractionId ?? string.Empty)
                .Trim().ToLowerInvariant();
    }

    public static class Simulation플레이어활동경로Codes
    {
        public const string FieldExpedition = "FieldExpedition";
        public const string AreaOperation = "AreaOperation";
        public const string AreaManufacturing = "AreaManufacturing";
    }

    public static class Simulation플레이흐름Codes
    {
        public const string 발산 = "Outward";
        public const string 수렴 = "Inward";
        public const string 순환연결부 = "TransformationBridge";
    }

    public static class Simulation플레이흐름인계Codes
    {
        public const string 발산에서수렴 = "OutwardToInward";
        public const string 수렴에서발산 = "InwardToOutward";
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
        public string FocusAccessibilityModeCode { get; set; }
            = Simulation집중판정Codes.Standard;
        public SimulationNatureResourceNodeInitialStateRequest[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeInitialStateRequest>();
        public Simulation영역건물발전CatalogSnapshot? BuildingProgressionCatalog
            { get; set; }
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
        /// <summary>
        /// 전투 권위가 직접 개입 성과를 판정한 뒤 내부 인계에만 사용하는 보상량이다.
        /// 일반 Unity/API 명령에서는 항상 0이어야 한다.
        /// </summary>
        public int AuthoritativeRewardBonusQuantity { get; set; }
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
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string PlayerActivityTrackCode { get; set; } = string.Empty;
        public string PlayerFlowCode { get; set; } = string.Empty;
        public string NextPlayerFlowCode { get; set; } = string.Empty;
        public string CycleHandoffCode { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public int RequiredTimberQuantity { get; set; }
        public int AvailableTimberQuantity { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public int TransferableTimberQuantity { get; set; }
        public int CabinStoredTimberQuantity { get; set; }
        public int CabinStorageCapacity { get; set; }
        public int RequiredRebuildPartQuantity { get; set; }
        public int AvailableRebuildPartQuantity { get; set; }
        public int TargetDroppedTimberQuantity { get; set; }
        public decimal RemainingInventoryCapacityUnits { get; set; }
        public string BuildingBlueprintStableId { get; set; } = string.Empty;
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
        public int StoredTimberQuantity { get; set; }
        public int NoiseEventCount { get; set; }
        public int RawThreatTier { get; set; }
        public int EffectiveThreatTier { get; set; }
        public int RebuildPartQuantity { get; set; }
        public int FieldSupplyPackQuantity { get; set; }
        public bool ExpeditionPrepared { get; set; }
        public string LastProtectedMaterialItemCode { get; set; } = string.Empty;
        public string LinkedCombatStableId { get; set; } = string.Empty;
        public string LastCombatResultCode { get; set; } = string.Empty;
        public bool Sleeping { get; set; }
        public string SelectedExpansionPlanCode { get; set; } = string.Empty;
        public bool Day2Ready { get; set; }
        public Simulation영역건물발전Snapshot? BuildingProgression { get; set; }
        public Simulation학습방문Snapshot? LearningVisit { get; set; }
        public bool PlayerInsideCabin { get; set; }
        public SimulationNatureResourceNodeSnapshot[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeSnapshot>();
        public SimulationNatureDroppedTimberSnapshot[] DroppedTimber { get; set; }
            = Array.Empty<SimulationNatureDroppedTimberSnapshot>();
        public SimulationNatureActiveWorkSnapshot? ActiveWork { get; set; }
        public Simulation집중판정ChallengeSnapshot? ActiveFocusChallenge { get; set; }
        public Simulation집중판정ResultSnapshot? LastFocusResult { get; set; }
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

    /// <summary>
    /// 벌목 결과가 월드에 남긴 결정적 통나무 묶음이다. Unity는 이 상태 사본을
    /// 표현할 뿐 생성 수량이나 획득 가능 여부를 계산하지 않는다.
    /// </summary>
    public sealed class SimulationNatureDroppedTimberSnapshot
    {
        public string DroppedTimberStableId { get; set; } = string.Empty;
        public string SourceResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public int Quantity { get; set; }
        public string UnitCode { get; set; } = SimulationNatureSurvivalCodes.UnitEach;
        public string StateCode { get; set; }
            = SimulationNatureSurvivalCodes.DroppedTimberAvailable;
        public long CreatedWorldRevision { get; set; }
        public long CollectedWorldRevision { get; set; }
    }

    public sealed class SimulationNatureActiveWorkSnapshot
    {
        public string OriginCommandId { get; set; } = string.Empty;
        public string WorkKindCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public int RequiredWorkSeconds { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int ReservedTimberQuantity { get; set; }
        public int ReservedRebuildPartQuantity { get; set; }
    }

    public sealed class Simulation플레이어기회Snapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string PlayerActivityTrackCode { get; set; } = string.Empty;
        public string PlayerFlowCode { get; set; } = string.Empty;
        public string NextPlayerFlowCode { get; set; } = string.Empty;
        public string CycleHandoffCode { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public bool Available { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation영역수요Snapshot
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string NeedCode { get; set; } = string.Empty;
        public string RequiredItemCode { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool Satisfied { get; set; }
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
        public int RawThreatTier { get; set; }
        public int EffectiveThreatTier { get; set; }
        public int HostileCount { get; set; }
        public string LinkedCombatStableId { get; set; } = string.Empty;
    }
}
