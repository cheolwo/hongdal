using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Unity.Cards
{
    public static class Farm방위소집PresentationCodes
    {
        public const string WatchAnchor = "Spatial.FarmDefenseWatchAnchor";
        public const string MusterAnchor = "Spatial.FarmDefenseMusterAnchor";
        public const string StationedVisualKey = "Farm.Defense.Squad.Stationed";
        public const string MobilizedVisualKey = "Farm.Defense.Squad.Mobilized";
        public const string FallbackVisualKey = "Primitive.FarmDefenseSquadMarker";
        public const string InteractionAnchorCode = "InteractionAnchor.WI-FARM-DEFENSE-MOBILIZE.Preview";
        public const string CandidateRevision = "farm-defense-mobilization-presentation-candidates.r1";
    }

    public sealed class Farm방위소집VisualBinding
    {
        public string StatusCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string PrimaryAssetCandidateRef { get; set; } = string.Empty;
        public string AlternativeAssetCandidateRef { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; } = string.Empty;
        public string AnimationRoleCode { get; set; } = string.Empty;
        public string ActionCueCode { get; set; } = string.Empty;
        public string PrimaryAnimationClipRef { get; set; } = string.Empty;
        public string FallbackActionCueCode { get; set; } = string.Empty;
    }

    public sealed class Farm방위소집CardPresentation
    {
        public string PresentationStableId { get; set; } = string.Empty;
        public string SquadStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string ThreatStableId { get; set; } = string.Empty;
        public int AssignedWorkerCount { get; set; }
        public bool ProductionContributionSuspended { get; set; }
        public string RequiredHCapability { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string PrimaryAssetCandidateRef { get; set; } = string.Empty;
        public string AlternativeAssetCandidateRef { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string InteractionAnchorCode { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; } = string.Empty;
        public string AnimationRoleCode { get; set; } = string.Empty;
        public string ActionCueCode { get; set; } = string.Empty;
        public string PrimaryAnimationClipRef { get; set; } = string.Empty;
        public string FallbackActionCueCode { get; set; } = string.Empty;
        public bool CanRequestPreview { get; set; }
        public bool CanConfirmAuthority { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class Farm방위소집PresentationPreparation
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public long SourceRevision { get; set; }
        public Farm방위소집CardPresentation[] Squads { get; set; } = Array.Empty<Farm방위소집CardPresentation>();
        public bool PresentationOnly { get; set; } = true;
        public bool MutatesCanonicalState { get; set; }
        public string PlanHashSha256 { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "Farm 방위 소집 카드의 판독 순간, 상태별 H 기준점, VisualKey와 fallback을 구현 준비 계획으로 결속한다.",
        WorkOrderIds = new[] { "E7-WO-FARM-BARRACKS-DEFENSE" },
        WorldInteractionIds = new[] { "WI-FARM-DEFENSE-MOBILIZE" },
        Boundary = "E4 준비 계획은 실제 초소·분대 이동·Prefab·Scene·Collider·전투·입력 증거가 아니다.")]
    public sealed class Farm방위소집PresentationPreparationProjector
    {
        public Farm방위소집PresentationPreparation Project(string worldStableId,
            IEnumerable<SimulationFarm방위소집CardSnapshot> cards,
            IEnumerable<Farm방위소집VisualBinding> bindings)
        {
            if (string.IsNullOrWhiteSpace(worldStableId)) throw new InvalidOperationException("FarmDefensePresentationWorldInvalid");
            var cardValues = cards?.ToArray() ?? throw new ArgumentNullException(nameof(cards));
            var bindingValues = bindings?.ToArray() ?? throw new ArgumentNullException(nameof(bindings));
            if (cardValues.Any(x => x == null || string.IsNullOrWhiteSpace(x.SquadStableId) || x.SourceWorldRevision < 0 || x.AssignedWorkerCount < 0 || (x.StatusCode != SimulationFarm방위소집Codes.대기 && x.StatusCode != SimulationFarm방위소집Codes.출동)) || cardValues.Select(x => x.SquadStableId).Distinct(StringComparer.Ordinal).Count() != cardValues.Length)
                throw new InvalidOperationException("FarmDefensePresentationCardInvalid");
            if (bindingValues.Any(x => x == null || string.IsNullOrWhiteSpace(x.StatusCode) || string.IsNullOrWhiteSpace(x.VisualKey) || string.IsNullOrWhiteSpace(x.PrimaryAssetCandidateRef) || string.IsNullOrWhiteSpace(x.AlternativeAssetCandidateRef) || string.IsNullOrWhiteSpace(x.FallbackVisualKey) || string.IsNullOrWhiteSpace(x.CandidateRevisionOrFingerprint) || string.IsNullOrWhiteSpace(x.AnimationRoleCode) || string.IsNullOrWhiteSpace(x.ActionCueCode) || string.IsNullOrWhiteSpace(x.PrimaryAnimationClipRef) || string.IsNullOrWhiteSpace(x.FallbackActionCueCode)) || bindingValues.Select(x => x.StatusCode).Distinct(StringComparer.Ordinal).Count() != bindingValues.Length)
                throw new InvalidOperationException("FarmDefenseVisualBindingInvalid");
            var revision = cardValues.Length == 0 ? 0 : cardValues[0].SourceWorldRevision;
            if (cardValues.Any(x => x.SourceWorldRevision != revision)) throw new InvalidOperationException("FarmDefensePresentationRevisionMixed");
            var byStatus = bindingValues.ToDictionary(x => x.StatusCode, StringComparer.Ordinal);
            var result = new Farm방위소집PresentationPreparation {
                PlanStableId = "presentation-plan:farm-defense-mobilization:" + worldStableId,
                WorldStableId = worldStableId, SourceRevision = revision,
                Squads = cardValues.OrderBy(x => x.SquadStableId, StringComparer.Ordinal).Select(x => Create(x, byStatus.TryGetValue(x.StatusCode, out var b) ? b : null)).ToArray(),
                PresentationOnly = true, MutatesCanonicalState = false };
            result.PlanHashSha256 = Hash(result);
            return result;
        }

        private static Farm방위소집CardPresentation Create(SimulationFarm방위소집CardSnapshot card, Farm방위소집VisualBinding? binding)
        {
            var fallback = binding?.FallbackVisualKey ?? Farm방위소집PresentationCodes.FallbackVisualKey;
            var stationed = card.StatusCode == SimulationFarm방위소집Codes.대기;
            return new Farm방위소집CardPresentation {
                PresentationStableId = "presentation:farm-defense-squad:" + card.SquadStableId,
                SquadStableId = card.SquadStableId, StatusCode = card.StatusCode, ThreatStableId = card.ThreatStableId,
                AssignedWorkerCount = card.AssignedWorkerCount, ProductionContributionSuspended = card.ProductionContributionSuspended,
                RequiredHCapability = stationed ? Farm방위소집PresentationCodes.MusterAnchor : Farm방위소집PresentationCodes.WatchAnchor,
                VisualKey = binding?.VisualKey ?? fallback, PrimaryAssetCandidateRef = binding?.PrimaryAssetCandidateRef ?? string.Empty,
                AlternativeAssetCandidateRef = binding?.AlternativeAssetCandidateRef ?? string.Empty, FallbackVisualKey = fallback,
                InteractionAnchorCode = Farm방위소집PresentationCodes.InteractionAnchorCode,
                CandidateRevisionOrFingerprint = binding?.CandidateRevisionOrFingerprint ?? "fallback:farm-defense-squad-marker.r1",
                AnimationRoleCode = binding?.AnimationRoleCode ?? "SquadStaticMarker", ActionCueCode = binding?.ActionCueCode ?? "Squad.State.Static",
                PrimaryAnimationClipRef = binding?.PrimaryAnimationClipRef ?? string.Empty, FallbackActionCueCode = binding?.FallbackActionCueCode ?? "Squad.State.Static",
                CanRequestPreview = stationed, CanConfirmAuthority = false, PresentationOnly = true };
        }

        private static string Hash(Farm방위소집PresentationPreparation value)
        {
            var text = new StringBuilder().Append(value.PlanStableId).Append('|').Append(value.SourceRevision).Append('|').Append(value.PresentationOnly).Append('|').Append(value.MutatesCanonicalState);
            foreach (var x in value.Squads) text.Append('\n').Append(x.PresentationStableId).Append('|').Append(x.StatusCode).Append('|').Append(x.ThreatStableId).Append('|').Append(x.AssignedWorkerCount).Append('|').Append(x.ProductionContributionSuspended).Append('|').Append(x.RequiredHCapability).Append('|').Append(x.VisualKey).Append('|').Append(x.PrimaryAssetCandidateRef).Append('|').Append(x.AlternativeAssetCandidateRef).Append('|').Append(x.CandidateRevisionOrFingerprint).Append('|').Append(x.AnimationRoleCode).Append('|').Append(x.ActionCueCode).Append('|').Append(x.PrimaryAnimationClipRef);
            using var sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString())).Select(x => x.ToString("x2")));
        }
    }
}
