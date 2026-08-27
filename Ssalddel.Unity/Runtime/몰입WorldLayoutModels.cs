using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.ImmersiveWorld
{
    public static class 몰입WorldInstanceCodes
    {
        public const string NatureHome = "immersive-instance:nature-home";
        public const string Farm = "immersive-instance:farm";
        public const string Town = "immersive-instance:town";
        public const string CityHub = "immersive-instance:city-hub";

        public static IReadOnlyList<string> All { get; } = new[]
        {
            NatureHome, Farm, Town, CityHub,
        };
    }

    public static class Nature위험단계Codes
    {
        public const string SafeCore = "SafeCore";
        public const string WarningBand = "WarningBand";
        public const string EncounterBand = "EncounterBand";
    }

    public static class 몰입World자산GateCodes
    {
        public const string PolygonApocalypse = "PolygonApocalypse";
        public const string PolygonApocalypsePack = "POLYGON Apocalypse";
        public const string WaitingForApocalypseAssetPack =
            "WaitingForApocalypseAssetPack";
        public const string Ready = "Ready";
        public const string FallbackForbidden = "Forbidden";
    }

    public static class Nature위협PresentationKeys
    {
        public const string ZombieWarning = "survival.zombie-warning";
        public const string TacticalZombiePressure =
            "survival.tactical.squad.zombie-pressure";

        public static IReadOnlyList<string> EncounterKeys { get; } = new[]
        {
            ZombieWarning, TacticalZombiePressure,
        };
    }

    public sealed class 몰입World자산GateDecision
    {
        public string AssetGateCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string FallbackPolicyCode { get; set; } = string.Empty;
        public bool MonsterPresentationAvailable { get; set; }
        public bool PresentationOnly { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어에게 AreaSet 자산 준비 상태를 표현한다.",
        Boundary = "자산 Gate 표현은 WI 권위 전이와 실제 플레이 완료 증거가 아니다.")]
    public static class 몰입World자산GatePolicy
    {
        public static 몰입World자산GateDecision Evaluate(
            IEnumerable<string> installedPackCodes)
        {
            if (installedPackCodes == null)
                throw new ArgumentNullException(nameof(installedPackCodes));
            var installed = installedPackCodes.Any(value => string.Equals(
                value?.Trim(), 몰입World자산GateCodes.PolygonApocalypsePack,
                StringComparison.Ordinal));
            return new 몰입World자산GateDecision
            {
                AssetGateCode = 몰입World자산GateCodes.PolygonApocalypse,
                StateCode = installed
                    ? 몰입World자산GateCodes.Ready
                    : 몰입World자산GateCodes.WaitingForApocalypseAssetPack,
                FallbackPolicyCode = 몰입World자산GateCodes.FallbackForbidden,
                MonsterPresentationAvailable = installed,
                PresentationOnly = true,
            };
        }
    }

    public sealed class Nature조우PresentationDecision
    {
        public bool ShowMonsterActors { get; set; }
        public string BlockReasonCode { get; set; } = string.Empty;
        public int ThreatUnitCount { get; set; }
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "Nature 조우 상태를 플레이어가 이해할 수 있는 표현으로 투영한다.",
        Boundary = "Presentation 정책은 Simulation 결과를 결정하지 않는다.")]
    public static class Nature조우PresentationPolicy
    {
        public const int MaximumThreatUnitCount = 5;

        public static Nature조우PresentationDecision Evaluate(
            string riskBandCode,
            string presentationKey,
            몰입World자산GateDecision assetGate,
            int threatUnitCount,
            bool simulationOnly,
            bool isOperationalState)
        {
            if (assetGate == null) throw new ArgumentNullException(nameof(assetGate));
            var block = isOperationalState ? "OperationalThreatForbidden"
                : !simulationOnly ? "SimulationOnlyRequired"
                : !string.Equals(riskBandCode, Nature위험단계Codes.EncounterBand,
                    StringComparison.Ordinal) ? "EncounterBandRequired"
                : !Nature위협PresentationKeys.EncounterKeys.Contains(
                    presentationKey, StringComparer.Ordinal) ? "PresentationKeyUnsupported"
                : threatUnitCount < 1 || threatUnitCount > MaximumThreatUnitCount
                    ? "ThreatUnitCountInvalid"
                : !assetGate.MonsterPresentationAvailable
                    ? 몰입World자산GateCodes.WaitingForApocalypseAssetPack
                    : string.Empty;
            return new Nature조우PresentationDecision
            {
                ShowMonsterActors = string.IsNullOrEmpty(block),
                BlockReasonCode = block,
                ThreatUnitCount = threatUnitCount,
                ChangesWorldState = false,
                PresentationOnly = true,
            };
        }
    }

    public sealed class Nature위협RoutePresentationDecision
    {
        public string PressureLevelCode { get; set; } = string.Empty;
        public bool ShowWarning { get; set; }
        public Nature조우PresentationDecision Encounter { get; set; }
            = new Nature조우PresentationDecision();
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// 서버가 확정한 자연권 압력을 경고와 조우 표현으로만 번역한다.
    /// 자산 팩이 없더라도 경고는 유지하며 다른 팩의 몬스터로 대체하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "Nature 위협 경로를 플레이어 경험으로 표현한다.",
        Boundary = "경로 표현은 위협 상태나 WI 결과를 확정하지 않는다.")]
    public static class Nature위협RoutePresentationPolicy
    {
        public static Nature위협RoutePresentationDecision Evaluate(
            string pressureLevelCode,
            string riskBandCode,
            string presentationKey,
            몰입World자산GateDecision assetGate,
            int threatUnitCount,
            bool simulationOnly,
            bool isOperationalState)
        {
            var showWarning = simulationOnly && !isOperationalState
                && !string.Equals(pressureLevelCode, "Stable", StringComparison.Ordinal);
            return new Nature위협RoutePresentationDecision
            {
                PressureLevelCode = pressureLevelCode,
                ShowWarning = showWarning,
                Encounter = Nature조우PresentationPolicy.Evaluate(
                    riskBandCode, presentationKey, assetGate, threatUnitCount,
                    simulationOnly, isOperationalState),
                ChangesWorldState = false,
                PresentationOnly = true,
            };
        }
    }

    public static class Nature위협관찰CueCodes
    {
        public const string ThreatRouteMarker = "NatureThreatRouteMarker";
        public const string SafeCoreMarker = "NatureSafeCoreMarker";
        public const string ThreatToSafeCoreAxis = "NatureThreatToSafeCoreAxis";
        public const string ObserveThreatRoute = "NatureObserveThreatRoute";
        public const string ThreatWarning = "NatureThreatWarning";
        public const string ObservationOnly = "ObservationOnly";
        public const string EmergencyRetreat = "EmergencyRetreat";
        public const string NatureRestoration = "NatureRestoration";
        public const string ThreatResponse = "ThreatResponse";
    }

    public sealed class Nature위협관찰ChoicePresentation
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    public sealed class Nature위협관찰PreviewApiModel
    {
        public string NatureRouteCode { get; set; } = string.Empty;
        public int EffectivePressure { get; set; }
        public string PressureLevelCode { get; set; } = string.Empty;
        public string[] NextWorldInteractionIds { get; set; } = Array.Empty<string>();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class Nature위협관찰PresentationDecision
    {
        public string NatureRouteCode { get; set; } = string.Empty;
        public string PressureLevelCode { get; set; } = string.Empty;
        public int EffectivePressure { get; set; }
        public string ThreatDirectionCode { get; set; } = string.Empty;
        public string SafeCoreDirectionCode { get; set; } = string.Empty;
        public string ThreatMarkerCueCode { get; set; } = string.Empty;
        public string SafeCoreMarkerCueCode { get; set; } = string.Empty;
        public string CameraCueCode { get; set; } = string.Empty;
        public string AnimationCueCode { get; set; } = string.Empty;
        public string SoundCueCode { get; set; } = string.Empty;
        public string ScopeBoundaryCode { get; set; } = string.Empty;
        public Nature위협관찰ChoicePresentation[] NextChoices { get; set; }
            = Array.Empty<Nature위협관찰ChoicePresentation>();
        public bool CanConfirmObservation { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// WI-NATURE-01의 권위 Preview를 위협 방향·압력·안전 거점 신호로 번역한다.
    /// 관찰 이후 후퇴·복원 경로만 표시하며 전투 참여 방식은 WI-NATURE-11이 소유한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "위협 관찰의 플레이어 질문·신호·다음 선택을 정제한다.",
        Boundary = "카메라·애니메이션·음향 Cue는 관찰 Confirm이나 위협 결과를 확정하지 않는다.")]
    public static class Nature위협관찰PresentationPolicy
    {
        public static Nature위협관찰PresentationDecision Evaluate(
            Nature위협관찰PreviewApiModel preview,
            string threatDirectionCode,
            string safeCoreDirectionCode)
        {
            if (preview == null) throw new ArgumentNullException(nameof(preview));
            var localBlocks = new List<string>();
            if (!preview.SimulationOnly || preview.IsOperationalState)
                localBlocks.Add("SimulationOnlyThreatObservationRequired");
            if (string.IsNullOrWhiteSpace(preview.NatureRouteCode))
                localBlocks.Add("NatureThreatRoutePresentationMissing");
            if (string.IsNullOrWhiteSpace(threatDirectionCode))
                localBlocks.Add("NatureThreatDirectionMissing");
            if (string.IsNullOrWhiteSpace(safeCoreDirectionCode))
                localBlocks.Add("NatureSafeCoreDirectionMissing");
            var blocks = (preview.BlockingReasonCodes ?? Array.Empty<string>())
                .Concat(localBlocks).Distinct(StringComparer.Ordinal).ToArray();
            var available = preview.CanConfirm && blocks.Length == 0;
            var warning = !string.Equals(preview.PressureLevelCode,
                "Stable", StringComparison.Ordinal);
            var choices = (preview.NextWorldInteractionIds ?? Array.Empty<string>())
                .Where(value => value == "WI-NATURE-02" || value == "WI-NATURE-03"
                    || value == "WI-NATURE-11")
                .Distinct(StringComparer.Ordinal)
                .Select(value => new Nature위협관찰ChoicePresentation
                {
                    WorldInteractionId = value,
                    ChoiceCode = value == "WI-NATURE-02"
                        ? Nature위협관찰CueCodes.EmergencyRetreat
                        : value == "WI-NATURE-03"
                            ? Nature위협관찰CueCodes.NatureRestoration
                            : Nature위협관찰CueCodes.ThreatResponse,
                    IsAvailable = available,
                }).ToArray();
            return new Nature위협관찰PresentationDecision
            {
                NatureRouteCode = preview.NatureRouteCode,
                PressureLevelCode = preview.PressureLevelCode,
                EffectivePressure = preview.EffectivePressure,
                ThreatDirectionCode = threatDirectionCode?.Trim() ?? string.Empty,
                SafeCoreDirectionCode = safeCoreDirectionCode?.Trim() ?? string.Empty,
                ThreatMarkerCueCode = Nature위협관찰CueCodes.ThreatRouteMarker,
                SafeCoreMarkerCueCode = Nature위협관찰CueCodes.SafeCoreMarker,
                CameraCueCode = Nature위협관찰CueCodes.ThreatToSafeCoreAxis,
                AnimationCueCode = Nature위협관찰CueCodes.ObserveThreatRoute,
                SoundCueCode = warning ? Nature위협관찰CueCodes.ThreatWarning : string.Empty,
                ScopeBoundaryCode = Nature위협관찰CueCodes.ObservationOnly,
                NextChoices = choices,
                CanConfirmObservation = available,
                BlockingReasonCodes = blocks,
                ChangesWorldState = false,
                PresentationOnly = true,
            };
        }
    }

    public sealed class 몰입WorldTransitionSnapshot
    {
        public string ActiveInstanceStableId { get; set; } = string.Empty;
        public string PendingInstanceStableId { get; set; } = string.Empty;
        public bool IsTransitioning { get; set; }
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// 경관 인스턴스의 원자적 표현 전환만 관리한다. Simulation Tick과 업무 상태는 바꾸지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "결정된 경관 상태의 화면 전환을 조율한다.",
        Boundary = "Unity 전환은 WorldTick·Revision·WI 결과를 변경하지 않는다.")]
    public sealed class 몰입WorldTransitionCoordinator
    {
        private readonly HashSet<string> knownInstanceIds;

        public 몰입WorldTransitionCoordinator(
            IEnumerable<string> instanceStableIds,
            string initialInstanceStableId = 몰입WorldInstanceCodes.NatureHome)
        {
            if (instanceStableIds == null)
                throw new ArgumentNullException(nameof(instanceStableIds));
            knownInstanceIds = new HashSet<string>(
                instanceStableIds.Where(value => !string.IsNullOrWhiteSpace(value)),
                StringComparer.Ordinal);
            if (knownInstanceIds.Count == 0
                || !knownInstanceIds.Contains(initialInstanceStableId))
                throw new ArgumentException("ImmersiveWorldInstanceCatalogInvalid",
                    nameof(instanceStableIds));
            ActiveInstanceStableId = initialInstanceStableId;
        }

        public string ActiveInstanceStableId { get; private set; }
        public string PendingInstanceStableId { get; private set; } = string.Empty;
        public bool IsTransitioning => !string.IsNullOrEmpty(PendingInstanceStableId);

        public 몰입WorldTransitionSnapshot Request(string targetInstanceStableId)
        {
            if (string.IsNullOrWhiteSpace(targetInstanceStableId)
                || !knownInstanceIds.Contains(targetInstanceStableId))
                throw new InvalidOperationException("ImmersiveWorldInstanceUnknown");
            if (IsTransitioning
                && !string.Equals(PendingInstanceStableId, targetInstanceStableId,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("ImmersiveWorldTransitionInProgress");
            if (string.Equals(ActiveInstanceStableId, targetInstanceStableId,
                StringComparison.Ordinal))
                return Snapshot();
            PendingInstanceStableId = targetInstanceStableId;
            return Snapshot();
        }

        public 몰입WorldTransitionSnapshot Complete(
            string targetInstanceStableId,
            bool traversalReady)
        {
            if (!IsTransitioning
                || !string.Equals(PendingInstanceStableId, targetInstanceStableId,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("ImmersiveWorldTransitionNotPending");
            if (!traversalReady)
            {
                PendingInstanceStableId = string.Empty;
                return Snapshot();
            }
            ActiveInstanceStableId = targetInstanceStableId;
            PendingInstanceStableId = string.Empty;
            return Snapshot();
        }

        public 몰입WorldTransitionSnapshot Cancel()
        {
            PendingInstanceStableId = string.Empty;
            return Snapshot();
        }

        public 몰입WorldTransitionSnapshot Snapshot()
            => new 몰입WorldTransitionSnapshot
            {
                ActiveInstanceStableId = ActiveInstanceStableId,
                PendingInstanceStableId = PendingInstanceStableId,
                IsTransitioning = IsTransitioning,
                ChangesWorldState = false,
                PresentationOnly = true,
            };
    }
}
