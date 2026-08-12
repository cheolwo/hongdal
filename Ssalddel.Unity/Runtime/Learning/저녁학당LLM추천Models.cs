using System;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Learning
{
    public static class 오전행동TagCodes
    {
        public const string UnknownSkipped = "UnknownSkipped";
        public const string EvidenceChecked = "EvidenceChecked";
        public const string CargoLoaded = "CargoLoaded";
        public const string JourneyStarted = "JourneyStarted";
        public const string CompetingForces = "CompetingForces";
    }

    public sealed class 오전행동Summary
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string[] OutcomeTags { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당LLM추천Request
    {
        public string StableId { get; set; } = string.Empty;
        public long MorningLedgerRevision { get; set; }
        public 오전행동Summary[] MorningActions { get; set; } = Array.Empty<오전행동Summary>();
        public string[] AllowedContentStableIds { get; set; } = Array.Empty<string>();
        public string Instruction { get; set; } = string.Empty;
    }

    public sealed class 저녁학당LLM추천Response
    {
        public string RequestStableId { get; set; } = string.Empty;
        public string RecommendedContentStableId { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public string[] ReferencedMorningActionStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 저녁학당추천Decision
    {
        public string ContentStableId { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public bool UsedLlm { get; set; }
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public interface I저녁학당LLM추천Provider
    {
        저녁학당LLM추천Response Recommend(저녁학당LLM추천Request request);
    }

    public sealed class 저녁학당추천Engine
    {
        public 저녁학당LLM추천Request CreateRequest(
            long morningLedgerRevision,
            오전행동Summary[] morningActions,
            저녁학당콘텐츠Snapshot[] catalog)
        {
            ValidateInputs(morningLedgerRevision, morningActions, catalog);
            return new 저녁학당LLM추천Request
            {
                StableId = "evening-recommendation-request:morning.r" + morningLedgerRevision,
                MorningLedgerRevision = morningLedgerRevision,
                MorningActions = morningActions.ToArray(),
                AllowedContentStableIds = catalog.Select(value => value.StableId).ToArray(),
                Instruction = "오전 행동과 직접 관련된 학습 콘텐츠 하나를 허용 목록에서 고르고, "
                    + "참조한 행동 stable ID와 이유만 반환한다. 스탯 수치나 규칙 효과는 만들지 않는다.",
            };
        }

        public 저녁학당추천Decision Accept(
            저녁학당LLM추천Request request,
            저녁학당LLM추천Response response)
        {
            if (request == null || response == null || response.RequestStableId != request.StableId
                || !request.AllowedContentStableIds.Contains(response.RecommendedContentStableId)
                || string.IsNullOrWhiteSpace(response.Rationale)
                || response.ReferencedMorningActionStableIds == null
                || response.ReferencedMorningActionStableIds.Length == 0
                || response.ReferencedMorningActionStableIds.Any(id =>
                    !request.MorningActions.Any(action => action.StableId == id)))
                throw new InvalidOperationException("EveningRecommendationLlmResponseInvalid");

            return new 저녁학당추천Decision
            {
                ContentStableId = response.RecommendedContentStableId,
                Rationale = response.Rationale.Trim(),
                UsedLlm = true,
                SourceStableIds = response.ReferencedMorningActionStableIds.ToArray(),
            };
        }

        public 저녁학당추천Decision Fallback(저녁학당LLM추천Request request)
        {
            if (request == null || request.MorningActions == null || request.MorningActions.Length == 0)
                throw new InvalidOperationException("EveningRecommendationRequestInvalid");
            var journey = request.MorningActions.Any(action => action.OutcomeTags.Any(tag =>
                tag == 오전행동TagCodes.CargoLoaded || tag == 오전행동TagCodes.JourneyStarted
                || tag == 오전행동TagCodes.CompetingForces));
            var preferred = journey
                ? "learning:hongik.chariot.integrated-progress"
                : 저녁학당SimulationFixture.FoolContentStableId;
            if (!request.AllowedContentStableIds.Contains(preferred))
                preferred = request.AllowedContentStableIds.First();
            return new 저녁학당추천Decision
            {
                ContentStableId = preferred,
                Rationale = journey
                    ? "오전의 상차·이동 행동에 지혜와 힘을 통합하는 전차 학습을 연결했습니다."
                    : "오전에 건너뛴 불확실성을 다시 보기 위해 바보의 '모를 뿐' 학습을 연결했습니다.",
                UsedLlm = false,
                SourceStableIds = request.MorningActions.Select(value => value.StableId).ToArray(),
            };
        }

        private static void ValidateInputs(long revision, 오전행동Summary[] actions,
            저녁학당콘텐츠Snapshot[] catalog)
        {
            if (revision <= 0 || actions == null || actions.Length == 0 || catalog == null
                || catalog.Length == 0 || actions.Any(action => action == null
                    || !StableDataId.IsValid(action.StableId) || action.Revision <= 0
                    || action.OccurredAt == default || string.IsNullOrWhiteSpace(action.ActionCode)
                    || string.IsNullOrWhiteSpace(action.Summary) || action.OutcomeTags == null
                    || action.OutcomeTags.Length == 0 || action.OutcomeTags.Any(string.IsNullOrWhiteSpace)
                    || action.SourceStableIds == null || action.SourceStableIds.Length == 0
                    || action.SourceStableIds.Any(id => !StableDataId.IsValid(id))))
                throw new InvalidOperationException("EveningRecommendationInputInvalid");
        }
    }
}
