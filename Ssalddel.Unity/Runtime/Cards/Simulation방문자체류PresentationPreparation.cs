using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Unity.Cards
{
    public static class 방문자체류PresentationCodes
    {
        public const string VisitorWaitingAnchor =
            "Spatial.VisitorWaitingAnchor";
        public const string GuestRestAnchor = "Spatial.GuestRestAnchor";
        public const string VisitorDepartureAnchor =
            "Spatial.VisitorDepartureAnchor";
        public const string WaitingVisualKey =
            "Community.Visitor.Stay.AwaitingDecision";
        public const string AcceptedVisualKey =
            "Community.Visitor.Stay.TemporaryStay";
        public const string RejectedVisualKey =
            "Community.Visitor.Stay.Rejected";
        public const string FallbackVisualKey =
            "Primitive.CommunityVisitorMarker";
        public const string InteractionAnchorCode =
            "InteractionAnchor.WI-COMMUNITY-VISITOR-STAY.Preview";
        public const string CandidateRevision =
            "community-visitor-stay-presentation-candidates.r1";
    }

    public sealed class 방문자체류VisualBinding
    {
        public string StatusCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string PrimaryAssetCandidateRef { get; set; } = string.Empty;
        public string AlternativeAssetCandidateRef { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; }
            = string.Empty;
        public string AnimationRoleCode { get; set; } = string.Empty;
        public string ActionCueCode { get; set; } = string.Empty;
        public string PrimaryAnimationClipRef { get; set; } = string.Empty;
        public string FallbackActionCueCode { get; set; } = string.Empty;
    }

    public sealed class 방문자체류CardPresentation
    {
        public string PresentationStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string VisitorStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string MindTraceCode { get; set; } = string.Empty;
        public int RemainingGuestCapacity { get; set; }
        public string RequiredHCapability { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string PrimaryAssetCandidateRef { get; set; } = string.Empty;
        public string AlternativeAssetCandidateRef { get; set; } = string.Empty;
        public string FallbackVisualKey { get; set; } = string.Empty;
        public string InteractionAnchorCode { get; set; } = string.Empty;
        public string CandidateRevisionOrFingerprint { get; set; }
            = string.Empty;
        public string AnimationRoleCode { get; set; } = string.Empty;
        public string ActionCueCode { get; set; } = string.Empty;
        public string PrimaryAnimationClipRef { get; set; } = string.Empty;
        public string FallbackActionCueCode { get; set; } = string.Empty;
        public bool UsesRootMotion { get; set; }
        public bool CanRequestPreview { get; set; }
        public bool CanConfirmAuthority { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class 방문자체류PresentationPreparation
    {
        public string PlanStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public long SourceRevision { get; set; }
        public string CandidateRevision { get; set; } = string.Empty;
        public 방문자체류CardPresentation[] Visitors { get; set; }
            = Array.Empty<방문자체류CardPresentation>();
        public bool PresentationOnly { get; set; } = true;
        public bool MutatesCanonicalState { get; set; }
        public string PlanHashSha256 { get; set; } = string.Empty;
    }

    /// <summary>
    /// 권위 방문자 응대 카드를 상태별 H 기준점, VisualKey와 보유 자산
    /// 후보에 결속한다. 실제 Prefab·Scene·좌표·Collider를 만들지 않고
    /// Confirm이나 WorldRevision 변경도 수행하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "방문자 체류 카드의 판독 순간, 상태별 H 기준점, VisualKey와 fallback을 구현 준비 계획으로 결속한다.",
        WorkOrderIds = new[] { "E7-WO-NATURE-CAMP-VISITOR-STAY" },
        WorldInteractionIds = new[] { "WI-COMMUNITY-VISITOR-STAY" },
        Boundary = "E4 준비 계획은 실제 방문자 이동·Prefab·Scene·Collider·입력·Game View 또는 체류 Confirm 증거가 아니다.")]
    public sealed class 방문자체류PresentationPreparationProjector
    {
        /// <summary>
        /// Accepted 연구 r1/r2의 상태별 key/role/cue 대응을 확인한 뒤 기존 Project를 소비한다.
        /// 제공되지 않은 상태 Binding의 명시적 primitive fallback은 유지한다. 문자열 일치는
        /// 승인·파일/GUID·Rig/Clip 조회 또는 실제 동작/Scene/E5 성공의 증거가 아니다.
        /// </summary>
        public 방문자체류PresentationPreparation ProjectWithStateBindingValidation(
            string worldStableId,
            IEnumerable<Simulation공동체방문자응대CardSnapshot> cards,
            IEnumerable<방문자체류VisualBinding> visualBindings)
        {
            if (string.IsNullOrWhiteSpace(worldStableId))
                throw new InvalidOperationException("CommunityVisitorPresentationWorldInvalid");
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (visualBindings == null) throw new ArgumentNullException(nameof(visualBindings));
            var cardValues = cards.ToArray();
            var bindingValues = visualBindings.ToArray();
            ValidateCards(cardValues);
            ValidateBindings(bindingValues);
            foreach (var binding in bindingValues)
            {
                string visualKey;
                string cue;
                switch (binding.StatusCode)
                {
                    case Simulation공동체방문자체류Codes.결정대기:
                        visualKey = 방문자체류PresentationCodes.WaitingVisualKey;
                        cue = "Visitor.Waiting.Greet";
                        break;
                    case Simulation공동체방문자체류Codes.임시체류:
                        visualKey = 방문자체류PresentationCodes.AcceptedVisualKey;
                        cue = "Visitor.State.IdleOrDepart";
                        break;
                    case Simulation공동체방문자체류Codes.거절:
                        visualKey = 방문자체류PresentationCodes.RejectedVisualKey;
                        cue = "Visitor.State.IdleOrDepart";
                        break;
                    default:
                        throw new InvalidOperationException("CommunityVisitorBindingStateUnsupported");
                }
                if (!string.Equals(binding.VisualKey, visualKey, StringComparison.Ordinal))
                    throw new InvalidOperationException("CommunityVisitorBindingVisualKeyMismatch");
                if (!string.Equals(binding.AnimationRoleCode, "VisitorArrival", StringComparison.Ordinal))
                    throw new InvalidOperationException("CommunityVisitorBindingAnimationRoleMismatch");
                if (!string.Equals(binding.ActionCueCode, cue, StringComparison.Ordinal))
                    throw new InvalidOperationException("CommunityVisitorBindingActionCueMismatch");
            }
            return Project(worldStableId, cardValues, bindingValues);
        }

        public 방문자체류PresentationPreparation Project(
            string worldStableId,
            IEnumerable<Simulation공동체방문자응대CardSnapshot> cards,
            IEnumerable<방문자체류VisualBinding> visualBindings)
        {
            if (string.IsNullOrWhiteSpace(worldStableId))
                throw new InvalidOperationException(
                    "CommunityVisitorPresentationWorldInvalid");
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));
            if (visualBindings == null)
                throw new ArgumentNullException(nameof(visualBindings));

            var cardValues = cards.ToArray();
            var bindingValues = visualBindings.ToArray();
            ValidateCards(cardValues);
            ValidateBindings(bindingValues);
            var sourceRevision = cardValues.Length == 0
                ? 0
                : cardValues[0].SourceWorldRevision;
            if (cardValues.Any(value =>
                    value.SourceWorldRevision != sourceRevision))
                throw new InvalidOperationException(
                    "CommunityVisitorPresentationRevisionMixed");

            var bindingByStatus = bindingValues.ToDictionary(
                value => value.StatusCode, StringComparer.Ordinal);
            var visitors = cardValues
                .OrderBy(value => value.VisitorStableId,
                    StringComparer.Ordinal)
                .Select(value => CreateVisitor(value,
                    bindingByStatus.TryGetValue(value.StatusCode,
                        out var binding) ? binding : null))
                .ToArray();

            var result = new 방문자체류PresentationPreparation
            {
                PlanStableId = "presentation-plan:community-visitor-stay:"
                    + worldStableId,
                WorldStableId = worldStableId,
                SourceRevision = sourceRevision,
                CandidateRevision =
                    방문자체류PresentationCodes.CandidateRevision,
                Visitors = visitors,
                PresentationOnly = true,
                MutatesCanonicalState = false,
            };
            result.PlanHashSha256 = ComputeHash(result);
            return result;
        }

        private static 방문자체류CardPresentation CreateVisitor(
            Simulation공동체방문자응대CardSnapshot card,
            방문자체류VisualBinding? binding)
        {
            var fallback = binding?.FallbackVisualKey
                ?? 방문자체류PresentationCodes.FallbackVisualKey;
            return new 방문자체류CardPresentation
            {
                PresentationStableId = "presentation:community-visitor:"
                    + card.VisitorStableId,
                CardStableId = card.CardStableId,
                VisitorStableId = card.VisitorStableId,
                StatusCode = card.StatusCode,
                MindTraceCode = card.MindTraceCode,
                RemainingGuestCapacity = card.RemainingGuestCapacity,
                RequiredHCapability = RequiredHCapability(card.StatusCode),
                VisualKey = binding?.VisualKey ?? fallback,
                PrimaryAssetCandidateRef =
                    binding?.PrimaryAssetCandidateRef ?? string.Empty,
                AlternativeAssetCandidateRef =
                    binding?.AlternativeAssetCandidateRef ?? string.Empty,
                FallbackVisualKey = fallback,
                InteractionAnchorCode =
                    방문자체류PresentationCodes.InteractionAnchorCode,
                CandidateRevisionOrFingerprint =
                    binding?.CandidateRevisionOrFingerprint
                    ?? "fallback:community-visitor-marker.r1",
                AnimationRoleCode = binding?.AnimationRoleCode
                    ?? "VisitorStaticMarker",
                ActionCueCode = binding?.ActionCueCode
                    ?? "Visitor.State.Static",
                PrimaryAnimationClipRef =
                    binding?.PrimaryAnimationClipRef ?? string.Empty,
                FallbackActionCueCode = binding?.FallbackActionCueCode
                    ?? "Visitor.State.Static",
                UsesRootMotion = false,
                CanRequestPreview = string.Equals(card.StatusCode,
                    Simulation공동체방문자체류Codes.결정대기,
                    StringComparison.Ordinal),
                CanConfirmAuthority = false,
                PresentationOnly = true,
            };
        }

        private static string RequiredHCapability(string statusCode)
        {
            if (string.Equals(statusCode,
                Simulation공동체방문자체류Codes.결정대기,
                StringComparison.Ordinal))
                return 방문자체류PresentationCodes.VisitorWaitingAnchor;
            if (string.Equals(statusCode,
                Simulation공동체방문자체류Codes.임시체류,
                StringComparison.Ordinal))
                return 방문자체류PresentationCodes.GuestRestAnchor;
            return 방문자체류PresentationCodes.VisitorDepartureAnchor;
        }

        private static void ValidateCards(
            Simulation공동체방문자응대CardSnapshot[] values)
        {
            var validStates = new[]
            {
                Simulation공동체방문자체류Codes.결정대기,
                Simulation공동체방문자체류Codes.임시체류,
                Simulation공동체방문자체류Codes.거절,
            };
            if (values.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.CardStableId)
                    || string.IsNullOrWhiteSpace(value.VisitorStableId)
                    || value.SourceWorldRevision < 0
                    || value.RemainingGuestCapacity < 0
                    || !validStates.Contains(value.StatusCode,
                        StringComparer.Ordinal))
                || values.Select(value => value.VisitorStableId)
                    .Distinct(StringComparer.Ordinal).Count() != values.Length
                || values.Select(value => value.CardStableId)
                    .Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException(
                    "CommunityVisitorPresentationCardInvalid");
        }

        private static void ValidateBindings(
            방문자체류VisualBinding[] values)
        {
            if (values.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.StatusCode)
                    || string.IsNullOrWhiteSpace(value.VisualKey)
                    || string.IsNullOrWhiteSpace(
                        value.PrimaryAssetCandidateRef)
                    || string.IsNullOrWhiteSpace(
                        value.AlternativeAssetCandidateRef)
                    || string.IsNullOrWhiteSpace(value.FallbackVisualKey)
                    || string.IsNullOrWhiteSpace(
                        value.CandidateRevisionOrFingerprint)
                    || string.IsNullOrWhiteSpace(value.AnimationRoleCode)
                    || string.IsNullOrWhiteSpace(value.ActionCueCode)
                    || string.IsNullOrWhiteSpace(
                        value.PrimaryAnimationClipRef)
                    || string.IsNullOrWhiteSpace(
                        value.FallbackActionCueCode))
                || values.Select(value => value.StatusCode)
                    .Distinct(StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException(
                    "CommunityVisitorVisualBindingInvalid");
        }

        private static string ComputeHash(
            방문자체류PresentationPreparation value)
        {
            var text = new StringBuilder()
                .Append(value.PlanStableId).Append('|')
                .Append(value.WorldStableId).Append('|')
                .Append(value.SourceRevision).Append('|')
                .Append(value.CandidateRevision).Append('|')
                .Append(value.PresentationOnly).Append('|')
                .Append(value.MutatesCanonicalState);
            foreach (var visitor in value.Visitors)
                text.Append('\n').Append(visitor.PresentationStableId)
                    .Append('|').Append(visitor.CardStableId)
                    .Append('|').Append(visitor.VisitorStableId)
                    .Append('|').Append(visitor.StatusCode)
                    .Append('|').Append(visitor.MindTraceCode)
                    .Append('|').Append(visitor.RemainingGuestCapacity)
                    .Append('|').Append(visitor.RequiredHCapability)
                    .Append('|').Append(visitor.VisualKey)
                    .Append('|').Append(visitor.PrimaryAssetCandidateRef)
                    .Append('|').Append(visitor.AlternativeAssetCandidateRef)
                    .Append('|').Append(visitor.FallbackVisualKey)
                    .Append('|').Append(visitor.InteractionAnchorCode)
                    .Append('|').Append(
                        visitor.CandidateRevisionOrFingerprint)
                    .Append('|').Append(visitor.AnimationRoleCode)
                    .Append('|').Append(visitor.ActionCueCode)
                    .Append('|').Append(visitor.PrimaryAnimationClipRef)
                    .Append('|').Append(visitor.FallbackActionCueCode)
                    .Append('|').Append(visitor.UsesRootMotion)
                    .Append('|').Append(visitor.CanRequestPreview)
                    .Append('|').Append(visitor.CanConfirmAuthority)
                    .Append('|').Append(visitor.PresentationOnly);
            using var sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    text.ToString())).Select(value => value.ToString("x2")));
        }
    }
}
