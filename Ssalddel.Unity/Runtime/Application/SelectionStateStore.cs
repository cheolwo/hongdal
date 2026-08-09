using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.Application
{
    /// <summary>현재 선택은 서버 사실이나 World 의미가 아닌 Zone-scoped interaction 상태입니다.</summary>
    public sealed class SelectionStateStore : IWorldDataContextState
    {
        private string authorizationScopeKey = string.Empty;

        public WorldStableId? SelectedWorldId { get; private set; }
        public string AuthorizationScopeKey => authorizationScopeKey;

        public void SetAuthorizationScope(string value)
        {
            var normalized = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("AuthorizationScopeKeyMissing", nameof(value))
                : value.Trim();
            if (string.Equals(authorizationScopeKey, normalized, StringComparison.Ordinal)) return;
            authorizationScopeKey = normalized;
            SelectedWorldId = null;
        }

        public void SetDataContext(WorldDataContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            SetAuthorizationScope(context.BoundaryKey);
        }

        public void Select(WorldStableId stableId)
        {
            if (string.IsNullOrWhiteSpace(authorizationScopeKey))
                throw new InvalidOperationException("SelectionAuthorizationScopeMissing");
            if (!stableId.IsDefined)
                throw new ArgumentException("SelectionWorldStableIdMissing", nameof(stableId));
            SelectedWorldId = stableId;
        }

        public void Clear() => SelectedWorldId = null;

        public void HandleContextTransition(WorldDataContextTransition transition)
        {
            if (transition == null) throw new ArgumentNullException(nameof(transition));
            if (transition.Kind == WorldDataContextTransitionKind.Unchanged) return;
            if (transition.Current == null)
            {
                authorizationScopeKey = string.Empty;
                SelectedWorldId = null;
                return;
            }

            SetDataContext(transition.Current);
        }

        public bool RetainIfPresent(IEnumerable<WorldStableId> availableWorldIds)
        {
            if (availableWorldIds == null) throw new ArgumentNullException(nameof(availableWorldIds));
            if (!SelectedWorldId.HasValue) return false;
            var selected = SelectedWorldId.Value;
            if (availableWorldIds.Contains(selected)) return true;
            SelectedWorldId = null;
            return false;
        }
    }
}
