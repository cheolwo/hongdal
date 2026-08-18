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
