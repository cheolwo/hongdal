using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.PresentationContracts.Reconciliation
{
    public sealed class StableIdReconciliationException : InvalidOperationException
    {
        public StableIdReconciliationException(
            string errorCode,
            string collectionName,
            string stableId = "")
            : base(errorCode + ":" + (string.IsNullOrWhiteSpace(stableId) ? collectionName : stableId))
        {
            ErrorCode = errorCode;
            CollectionName = collectionName;
            StableId = stableId;
        }

        public string ErrorCode { get; }
        public string CollectionName { get; }
        public string StableId { get; }
    }

    /// <summary>
    /// Unity type에 의존하지 않는 stable-ID 증분 변경 계약입니다.
    /// Unchanged에는 기존 instance를 보존해 View binding을 유지합니다.
    /// </summary>
    public sealed class StableIdChangeSet<T>
    {
        public T[] Added { get; set; } = Array.Empty<T>();
        public T[] Updated { get; set; } = Array.Empty<T>();
        public T[] Removed { get; set; } = Array.Empty<T>();
        public T[] Unchanged { get; set; } = Array.Empty<T>();
    }

    /// <summary>
    /// Data revision의 시간 순서와 Presentation revision의 동일성을 분리해 비교합니다.
    /// Presentation revision이 없는 점진 migration 대상은 presentationEquivalent를 사용합니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class StableIdReconciliationPolicy<T>
    {
        public StableIdReconciliationPolicy(
            Func<T, string> stableId,
            Func<T, T, bool>? presentationEquivalent = null,
            Func<T, string>? presentationRevision = null,
            Comparison<T>? dataRevisionComparison = null)
        {
            StableId = stableId ?? throw new ArgumentNullException(nameof(stableId));
            PresentationEquivalent = presentationEquivalent;
            PresentationRevision = presentationRevision;
            DataRevisionComparison = dataRevisionComparison;

            if (presentationEquivalent == null && presentationRevision == null)
            {
                throw new ArgumentException(
                    "Presentation comparison contract is required.",
                    nameof(presentationEquivalent));
            }
        }

        public Func<T, string> StableId { get; }
        public Func<T, T, bool>? PresentationEquivalent { get; }
        public Func<T, string>? PresentationRevision { get; }
        public Comparison<T>? DataRevisionComparison { get; }
    }

    public sealed class StableIdReconciler<T>
    {
        private readonly StableIdReconciliationPolicy<T> policy;

        public StableIdReconciler(StableIdReconciliationPolicy<T> policy)
            => this.policy = policy ?? throw new ArgumentNullException(nameof(policy));

        public StableIdChangeSet<T> Reconcile(
            IEnumerable<T> current,
            IEnumerable<T> incoming)
        {
            if (current == null) throw new ArgumentNullException(nameof(current));
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));

            var before = Index(current, nameof(current));
            var after = Index(incoming, nameof(incoming));
            var added = new List<T>();
            var updated = new List<T>();
            var unchanged = new List<T>();

            foreach (var pair in after.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                if (!before.TryGetValue(pair.Key, out var existing))
                {
                    added.Add(pair.Value);
                    continue;
                }

                if (policy.DataRevisionComparison != null
                    && policy.DataRevisionComparison(pair.Value, existing) < 0)
                {
                    throw new StableIdReconciliationException(
                        "LowerDataRevision",
                        nameof(incoming),
                        pair.Key);
                }

                if (HasPresentationChange(existing, pair.Value, pair.Key))
                {
                    updated.Add(pair.Value);
                }
                else
                {
                    unchanged.Add(existing);
                }
            }

            var removed = before
                .Where(pair => !after.ContainsKey(pair.Key))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Value)
                .ToArray();

            return new StableIdChangeSet<T>
            {
                Added = added.ToArray(),
                Updated = updated.ToArray(),
                Removed = removed,
                Unchanged = unchanged.ToArray(),
            };
        }

        private Dictionary<string, T> Index(IEnumerable<T> values, string parameterName)
        {
            var result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (var item in values)
            {
                if (item == null)
                {
                    throw new StableIdReconciliationException(
                        "StableIdReconcileItemMissing",
                        parameterName);
                }

                var stableId = policy.StableId(item);
                if (!StableDataId.IsValid(stableId))
                {
                    throw new StableIdReconciliationException(
                        "StableIdInvalid",
                        parameterName,
                        stableId);
                }

                if (!result.TryAdd(stableId, item))
                {
                    throw new StableIdReconciliationException(
                        "DuplicateStableId",
                        parameterName,
                        stableId);
                }

                // 최초 추가와 삭제 대상도 같은 판본 계약을 검사한다.
                // 기존 동등성 함수만 사용하는 소비자에는 새 판본 필드를 요구하지 않는다.
                if (policy.PresentationRevision != null
                    && string.IsNullOrWhiteSpace(policy.PresentationRevision(item)))
                {
                    throw new StableIdReconciliationException(
                        "PresentationRevisionMissing", parameterName, stableId);
                }
            }

            return result;
        }

        private bool HasPresentationChange(T current, T incoming, string stableId)
        {
            if (policy.PresentationRevision != null)
            {
                var currentRevision = policy.PresentationRevision(current);
                var incomingRevision = policy.PresentationRevision(incoming);
                if (string.IsNullOrWhiteSpace(currentRevision)
                    || string.IsNullOrWhiteSpace(incomingRevision))
                {
                    throw new StableIdReconciliationException(
                        "PresentationRevisionMissing",
                        nameof(incoming),
                        stableId);
                }

                return !string.Equals(currentRevision, incomingRevision, StringComparison.Ordinal);
            }

            return !policy.PresentationEquivalent!(current, incoming);
        }
    }
}
