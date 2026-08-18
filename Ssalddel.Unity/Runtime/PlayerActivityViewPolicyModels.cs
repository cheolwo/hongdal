using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.PlayerActivities
{
    public static class PlayerActivityCodes
    {
        public const string WorldOverview = "WorldOverview";
        public const string FarmManagement = "FarmManagement";
        public const string Exploration = "Exploration";
        public const string Logistics = "Logistics";
        public const string Combat = "Combat";
    }

    public static class PlayerActivityViewModeCodes
    {
        public const string Strategy = "Strategy";
        public const string FirstPerson = "FirstPerson";
        public const string TacticalThirdPerson = "TacticalThirdPerson";
    }

    public static class PlayerActivityViewCapabilityCodes
    {
        public const string DirectMovement = "DirectMovement";
        public const string ProximityInteraction = "ProximityInteraction";
        public const string VisibilityDrivenStreaming = "VisibilityDrivenStreaming";
        public const string MultiTargetSelection = "MultiTargetSelection";
        public const string AreaStatusOverlay = "AreaStatusOverlay";
        public const string WorkDraftPreview = "WorkDraftPreview";
        public const string BatchWorkPlanning = "BatchWorkPlanning";
        public const string WiderReactionWindow = "WiderReactionWindow";
        public const string FocusedThreatTelegraph = "FocusedThreatTelegraph";
    }

    public sealed class PlayerActivityViewPolicy
    {
        public string ActivityCode { get; set; } = string.Empty;
        public string DefaultViewModeCode { get; set; } = string.Empty;
        public string[] AllowedViewModeCodes { get; set; } = Array.Empty<string>();
        public string[] AdvantageCapabilityCodes { get; set; } = Array.Empty<string>();
        public bool AllowsManualOverride { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class PlayerActivityViewDecision
    {
        public string ActivityCode { get; set; } = string.Empty;
        public string ViewModeCode { get; set; } = string.Empty;
        public bool UsedActivityDefault { get; set; }
        public bool ManualOverrideApplied { get; set; }
        public string[] AdvantageCapabilityCodes { get; set; } = Array.Empty<string>();
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// 활동에 알맞은 기본 시점을 정하되 사용자의 허용된 수동 전환을 막지 않는다.
    /// 이 정책은 카메라와 입력 표현만 결정하며 Simulation 상태를 변경하지 않는다.
    /// </summary>
    public sealed class PlayerActivityViewPolicyCatalog
    {
        public const string RuleRevision = "player-activity-view-policy.v1";

        private readonly IReadOnlyDictionary<string, PlayerActivityViewPolicy> policies;

        public PlayerActivityViewPolicyCatalog(IEnumerable<PlayerActivityViewPolicy> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var normalized = values.ToArray();
            if (normalized.Length == 0
                || normalized.Any(value => !Validate(value))
                || normalized.GroupBy(value => value.ActivityCode, StringComparer.Ordinal)
                    .Any(group => group.Count() != 1))
                throw new ArgumentException("PlayerActivityViewPolicyInvalid", nameof(values));
            policies = normalized.ToDictionary(
                value => value.ActivityCode,
                value => value,
                StringComparer.Ordinal);
        }

        public PlayerActivityViewDecision Resolve(
            string activityCode,
            string? requestedViewModeCode = null)
        {
            if (string.IsNullOrWhiteSpace(activityCode)
                || !policies.TryGetValue(activityCode.Trim(), out var policy))
                throw new InvalidOperationException("PlayerActivityViewPolicyNotFound");

            var requested = requestedViewModeCode?.Trim();
            var manualOverride = !string.IsNullOrWhiteSpace(requested)
                && !string.Equals(requested, policy.DefaultViewModeCode,
                    StringComparison.Ordinal);
            if (manualOverride
                && (!policy.AllowsManualOverride
                    || !policy.AllowedViewModeCodes.Contains(requested!,
                        StringComparer.Ordinal)))
                throw new InvalidOperationException("PlayerActivityViewOverrideNotAllowed");

            var resolved = string.IsNullOrWhiteSpace(requested)
                ? policy.DefaultViewModeCode
                : requested!;
            return new PlayerActivityViewDecision
            {
                ActivityCode = policy.ActivityCode,
                ViewModeCode = resolved,
                UsedActivityDefault = string.IsNullOrWhiteSpace(requested),
                ManualOverrideApplied = manualOverride,
                AdvantageCapabilityCodes = resolved == policy.DefaultViewModeCode
                    ? policy.AdvantageCapabilityCodes.ToArray()
                    : Array.Empty<string>(),
                ChangesWorldState = false,
                PresentationOnly = true,
            };
        }

        public static PlayerActivityViewPolicyCatalog CreateDefault()
            => new PlayerActivityViewPolicyCatalog(new[]
            {
                Policy(PlayerActivityCodes.WorldOverview,
                    PlayerActivityViewModeCodes.Strategy,
                    new[] { PlayerActivityViewModeCodes.Strategy },
                    Array.Empty<string>(), false),
                Policy(PlayerActivityCodes.FarmManagement,
                    PlayerActivityViewModeCodes.TacticalThirdPerson,
                    new[]
                    {
                        PlayerActivityViewModeCodes.TacticalThirdPerson,
                        PlayerActivityViewModeCodes.FirstPerson,
                    },
                    new[]
                    {
                        PlayerActivityViewCapabilityCodes.MultiTargetSelection,
                        PlayerActivityViewCapabilityCodes.AreaStatusOverlay,
                        PlayerActivityViewCapabilityCodes.WorkDraftPreview,
                        PlayerActivityViewCapabilityCodes.BatchWorkPlanning,
                    }, true),
                Policy(PlayerActivityCodes.Exploration,
                    PlayerActivityViewModeCodes.TacticalThirdPerson,
                    new[]
                    {
                        PlayerActivityViewModeCodes.TacticalThirdPerson,
                        PlayerActivityViewModeCodes.FirstPerson,
                    },
                    new[]
                    {
                        PlayerActivityViewCapabilityCodes.DirectMovement,
                        PlayerActivityViewCapabilityCodes.ProximityInteraction,
                        PlayerActivityViewCapabilityCodes.VisibilityDrivenStreaming,
                    }, true),
                Policy(PlayerActivityCodes.Logistics,
                    PlayerActivityViewModeCodes.TacticalThirdPerson,
                    new[]
                    {
                        PlayerActivityViewModeCodes.TacticalThirdPerson,
                        PlayerActivityViewModeCodes.FirstPerson,
                    },
                    new[]
                    {
                        PlayerActivityViewCapabilityCodes.AreaStatusOverlay,
                        PlayerActivityViewCapabilityCodes.WorkDraftPreview,
                    }, true),
                Policy(PlayerActivityCodes.Combat,
                    PlayerActivityViewModeCodes.TacticalThirdPerson,
                    new[]
                    {
                        PlayerActivityViewModeCodes.TacticalThirdPerson,
                        PlayerActivityViewModeCodes.FirstPerson,
                    },
                    new[]
                    {
                        PlayerActivityViewCapabilityCodes.WiderReactionWindow,
                        PlayerActivityViewCapabilityCodes.FocusedThreatTelegraph,
                    }, true),
            });

        private static PlayerActivityViewPolicy Policy(
            string activityCode,
            string defaultViewModeCode,
            string[] allowedViewModeCodes,
            string[] advantageCapabilityCodes,
            bool allowsManualOverride)
            => new PlayerActivityViewPolicy
            {
                ActivityCode = activityCode,
                DefaultViewModeCode = defaultViewModeCode,
                AllowedViewModeCodes = allowedViewModeCodes,
                AdvantageCapabilityCodes = advantageCapabilityCodes,
                AllowsManualOverride = allowsManualOverride,
                PresentationOnly = true,
            };

        private static bool Validate(PlayerActivityViewPolicy value)
            => value != null
                && !string.IsNullOrWhiteSpace(value.ActivityCode)
                && !string.IsNullOrWhiteSpace(value.DefaultViewModeCode)
                && value.AllowedViewModeCodes != null
                && value.AllowedViewModeCodes.Length > 0
                && value.AllowedViewModeCodes.Contains(
                    value.DefaultViewModeCode, StringComparer.Ordinal)
                && value.AllowedViewModeCodes.All(code =>
                    !string.IsNullOrWhiteSpace(code))
                && value.AllowedViewModeCodes.Distinct(StringComparer.Ordinal).Count()
                    == value.AllowedViewModeCodes.Length
                && value.AdvantageCapabilityCodes != null
                && value.AdvantageCapabilityCodes.All(code =>
                    !string.IsNullOrWhiteSpace(code))
                && value.PresentationOnly;
    }
}
