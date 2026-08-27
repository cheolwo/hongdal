using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation학습자료승인Pipeline
    {
        public Simulation학습해석후보Snapshot CreateCandidate(
            SimulationYouTube학습원문관측Snapshot observation,
            Simulation학습해석후보Snapshot draft)
        {
            ValidateObservation(observation);
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (string.IsNullOrWhiteSpace(draft.후보StableId)
                || string.IsNullOrWhiteSpace(draft.요약)
                || (draft.성찰질문들?.Length ?? 0) == 0
                || string.IsNullOrWhiteSpace(draft.해석RuleRevision)
                || !IsAllowedPair(draft.분류Code, draft.제안내면능력치Code,
                    draft.제안내면효과Code))
                throw new SimulationContractException(
                    "SimulationLearningInterpretationCandidateInvalid");

            var candidate = new Simulation학습해석후보Snapshot
            {
                후보StableId = draft.후보StableId.Trim(),
                원문관측StableId = observation.관측StableId.Trim(),
                원문관측HashSha256 = observation.원문MetadataHashSha256,
                분류Code = draft.분류Code,
                요약 = draft.요약.Trim(),
                성찰질문들 = draft.성찰질문들.Select(value => value.Trim()).ToArray(),
                제안내면능력치Code = draft.제안내면능력치Code,
                제안내면효과Code = draft.제안내면효과Code,
                해석RuleRevision = draft.해석RuleRevision.Trim(),
                상태Code = Simulation학습자료상태Codes.Candidate,
            };
            candidate.InputHashSha256 = CalculateCandidateHash(candidate);
            return candidate;
        }

        public Simulation승인학습자료Publication Approve(
            SimulationYouTube학습원문관측Snapshot observation,
            Simulation학습해석후보Snapshot candidate,
            string publicationStableId,
            string revision,
            string 승인자StableId,
            DateTimeOffset 승인시각)
        {
            ValidateObservation(observation);
            ValidateCandidate(candidate);
            if (!string.Equals(candidate.원문관측StableId,
                    observation.관측StableId, StringComparison.Ordinal)
                || !string.Equals(candidate.원문관측HashSha256,
                    observation.원문MetadataHashSha256, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(publicationStableId)
                || string.IsNullOrWhiteSpace(revision)
                || string.IsNullOrWhiteSpace(승인자StableId)
                || 승인시각 == default(DateTimeOffset))
                throw new SimulationContractException(
                    "SimulationLearningPublicationApprovalInvalid");

            var publication = new Simulation승인학습자료Publication
            {
                PublicationStableId = publicationStableId.Trim(),
                Revision = revision.Trim(),
                제목 = observation.제목.Trim(),
                분류Code = candidate.분류Code,
                요약 = candidate.요약,
                성찰질문들 = candidate.성찰질문들.ToArray(),
                내면능력치Code = candidate.제안내면능력치Code,
                능력치증가량 = 1,
                내면효과Code = candidate.제안내면효과Code,
                원문관측StableId = observation.관측StableId,
                원문관측HashSha256 = observation.원문MetadataHashSha256,
                SourceUrl = observation.SourceUrl,
                승인자StableId = 승인자StableId.Trim(),
                승인시각 = 승인시각,
                상태Code = Simulation학습자료상태Codes.Approved,
                InputHashSha256 = candidate.InputHashSha256,
                이용한계 = observation.이용한계,
            };
            publication.PublicationHashSha256 =
                Simulation거점성찰Rules.CalculatePublicationHash(publication);
            Simulation거점성찰Rules.ValidatePublication(publication);
            return publication;
        }

        private static void ValidateObservation(
            SimulationYouTube학습원문관측Snapshot observation)
        {
            if (observation == null)
                throw new ArgumentNullException(nameof(observation));
            if (!string.Equals(observation.SchemaCode,
                    Simulation거점성찰SchemaCodes.YouTube학습원문관측,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(observation.관측StableId)
                || string.IsNullOrWhiteSpace(observation.VideoStableId)
                || string.IsNullOrWhiteSpace(observation.SourceUrl)
                || string.IsNullOrWhiteSpace(observation.제목)
                || observation.조회시각 == default(DateTimeOffset)
                || string.IsNullOrWhiteSpace(observation.수집AdapterCode)
                || string.IsNullOrWhiteSpace(observation.이용한계)
                || !IsSha256(observation.원문MetadataHashSha256)
                || (observation.근거구간들?.Length ?? 0) == 0
                || observation.근거구간들.Any(value => value.시작Millisecond < 0
                    || value.종료Millisecond <= value.시작Millisecond
                    || string.IsNullOrWhiteSpace(value.근거요약)
                    || !IsSha256(value.구간HashSha256)))
                throw new SimulationContractException(
                    "SimulationYouTubeLearningObservationInvalid");
        }

        private static void ValidateCandidate(
            Simulation학습해석후보Snapshot candidate)
        {
            if (candidate == null
                || !string.Equals(candidate.SchemaCode,
                    Simulation거점성찰SchemaCodes.학습해석후보,
                    StringComparison.Ordinal)
                || !string.Equals(candidate.상태Code,
                    Simulation학습자료상태Codes.Candidate,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(candidate.후보StableId)
                || string.IsNullOrWhiteSpace(candidate.원문관측StableId)
                || !IsSha256(candidate.원문관측HashSha256)
                || string.IsNullOrWhiteSpace(candidate.요약)
                || (candidate.성찰질문들?.Length ?? 0) == 0
                || string.IsNullOrWhiteSpace(candidate.해석RuleRevision)
                || !IsAllowedPair(candidate.분류Code,
                    candidate.제안내면능력치Code,
                    candidate.제안내면효과Code)
                || !string.Equals(candidate.InputHashSha256,
                    CalculateCandidateHash(candidate), StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningInterpretationCandidateInvalid");
        }

        private static string CalculateCandidateHash(
            Simulation학습해석후보Snapshot value)
            => Simulation거점성찰Rules.Hash(string.Join("|", new[]
            {
                value.SchemaCode,
                value.후보StableId,
                value.원문관측StableId,
                value.원문관측HashSha256,
                value.분류Code,
                value.요약,
                string.Join("\u001f", value.성찰질문들 ?? Array.Empty<string>()),
                value.제안내면능력치Code,
                value.제안내면효과Code,
                value.해석RuleRevision,
                value.상태Code,
            }));

        private static bool IsAllowedPair(string category, string stat, string effect)
            => string.Equals(category, Simulation학습분류Codes.상황인식,
                       StringComparison.Ordinal)
                    && string.Equals(stat, Simulation내면능력치Codes.알아차림,
                        StringComparison.Ordinal)
                    && string.Equals(effect, Simulation내면효과Codes.초심,
                        StringComparison.Ordinal)
                || string.Equals(category, Simulation학습분류Codes.통합실천,
                       StringComparison.Ordinal)
                    && string.Equals(stat, Simulation내면능력치Codes.결의,
                        StringComparison.Ordinal)
                    && string.Equals(effect,
                        Simulation내면효과Codes.통합진전,
                        StringComparison.Ordinal);

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
                && value.All(character => character >= '0' && character <= '9'
                    || character >= 'a' && character <= 'f');
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Domain,
        "승인 학습자료를 멱등 동기화하고 Simulation 세션용 불변 사본을 만든다.",
        StepKey = "domain.approved-learning-ledger",
        DependsOnStepKeys = new[] { "contract.base-reflection-learning-material" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 20,
        Boundary = "Provider를 호출하거나 운영 DB를 쓰지 않고 사람 승인 Publication만 Simulation 파생 원장에 보관한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "승인 자료를 멱등 동기화하고 세션이 동결할 불변 사본을 제공한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-REFLECT-01" },
        WorkOrderIds = new[] { "E9-WO-NATURE-BASE-REFLECTION" },
        Boundary = "파생 원장 준비는 거점 성찰 실행이나 E3 회귀 증거를 대신하지 않는다.")]
    public sealed class Simulation승인학습자료파생원장
    {
        private readonly Dictionary<string, Simulation승인학습자료Publication> publications =
            new Dictionary<string, Simulation승인학습자료Publication>(StringComparer.Ordinal);

        public string LedgerRevision { get; private set; } = string.Empty;
        public string InputHashSha256 { get; private set; } = string.Empty;

        public bool Synchronize(Simulation승인학습자료동기화Bundle bundle)
        {
            Simulation거점성찰Rules.ValidateBundle(bundle);
            var calculatedHash = Simulation거점성찰Rules.CalculateBundleInputHash(bundle);
            if (!string.Equals(calculatedHash, bundle.InputHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationApprovedLearningBundleHashMismatch");

            var changed = false;
            foreach (var publication in bundle.Publications.OrderBy(
                         value => value.PublicationStableId, StringComparer.Ordinal)
                     .ThenBy(value => value.Revision, StringComparer.Ordinal))
            {
                var key = BuildKey(publication.PublicationStableId, publication.Revision);
                if (publications.TryGetValue(key, out var current))
                {
                    if (!string.Equals(current.PublicationHashSha256,
                            publication.PublicationHashSha256,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationApprovedLearningRevisionConflict");
                    continue;
                }

                publications.Add(key, Simulation거점성찰Cloner.Clone(publication));
                changed = true;
            }

            if (!changed && publications.Count > 0
                && !string.Equals(InputHashSha256, bundle.InputHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationApprovedLearningLedgerInputConflict");

            LedgerRevision = bundle.LedgerRevision.Trim();
            InputHashSha256 = bundle.InputHashSha256;
            return changed;
        }

        public Simulation승인학습자료동기화Bundle Freeze()
        {
            if (string.IsNullOrWhiteSpace(LedgerRevision)
                || string.IsNullOrWhiteSpace(InputHashSha256))
                throw new SimulationConflictException(
                    "SimulationApprovedLearningLedgerUnavailable");

            return new Simulation승인학습자료동기화Bundle
            {
                SchemaCode = Simulation거점성찰SchemaCodes.파생원장,
                LedgerRevision = LedgerRevision,
                InputHashSha256 = InputHashSha256,
                Publications = publications.Values.OrderBy(
                        value => value.PublicationStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.Revision, StringComparer.Ordinal)
                    .Select(Simulation거점성찰Cloner.Clone).ToArray(),
            };
        }

        private static string BuildKey(string stableId, string revision)
            => stableId.Trim() + "|" + revision.Trim();
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Domain,
        "WI-REFLECT-01의 Preview, Confirm, 다음 활동 적용을 결정적으로 처리한다.",
        StepKey = "domain.base-reflection",
        DependsOnStepKeys = new[] { "domain.approved-learning-ledger" },
        ExecutionStage = SsalddelCodeExecutionStage.Tick,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "보상은 게임 안 성찰 선택에만 귀속하며 영상 재생·시청 시간·외부 Provider 결과를 읽지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "WI-REFLECT-01의 Preview, Confirm과 다음 활동 적용을 권위 상태에서 처리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-REFLECT-01" },
        WorkOrderIds = new[] { "E9-WO-NATURE-BASE-REFLECTION" },
        Boundary = "도메인 실행은 Unity H1 상호작용, 실제 입력 또는 E7 폐루프 증거를 소유하지 않는다.")]
    public sealed class Simulation거점성찰Engine
    {
        private Simulation거점성찰StateSnapshot state;

        public Simulation거점성찰Engine(Simulation거점성찰InitialStateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            Simulation거점성찰Rules.ValidatePlayerStableId(request.PlayerStableId);
            if (request.시작일차 < 1)
                throw new SimulationContractException("SimulationReflectionDayInvalid");
            Simulation거점성찰Rules.ValidateBundle(request.승인자료묶음);
            var bundleHash = Simulation거점성찰Rules.CalculateBundleInputHash(
                request.승인자료묶음);
            if (!string.Equals(bundleHash, request.승인자료묶음.InputHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationApprovedLearningBundleHashMismatch");

            state = new Simulation거점성찰StateSnapshot
            {
                PlayerStableId = request.PlayerStableId.Trim(),
                현재일차 = request.시작일차,
                FrozenLedgerRevision = request.승인자료묶음.LedgerRevision.Trim(),
                FrozenLedgerInputHashSha256 = request.승인자료묶음.InputHashSha256,
                FrozenPublications = request.승인자료묶음.Publications
                    .OrderBy(value => value.PublicationStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.Revision, StringComparer.Ordinal)
                    .Select(Simulation거점성찰Cloner.Clone).ToArray(),
                내면상태 = Simulation거점성찰Cloner.Clone(request.내면상태),
            };
            RefreshHash();
        }

        private Simulation거점성찰Engine(Simulation거점성찰StateSnapshot snapshot)
        {
            state = Simulation거점성찰Cloner.Clone(snapshot);
            var expectedHash = Simulation거점성찰StateHasher.Calculate(state);
            if (!string.Equals(expectedHash, state.StateHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationReflectionStateHashMismatch");
            ValidateFrozenPublications();
        }

        public static Simulation거점성찰Engine Restore(
            Simulation거점성찰StateSnapshot snapshot)
            => new Simulation거점성찰Engine(snapshot
                ?? throw new ArgumentNullException(nameof(snapshot)));

        public Simulation거점성찰Preview Preview(
            Simulation거점성찰PreviewRequest request)
        {
            ValidatePreviewRequest(request);
            var publication = ResolvePublication(request);
            var 보상적용가능 = string.Equals(request.선택StableId,
                Simulation거점성찰선택Codes.오늘행동성찰, StringComparison.Ordinal);

            if (보상적용가능)
            {
                if (state.성찰완료일차들.Contains(request.일차))
                    throw new SimulationConflictException(
                        "SimulationReflectionDailyLimitReached");
                if (state.Grants.Any(value =>
                        string.Equals(value.PublicationStableId,
                            publication!.PublicationStableId, StringComparison.Ordinal)
                        && string.Equals(value.PublicationRevision,
                            publication.Revision, StringComparison.Ordinal)))
                    throw new SimulationConflictException(
                        "SimulationReflectionPublicationAlreadyGranted");
            }

            var preview = new Simulation거점성찰Preview
            {
                ExpectedRevision = state.Revision,
                PlayerStableId = state.PlayerStableId,
                일차 = request.일차,
                선택StableId = request.선택StableId,
                PublicationStableId = publication?.PublicationStableId ?? string.Empty,
                PublicationRevision = publication?.Revision ?? string.Empty,
                PublicationHashSha256 = publication?.PublicationHashSha256 ?? string.Empty,
                보상적용가능 = 보상적용가능,
                내면능력치Code = 보상적용가능
                    ? publication!.내면능력치Code : string.Empty,
                능력치증가량 = 보상적용가능 ? publication!.능력치증가량 : 0,
                내면효과Code = 보상적용가능
                    ? publication!.내면효과Code : string.Empty,
                결과Code = 보상적용가능
                    ? Simulation거점성찰결과Codes.다음활동적용대기
                    : string.Equals(request.선택StableId,
                        Simulation거점성찰선택Codes.원문열기,
                        StringComparison.Ordinal)
                        ? Simulation거점성찰결과Codes.원문확인가능
                        : Simulation거점성찰결과Codes.휴식함,
                설명Codes = 보상적용가능
                    ? new[] { "RewardBoundToInGameReflection", "VideoTelemetryIgnored" }
                    : new[] { "NoLearningReward" },
            };
            preview.PreviewStableId = "reflection-preview:"
                + Simulation거점성찰Rules.Hash(Simulation거점성찰Rules
                    .BuildPreviewPayload(preview)).Substring(0, 24);
            return preview;
        }

        public Simulation거점성찰StateSnapshot Confirm(
            Simulation거점성찰ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.CommandId))
                throw new SimulationContractException("SimulationCommandIdRequired");
            if (state.적용CommandIds.Contains(request.CommandId, StringComparer.Ordinal))
                return Snapshot();
            if (request.ExpectedRevision != state.Revision
                || request.Preview.ExpectedRevision != state.Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");

            var expected = Preview(new Simulation거점성찰PreviewRequest
            {
                ExpectedRevision = request.ExpectedRevision,
                PlayerStableId = request.Preview.PlayerStableId,
                일차 = request.Preview.일차,
                선택StableId = request.Preview.선택StableId,
                PublicationStableId = request.Preview.PublicationStableId,
                PublicationRevision = request.Preview.PublicationRevision,
            });
            if (!string.Equals(expected.PreviewStableId,
                    request.Preview.PreviewStableId, StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationReflectionPreviewMismatch");

            if (expected.보상적용가능)
            {
                var grantKey = expected.PublicationStableId + "|"
                    + expected.PublicationRevision + "|" + state.PlayerStableId;
                state.Grants = state.Grants.Concat(new[]
                    {
                        new Simulation거점성찰GrantSnapshot
                        {
                            GrantStableId = "reflection-grant:"
                                + Simulation거점성찰Rules.Hash(grantKey).Substring(0, 24),
                            PlayerStableId = state.PlayerStableId,
                            선택일차 = expected.일차,
                            PublicationStableId = expected.PublicationStableId,
                            PublicationRevision = expected.PublicationRevision,
                            PublicationHashSha256 = expected.PublicationHashSha256,
                            내면능력치Code = expected.내면능력치Code,
                            능력치증가량 = expected.능력치증가량,
                            내면효과Code = expected.내면효과Code,
                            상태Code = Simulation거점성찰결과Codes.다음활동적용대기,
                        },
                    }).ToArray();
                state.성찰완료일차들 = state.성찰완료일차들.Concat(
                    new[] { expected.일차 }).OrderBy(value => value).ToArray();
            }

            state.적용CommandIds = state.적용CommandIds.Concat(
                new[] { request.CommandId.Trim() }).ToArray();
            state.Revision++;
            RefreshHash();
            return Snapshot();
        }

        public Simulation거점성찰StateSnapshot ApplyAtNextActivity(
            string commandId,
            long expectedRevision,
            int 다음일차)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                throw new SimulationContractException("SimulationCommandIdRequired");
            if (state.적용CommandIds.Contains(commandId, StringComparer.Ordinal))
                return Snapshot();
            if (expectedRevision != state.Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");
            if (다음일차 < state.현재일차)
                throw new SimulationContractException("SimulationReflectionDayInvalid");

            var pending = state.Grants.Where(value => string.Equals(value.상태Code,
                    Simulation거점성찰결과Codes.다음활동적용대기,
                    StringComparison.Ordinal))
                .OrderBy(value => value.GrantStableId, StringComparer.Ordinal).ToArray();
            foreach (var grant in pending)
            {
                ApplyGrant(grant);
                grant.상태Code = Simulation거점성찰결과Codes.내면학습적용;
            }

            state.현재일차 = 다음일차;
            state.적용CommandIds = state.적용CommandIds.Concat(
                new[] { commandId.Trim() }).ToArray();
            state.Revision++;
            RefreshHash();
            return Snapshot();
        }

        public Simulation거점성찰StateSnapshot Snapshot()
            => Simulation거점성찰Cloner.Clone(state);

        private void ApplyGrant(Simulation거점성찰GrantSnapshot grant)
        {
            if (string.Equals(grant.내면능력치Code,
                    Simulation내면능력치Codes.알아차림, StringComparison.Ordinal))
                state.내면상태.알아차림 += grant.능력치증가량;
            else if (string.Equals(grant.내면능력치Code,
                         Simulation내면능력치Codes.결의, StringComparison.Ordinal))
                state.내면상태.결의 += grant.능력치증가량;
            else
                throw new SimulationContractException(
                    "SimulationReflectionEffectNotAllowed");

            if (!state.내면상태.획득내면효과Codes.Contains(
                    grant.내면효과Code, StringComparer.Ordinal))
                state.내면상태.획득내면효과Codes = state.내면상태
                    .획득내면효과Codes.Concat(new[] { grant.내면효과Code })
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
        }

        private Simulation승인학습자료Publication? ResolvePublication(
            Simulation거점성찰PreviewRequest request)
        {
            if (string.Equals(request.선택StableId,
                    Simulation거점성찰선택Codes.그냥휴식, StringComparison.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(request.PublicationStableId)
                    || !string.IsNullOrWhiteSpace(request.PublicationRevision))
                    throw new SimulationContractException(
                        "SimulationReflectionRestMaterialNotAllowed");
                return null;
            }

            if (!string.Equals(request.선택StableId,
                    Simulation거점성찰선택Codes.오늘행동성찰,
                    StringComparison.Ordinal)
                && !string.Equals(request.선택StableId,
                    Simulation거점성찰선택Codes.원문열기,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationReflectionChoiceInvalid");

            var publication = state.FrozenPublications.SingleOrDefault(value =>
                string.Equals(value.PublicationStableId,
                    request.PublicationStableId?.Trim(), StringComparison.Ordinal)
                && string.Equals(value.Revision,
                    request.PublicationRevision?.Trim(), StringComparison.Ordinal));
            if (publication == null)
                throw new SimulationConflictException(
                    "SimulationApprovedLearningMaterialUnavailable");
            return publication;
        }

        private void ValidatePreviewRequest(Simulation거점성찰PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision != state.Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");
            if (!string.Equals(request.PlayerStableId?.Trim(), state.PlayerStableId,
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationReflectionPlayerMismatch");
            if (request.일차 != state.현재일차)
                throw new SimulationConflictException(
                    "SimulationReflectionDayMismatch");
        }

        private void ValidateFrozenPublications()
        {
            foreach (var publication in state.FrozenPublications)
                Simulation거점성찰Rules.ValidatePublication(publication);
        }

        private void RefreshHash()
            => state.StateHashSha256 = Simulation거점성찰StateHasher.Calculate(state);
    }

    public static class Simulation거점성찰Rules
    {
        public static void ValidateBundle(Simulation승인학습자료동기화Bundle bundle)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (!string.Equals(bundle.SchemaCode,
                    Simulation거점성찰SchemaCodes.파생원장, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(bundle.LedgerRevision)
                || !IsSha256(bundle.InputHashSha256)
                || bundle.Publications == null)
                throw new SimulationContractException(
                    "SimulationApprovedLearningBundleInvalid");
            foreach (var publication in bundle.Publications)
                ValidatePublication(publication);
            if (bundle.Publications.GroupBy(value => value.PublicationStableId.Trim()
                    + "|" + value.Revision.Trim(), StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
                throw new SimulationContractException(
                    "SimulationApprovedLearningDuplicateRevision");
        }

        public static void ValidatePublication(
            Simulation승인학습자료Publication publication)
        {
            if (publication == null)
                throw new SimulationContractException(
                    "SimulationApprovedLearningPublicationInvalid");
            var validCategory = string.Equals(publication.분류Code,
                    Simulation학습분류Codes.상황인식, StringComparison.Ordinal)
                || string.Equals(publication.분류Code,
                    Simulation학습분류Codes.통합실천, StringComparison.Ordinal);
            var validEffect = string.Equals(publication.내면능력치Code,
                        Simulation내면능력치Codes.알아차림, StringComparison.Ordinal)
                    && string.Equals(publication.내면효과Code,
                        Simulation내면효과Codes.초심, StringComparison.Ordinal)
                || string.Equals(publication.내면능력치Code,
                        Simulation내면능력치Codes.결의, StringComparison.Ordinal)
                    && string.Equals(publication.내면효과Code,
                        Simulation내면효과Codes.통합진전, StringComparison.Ordinal);
            if (!string.Equals(publication.SchemaCode,
                    Simulation거점성찰SchemaCodes.승인학습자료Publication,
                    StringComparison.Ordinal)
                && !string.Equals(publication.SchemaCode,
                    Simulation거점성찰SchemaCodes.기존학습카드Publication,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(publication.PublicationStableId)
                || string.IsNullOrWhiteSpace(publication.Revision)
                || string.IsNullOrWhiteSpace(publication.제목)
                || !validCategory || !validEffect
                || publication.능력치증가량 != 1
                || !string.Equals(publication.상태Code,
                    Simulation학습자료상태Codes.Approved, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(publication.승인자StableId)
                || publication.승인시각 == default(DateTimeOffset)
                || !IsSha256(publication.InputHashSha256)
                || !IsSha256(publication.PublicationHashSha256)
                || !string.Equals(CalculatePublicationHash(publication),
                    publication.PublicationHashSha256, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationApprovedLearningPublicationInvalid");
        }

        public static string CalculatePublicationHash(
            Simulation승인학습자료Publication value)
        {
            var canonical = new StringBuilder();
            Add(canonical, value.SchemaCode);
            Add(canonical, value.PublicationStableId);
            Add(canonical, value.Revision);
            Add(canonical, value.제목);
            Add(canonical, value.분류Code);
            Add(canonical, value.요약);
            foreach (var question in value.성찰질문들 ?? Array.Empty<string>())
                Add(canonical, question);
            Add(canonical, value.내면능력치Code);
            Add(canonical, value.능력치증가량);
            Add(canonical, value.내면효과Code);
            Add(canonical, value.원문관측StableId);
            Add(canonical, value.원문관측HashSha256);
            Add(canonical, value.SourceUrl);
            Add(canonical, value.승인자StableId);
            Add(canonical, value.승인시각.ToUniversalTime().ToString("O",
                CultureInfo.InvariantCulture));
            Add(canonical, value.상태Code);
            Add(canonical, value.InputHashSha256);
            Add(canonical, value.이용한계);
            return Hash(canonical.ToString());
        }

        public static string CalculateBundleInputHash(
            Simulation승인학습자료동기화Bundle bundle)
        {
            var canonical = new StringBuilder();
            Add(canonical, bundle.SchemaCode);
            Add(canonical, bundle.LedgerRevision);
            foreach (var publication in (bundle.Publications
                         ?? Array.Empty<Simulation승인학습자료Publication>())
                     .OrderBy(value => value.PublicationStableId, StringComparer.Ordinal)
                     .ThenBy(value => value.Revision, StringComparer.Ordinal))
            {
                Add(canonical, publication.PublicationStableId);
                Add(canonical, publication.Revision);
                Add(canonical, publication.PublicationHashSha256);
            }
            return Hash(canonical.ToString());
        }

        public static string BuildPreviewPayload(Simulation거점성찰Preview value)
            => string.Join("|", new[]
            {
                value.ExpectedRevision.ToString(CultureInfo.InvariantCulture),
                value.PlayerStableId,
                value.일차.ToString(CultureInfo.InvariantCulture),
                value.선택StableId,
                value.PublicationStableId,
                value.PublicationRevision,
                value.PublicationHashSha256,
                value.내면능력치Code,
                value.능력치증가량.ToString(CultureInfo.InvariantCulture),
                value.내면효과Code,
                value.결과Code,
            });

        public static string Hash(string value)
        {
            using (var algorithm = SHA256.Create())
            {
                return BitConverter.ToString(algorithm.ComputeHash(
                        Encoding.UTF8.GetBytes(value ?? string.Empty)))
                    .Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        public static void ValidatePlayerStableId(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                throw new SimulationContractException(
                    "SimulationReflectionPlayerRequired");
        }

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
                && value.All(character => character >= '0' && character <= '9'
                    || character >= 'a' && character <= 'f');

        internal static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture)
                ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':');
            target.Append(text);
            target.Append('|');
        }
    }

    public static class Simulation거점성찰StateHasher
    {
        public static string Calculate(Simulation거점성찰StateSnapshot value)
        {
            var canonical = new StringBuilder();
            Simulation거점성찰Rules.Add(canonical, value.SchemaCode);
            Simulation거점성찰Rules.Add(canonical, value.PlayerStableId);
            Simulation거점성찰Rules.Add(canonical, value.현재일차);
            Simulation거점성찰Rules.Add(canonical, value.Revision);
            Simulation거점성찰Rules.Add(canonical, value.FrozenLedgerRevision);
            Simulation거점성찰Rules.Add(canonical, value.FrozenLedgerInputHashSha256);
            foreach (var publication in value.FrozenPublications.OrderBy(
                         item => item.PublicationStableId, StringComparer.Ordinal)
                     .ThenBy(item => item.Revision, StringComparer.Ordinal))
                Simulation거점성찰Rules.Add(canonical,
                    publication.PublicationHashSha256);
            Simulation거점성찰Rules.Add(canonical, value.내면상태.알아차림);
            Simulation거점성찰Rules.Add(canonical, value.내면상태.결의);
            foreach (var effect in value.내면상태.획득내면효과Codes.OrderBy(
                         item => item, StringComparer.Ordinal))
                Simulation거점성찰Rules.Add(canonical, effect);
            foreach (var day in value.성찰완료일차들.OrderBy(item => item))
                Simulation거점성찰Rules.Add(canonical, day);
            foreach (var grant in value.Grants.OrderBy(
                         item => item.GrantStableId, StringComparer.Ordinal))
            {
                Simulation거점성찰Rules.Add(canonical, grant.GrantStableId);
                Simulation거점성찰Rules.Add(canonical, grant.PlayerStableId);
                Simulation거점성찰Rules.Add(canonical, grant.선택일차);
                Simulation거점성찰Rules.Add(canonical, grant.PublicationStableId);
                Simulation거점성찰Rules.Add(canonical, grant.PublicationRevision);
                Simulation거점성찰Rules.Add(canonical, grant.PublicationHashSha256);
                Simulation거점성찰Rules.Add(canonical, grant.내면능력치Code);
                Simulation거점성찰Rules.Add(canonical, grant.능력치증가량);
                Simulation거점성찰Rules.Add(canonical, grant.내면효과Code);
                Simulation거점성찰Rules.Add(canonical, grant.상태Code);
            }
            foreach (var commandId in value.적용CommandIds)
                Simulation거점성찰Rules.Add(canonical, commandId);
            return Simulation거점성찰Rules.Hash(canonical.ToString());
        }
    }

    public static class Simulation거점성찰Cloner
    {
        public static Simulation승인학습자료Publication Clone(
            Simulation승인학습자료Publication source)
            => new Simulation승인학습자료Publication
            {
                SchemaCode = source.SchemaCode,
                PublicationStableId = source.PublicationStableId,
                Revision = source.Revision,
                제목 = source.제목,
                분류Code = source.분류Code,
                요약 = source.요약,
                성찰질문들 = (source.성찰질문들 ?? Array.Empty<string>()).ToArray(),
                내면능력치Code = source.내면능력치Code,
                능력치증가량 = source.능력치증가량,
                내면효과Code = source.내면효과Code,
                원문관측StableId = source.원문관측StableId,
                원문관측HashSha256 = source.원문관측HashSha256,
                SourceUrl = source.SourceUrl,
                승인자StableId = source.승인자StableId,
                승인시각 = source.승인시각,
                상태Code = source.상태Code,
                InputHashSha256 = source.InputHashSha256,
                PublicationHashSha256 = source.PublicationHashSha256,
                이용한계 = source.이용한계,
            };

        public static Simulation내면상태Snapshot Clone(Simulation내면상태Snapshot source)
            => new Simulation내면상태Snapshot
            {
                알아차림 = source?.알아차림 ?? 0,
                결의 = source?.결의 ?? 0,
                획득내면효과Codes = source?.획득내면효과Codes?.ToArray()
                    ?? Array.Empty<string>(),
            };

        public static Simulation거점성찰StateSnapshot Clone(
            Simulation거점성찰StateSnapshot source)
            => new Simulation거점성찰StateSnapshot
            {
                SchemaCode = source.SchemaCode,
                PlayerStableId = source.PlayerStableId,
                현재일차 = source.현재일차,
                Revision = source.Revision,
                FrozenLedgerRevision = source.FrozenLedgerRevision,
                FrozenLedgerInputHashSha256 = source.FrozenLedgerInputHashSha256,
                FrozenPublications = (source.FrozenPublications
                    ?? Array.Empty<Simulation승인학습자료Publication>())
                    .Select(Clone).ToArray(),
                내면상태 = Clone(source.내면상태),
                성찰완료일차들 = source.성찰완료일차들?.ToArray()
                    ?? Array.Empty<int>(),
                Grants = (source.Grants ?? Array.Empty<Simulation거점성찰GrantSnapshot>())
                    .Select(value => new Simulation거점성찰GrantSnapshot
                    {
                        GrantStableId = value.GrantStableId,
                        PlayerStableId = value.PlayerStableId,
                        선택일차 = value.선택일차,
                        PublicationStableId = value.PublicationStableId,
                        PublicationRevision = value.PublicationRevision,
                        PublicationHashSha256 = value.PublicationHashSha256,
                        내면능력치Code = value.내면능력치Code,
                        능력치증가량 = value.능력치증가량,
                        내면효과Code = value.내면효과Code,
                        상태Code = value.상태Code,
                    }).ToArray(),
                적용CommandIds = source.적용CommandIds?.ToArray()
                    ?? Array.Empty<string>(),
                StateHashSha256 = source.StateHashSha256,
            };
    }
}
