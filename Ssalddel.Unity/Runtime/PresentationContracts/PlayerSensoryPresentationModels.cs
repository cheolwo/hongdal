using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Unity.PresentationContracts
{
    public static class 감각표현요구Codes
    {
        public const string Observable = "Observable";
        public const string Interactable = "Interactable";

        public static readonly string[] All = { Observable, Interactable };
    }

    public static class 감각표현단계Codes
    {
        public const string Available = "Available";
        public const string Acquired = "Acquired";
        public const string Working = "Working";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";

        public static readonly string[] All =
        {
            Available, Acquired, Working, Cancelled, Completed,
        };
    }

    public static class 배치표현AnchorRoleCodes
    {
        public const string CameraFocus = "CameraFocus";
        public const string ActorWork = "ActorWork";
        public const string ToolSocket = "ToolSocket";
        public const string AudioEmitter = "AudioEmitter";
        public const string Fx = "Fx";
        public const string Cutaway = "Cutaway";

        public static readonly string[] All =
        {
            CameraFocus, ActorWork, ToolSocket, AudioEmitter, Fx, Cutaway,
        };
    }

    public static class 감각표현CueCodes
    {
        public const string None = "none";
        public const string AxePickup = "audio:nature:axe-pickup";
        public const string AxeImpact = "audio:nature:axe-impact";
        public const string HarvestCancelled = "audio:nature:harvest-cancelled";
        public const string TreeFall = "audio:nature:tree-fall";
        public const string WoodChip = "fx:nature:wood-chip";
        public const string TreeFallDust = "fx:nature:tree-fall-dust";
        public const string ForestAmbient = "ambient:nature:forest";
        public const string NatureExplorationMusic = "music:nature:exploration";
    }

    public sealed class 배치표현AnchorBinding
    {
        public string AnchorStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string OwningH1StableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public float LocalX { get; set; }
        public float LocalY { get; set; }
        public float LocalZ { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class 배치감각표현BindingPlan
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string SchemaCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string PlacementControlRevision { get; set; } = string.Empty;
        public string[] StructuralPlacementSourceStableIds { get; set; } =
            Array.Empty<string>();
        public string StructuralPlacementSourceHash { get; set; } = string.Empty;
        public 배치표현AnchorBinding[] Anchors { get; set; } =
            Array.Empty<배치표현AnchorBinding>();
        public bool PresentationOnly { get; set; } = true;
        public string PlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Wi감각표현PhaseBinding
    {
        public string PhaseCode { get; set; } = string.Empty;
        public string[] RequiredDomainCodes { get; set; } = Array.Empty<string>();
        public string AnimationIntentCode { get; set; } = string.Empty;
        public string AudioCueCode { get; set; } = string.Empty;
        public string FxCueCode { get; set; } = string.Empty;
        public string UiCueCode { get; set; } = string.Empty;
    }

    public sealed class Wi감각표현Plan
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string SchemaCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string WorldInteractionId { get; set; } = string.Empty;
        public string RequirementCode { get; set; } = string.Empty;
        public string PlacementBindingPlanHash { get; set; } = string.Empty;
        public string AreaAmbientCueCode { get; set; } = string.Empty;
        public string AreaMusicCueCode { get; set; } = string.Empty;
        public bool AreaAmbientRequired { get; set; }
        public bool AreaMusicRequired { get; set; }
        public Wi감각표현PhaseBinding[] Phases { get; set; } =
            Array.Empty<Wi감각표현PhaseBinding>();
        public bool PresentationOnly { get; set; } = true;
        public bool MutatesCanonicalState { get; set; }
        public bool ConfirmsBusinessCompletion { get; set; }
        public string PlanHashSha256 { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "배치 객체의 카메라·애니메이션·조명·음향·UI 표현 결속을 정제한다.",
        WorkOrderIds = new[] { "E9-WO-NATURE-SURVIVAL-SOLO-PLACEMENT" },
        Boundary = "표현 계약과 자동 검증은 실제 Game View·청음 또는 권위 상태 완료를 대신하지 않는다.")]
    public sealed class 플레이어감각표현Validator
    {
        public const string PlacementSchema = "placement-presentation-bindings.v1";
        public const string WiSchema = "wi-presentation-plan.v1";
        public const string PlacementControlRevision = "placement-control-hierarchy.v4";

        public void Validate(배치감각표현BindingPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            RequireId(plan.PlanStableId, "PlacementPresentationPlanStableIdInvalid");
            if (plan.SchemaCode != PlacementSchema || plan.Revision <= 0
                || plan.PlacementControlRevision != PlacementControlRevision
                || plan.StructuralPlacementSourceStableIds == null
                || plan.StructuralPlacementSourceStableIds.Length == 0
                || plan.StructuralPlacementSourceStableIds.Any(
                    string.IsNullOrWhiteSpace)
                || plan.StructuralPlacementSourceStableIds.Distinct(
                    StringComparer.Ordinal).Count()
                    != plan.StructuralPlacementSourceStableIds.Length
                || !IsHash(plan.StructuralPlacementSourceHash)
                || plan.Anchors == null || plan.Anchors.Length == 0
                || !plan.PresentationOnly)
                throw new InvalidOperationException("PlacementPresentationPlanInvalid");
            if (!string.Equals(plan.StructuralPlacementSourceHash,
                    플레이어감각표현Hasher.ComputeSourceSetHash(
                        plan.StructuralPlacementSourceStableIds),
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "PlacementPresentationSourceHashInvalid");
            foreach (var sourceStableId in plan.StructuralPlacementSourceStableIds)
                RequireId(sourceStableId,
                    "PlacementPresentationSourceStableIdInvalid");

            var anchorIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var anchor in plan.Anchors)
            {
                if (anchor == null) throw new InvalidOperationException(
                    "PlacementPresentationAnchorInvalid");
                RequireId(anchor.AnchorStableId, "PlacementPresentationAnchorInvalid");
                RequireId(anchor.PlacementStableId, "PlacementPresentationPlacementInvalid");
                RequireId(anchor.OwningH1StableId, "PlacementPresentationH1Invalid");
                if (!배치표현AnchorRoleCodes.All.Contains(anchor.RoleCode,
                        StringComparer.Ordinal)
                    || !anchor.PresentationOnly
                    || !IsFinite(anchor.LocalX) || !IsFinite(anchor.LocalY)
                    || !IsFinite(anchor.LocalZ)
                    || !anchorIds.Add(anchor.AnchorStableId))
                    throw new InvalidOperationException("PlacementPresentationAnchorInvalid");
            }

            RequireMatchingHash(plan.PlanHashSha256,
                플레이어감각표현Hasher.Compute(plan),
                "PlacementPresentationPlanHashInvalid");
        }

        public void Validate(Wi감각표현Plan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            RequireId(plan.PlanStableId, "WiPresentationPlanStableIdInvalid");
            RequireId(plan.WorldInteractionId, "WiPresentationWorldInteractionInvalid");
            if (plan.SchemaCode != WiSchema || plan.Revision <= 0
                || !감각표현요구Codes.All.Contains(plan.RequirementCode,
                    StringComparer.Ordinal)
                || !IsHash(plan.PlacementBindingPlanHash)
                || plan.Phases == null || plan.Phases.Length == 0
                || !plan.PresentationOnly || plan.MutatesCanonicalState
                || plan.ConfirmsBusinessCompletion)
                throw new InvalidOperationException("WiPresentationPlanInvalid");

            RequireId(plan.AreaAmbientCueCode, "WiPresentationAmbientCueInvalid");
            RequireId(plan.AreaMusicCueCode, "WiPresentationMusicCueInvalid");
            var phaseCodes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var phase in plan.Phases)
            {
                if (phase == null
                    || !감각표현단계Codes.All.Contains(phase.PhaseCode,
                        StringComparer.Ordinal)
                    || !phaseCodes.Add(phase.PhaseCode)
                    || phase.RequiredDomainCodes == null
                    || phase.RequiredDomainCodes.Length == 0
                    || phase.RequiredDomainCodes.Any(value =>
                        !표현규칙영역Codes.All.Contains(value, StringComparer.Ordinal))
                    || phase.RequiredDomainCodes.Distinct(StringComparer.Ordinal).Count()
                        != phase.RequiredDomainCodes.Length)
                    throw new InvalidOperationException("WiPresentationPhaseInvalid");
                ValidateCueDomain(phase.AnimationIntentCode,
                    표현규칙영역Codes.Animation, phase.RequiredDomainCodes,
                    "WiPresentationAnimationDomainMissing");
                ValidateCueDomain(phase.AudioCueCode,
                    표현규칙영역Codes.Audio, phase.RequiredDomainCodes,
                    "WiPresentationAudioDomainMissing");
                ValidateCueDomain(phase.FxCueCode,
                    표현규칙영역Codes.Graphics, phase.RequiredDomainCodes,
                    "WiPresentationFxDomainMissing");
                ValidateCueDomain(phase.UiCueCode,
                    표현규칙영역Codes.UI, phase.RequiredDomainCodes,
                    "WiPresentationUiDomainMissing");
            }

            if (plan.RequirementCode == 감각표현요구Codes.Interactable
                && (!plan.Phases.Any(value => value.RequiredDomainCodes.Contains(
                        표현규칙영역Codes.Camera, StringComparer.Ordinal))
                    || !plan.Phases.Any(value => value.RequiredDomainCodes.Contains(
                        표현규칙영역Codes.Graphics, StringComparer.Ordinal))))
                throw new InvalidOperationException(
                    "WiPresentationInteractableViewDomainMissing");

            RequireMatchingHash(plan.PlanHashSha256,
                플레이어감각표현Hasher.Compute(plan),
                "WiPresentationPlanHashInvalid");
        }

        private static void ValidateCueDomain(string value, string domain,
            IEnumerable<string> requiredDomains, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            RequireId(value, errorCode);
            if (!requiredDomains.Contains(domain, StringComparer.Ordinal))
                throw new InvalidOperationException(errorCode);
        }

        private static void RequireMatchingHash(string actual, string expected,
            string errorCode)
        {
            if (!IsHash(actual) || !string.Equals(actual, expected,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(errorCode);
        }

        private static bool IsHash(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(character => character is >= '0' and <= '9'
                   or >= 'a' and <= 'f' or >= 'A' and <= 'F');

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void RequireId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 180
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new InvalidOperationException(errorCode);
        }
    }

    public static class 플레이어감각표현Hasher
    {
        public static string Compute(배치감각표현BindingPlan plan)
        {
            var text = new StringBuilder()
                .Append(plan.PlanStableId).Append('|')
                .Append(plan.SchemaCode).Append('|')
                .Append(plan.Revision).Append('|')
                .Append(plan.PlacementControlRevision).Append('|')
                .Append(string.Join(",", plan.StructuralPlacementSourceStableIds
                    .OrderBy(value => value, StringComparer.Ordinal))).Append('|')
                .Append(plan.StructuralPlacementSourceHash.ToLowerInvariant())
                .Append('|').Append(plan.PresentationOnly);
            foreach (var anchor in plan.Anchors.OrderBy(value =>
                         value.AnchorStableId, StringComparer.Ordinal))
                text.Append('\n').Append(anchor.AnchorStableId).Append('|')
                    .Append(anchor.PlacementStableId).Append('|')
                    .Append(anchor.OwningH1StableId).Append('|')
                    .Append(anchor.RoleCode).Append('|')
                    .Append(Number(anchor.LocalX)).Append('|')
                    .Append(Number(anchor.LocalY)).Append('|')
                    .Append(Number(anchor.LocalZ)).Append('|')
                    .Append(anchor.PresentationOnly);
            return Sha256(text.ToString());
        }

        public static string Compute(Wi감각표현Plan plan)
        {
            var text = new StringBuilder()
                .Append(plan.PlanStableId).Append('|')
                .Append(plan.SchemaCode).Append('|')
                .Append(plan.Revision).Append('|')
                .Append(plan.WorldInteractionId).Append('|')
                .Append(plan.RequirementCode).Append('|')
                .Append(plan.PlacementBindingPlanHash.ToLowerInvariant()).Append('|')
                .Append(plan.AreaAmbientCueCode).Append('|')
                .Append(plan.AreaMusicCueCode).Append('|')
                .Append(plan.AreaAmbientRequired).Append('|')
                .Append(plan.AreaMusicRequired).Append('|')
                .Append(plan.PresentationOnly).Append('|')
                .Append(plan.MutatesCanonicalState).Append('|')
                .Append(plan.ConfirmsBusinessCompletion);
            foreach (var phase in plan.Phases.OrderBy(value =>
                         value.PhaseCode, StringComparer.Ordinal))
                text.Append('\n').Append(phase.PhaseCode).Append('|')
                    .Append(string.Join(",", phase.RequiredDomainCodes
                        .OrderBy(value => value, StringComparer.Ordinal)))
                    .Append('|').Append(phase.AnimationIntentCode)
                    .Append('|').Append(phase.AudioCueCode)
                    .Append('|').Append(phase.FxCueCode)
                    .Append('|').Append(phase.UiCueCode);
            return Sha256(text.ToString());
        }

        private static string Number(float value)
            => value.ToString("0.####", CultureInfo.InvariantCulture);

        private static string Sha256(string value)
        {
            using var algorithm = SHA256.Create();
            return string.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value))
                .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
        }

        public static string ComputeSourceSetHash(IEnumerable<string> stableIds)
            => Sha256(string.Join("\n", stableIds
                .OrderBy(value => value, StringComparer.Ordinal)));
    }

    public static class Nature플레이어감각표현Fixture
    {
        public static 배치감각표현BindingPlan CreatePlacementPlan()
        {
            var sourceIds = new[]
            {
                "wi-spatial-seedbed:nature-survival-home.v1",
                "wi-spatial-seedbed:nature-survival-encounter.v1",
            };
            var plan = new 배치감각표현BindingPlan
            {
                PlanStableId = "placement-presentation:nature-axe-harvest.v1",
                SchemaCode = 플레이어감각표현Validator.PlacementSchema,
                Revision = 1,
                PlacementControlRevision =
                    플레이어감각표현Validator.PlacementControlRevision,
                StructuralPlacementSourceStableIds = sourceIds,
                StructuralPlacementSourceHash = 플레이어감각표현Hasher
                    .ComputeSourceSetHash(sourceIds),
                Anchors = new[]
                {
                    Anchor("anchor:nature-axe:camera-focus",
                        "pickup:nature-safe-clearing:basic-axe",
                        "h1-stock:nature-trailhead",
                        배치표현AnchorRoleCodes.CameraFocus, 0f, .35f, 0f),
                    Anchor("anchor:nature-axe:tool-socket",
                        "pickup:nature-safe-clearing:basic-axe",
                        "h1-stock:nature-trailhead",
                        배치표현AnchorRoleCodes.ToolSocket, .32f, -.35f, .7f),
                    Anchor("anchor:nature-axe:audio-emitter",
                        "pickup:nature-safe-clearing:basic-axe",
                        "h1-stock:nature-trailhead",
                        배치표현AnchorRoleCodes.AudioEmitter, 0f, .35f, 0f),
                    Anchor("anchor:nature-tree:camera-focus",
                        "placement-role:nature-harvest-tree",
                        "h1-stock:nature-exploration-buffer",
                        배치표현AnchorRoleCodes.CameraFocus, 0f, 1.2f, 0f),
                    Anchor("anchor:nature-tree:actor-work",
                        "placement-role:nature-harvest-tree",
                        "h1-stock:nature-exploration-buffer",
                        배치표현AnchorRoleCodes.ActorWork, 0f, 0f, -1.5f),
                    Anchor("anchor:nature-tree:audio-emitter",
                        "placement-role:nature-harvest-tree",
                        "h1-stock:nature-exploration-buffer",
                        배치표현AnchorRoleCodes.AudioEmitter, 0f, .9f, 0f),
                    Anchor("anchor:nature-tree:fx",
                        "placement-role:nature-harvest-tree",
                        "h1-stock:nature-exploration-buffer",
                        배치표현AnchorRoleCodes.Fx, 0f, .8f, 0f),
                },
            };
            plan.PlanHashSha256 = 플레이어감각표현Hasher.Compute(plan);
            return plan;
        }

        public static Wi감각표현Plan CreateAxePlan()
        {
            var plan = BasePlan("wi-presentation:nature-axe-pickup.v1",
                "WI-NATURE-05");
            plan.Phases = new[]
            {
                Phase(감각표현단계Codes.Available,
                    new[] { 표현규칙영역Codes.Graphics,
                        표현규칙영역Codes.Camera, 표현규칙영역Codes.UI },
                    ui: "ui:nature:axe-available"),
                Phase(감각표현단계Codes.Acquired,
                    new[] { 표현규칙영역Codes.Graphics,
                        표현규칙영역Codes.Audio, 표현규칙영역Codes.UI },
                    audio: 감각표현CueCodes.AxePickup,
                    ui: "ui:nature:axe-acquired"),
            };
            plan.PlanHashSha256 = 플레이어감각표현Hasher.Compute(plan);
            return plan;
        }

        public static Wi감각표현Plan CreateHarvestPlan()
        {
            var plan = BasePlan("wi-presentation:nature-tree-harvest.v1",
                "WI-NATURE-06");
            plan.Phases = new[]
            {
                Phase(감각표현단계Codes.Available,
                    new[] { 표현규칙영역Codes.Graphics,
                        표현규칙영역Codes.Camera, 표현규칙영역Codes.UI },
                    ui: "ui:nature:tree-available"),
                Phase(감각표현단계Codes.Working,
                    new[] { 표현규칙영역Codes.Graphics,
                        표현규칙영역Codes.Camera,
                        표현규칙영역Codes.Animation,
                        표현규칙영역Codes.Audio, 표현규칙영역Codes.UI },
                    animation: "animation:nature:axe-swing",
                    audio: 감각표현CueCodes.AxeImpact,
                    fx: 감각표현CueCodes.WoodChip,
                    ui: "ui:nature:harvest-progress"),
                Phase(감각표현단계Codes.Cancelled,
                    new[] { 표현규칙영역Codes.Animation,
                        표현규칙영역Codes.Audio, 표현규칙영역Codes.UI },
                    animation: "animation:common:idle",
                    audio: 감각표현CueCodes.HarvestCancelled,
                    ui: "ui:nature:harvest-cancelled"),
                Phase(감각표현단계Codes.Completed,
                    new[] { 표현규칙영역Codes.Graphics,
                        표현규칙영역Codes.Animation,
                        표현규칙영역Codes.Audio, 표현규칙영역Codes.UI },
                    animation: "animation:common:idle",
                    audio: 감각표현CueCodes.TreeFall,
                    fx: 감각표현CueCodes.TreeFallDust,
                    ui: "ui:nature:harvest-completed"),
            };
            plan.PlanHashSha256 = 플레이어감각표현Hasher.Compute(plan);
            return plan;
        }

        private static Wi감각표현Plan BasePlan(string stableId, string wiId)
            => new Wi감각표현Plan
            {
                PlanStableId = stableId,
                SchemaCode = 플레이어감각표현Validator.WiSchema,
                Revision = 1,
                WorldInteractionId = wiId,
                RequirementCode = 감각표현요구Codes.Interactable,
                PlacementBindingPlanHash = CreatePlacementPlan().PlanHashSha256,
                AreaAmbientCueCode = 감각표현CueCodes.ForestAmbient,
                AreaMusicCueCode = 감각표현CueCodes.NatureExplorationMusic,
                AreaAmbientRequired = false,
                AreaMusicRequired = false,
                PresentationOnly = true,
            };

        private static Wi감각표현PhaseBinding Phase(string code,
            string[] domains, string animation = "", string audio = "",
            string fx = "", string ui = "")
            => new Wi감각표현PhaseBinding
            {
                PhaseCode = code,
                RequiredDomainCodes = domains,
                AnimationIntentCode = animation,
                AudioCueCode = audio,
                FxCueCode = fx,
                UiCueCode = ui,
            };

        private static 배치표현AnchorBinding Anchor(string stableId,
            string placementId, string h1Id, string role, float x, float y,
            float z)
            => new 배치표현AnchorBinding
            {
                AnchorStableId = stableId,
                PlacementStableId = placementId,
                OwningH1StableId = h1Id,
                RoleCode = role,
                LocalX = x,
                LocalY = y,
                LocalZ = z,
            };
    }
}
