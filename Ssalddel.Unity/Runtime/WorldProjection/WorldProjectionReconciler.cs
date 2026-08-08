using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.WorldProjection
{
    public sealed class WorldProjectionChangeSet
    {
        public WorldObjectProjection[] Added { get; set; } = Array.Empty<WorldObjectProjection>();

        public WorldObjectProjection[] Updated { get; set; } = Array.Empty<WorldObjectProjection>();

        public WorldObjectProjection[] Removed { get; set; } = Array.Empty<WorldObjectProjection>();

        public WorldObjectProjection[] Unchanged { get; set; } = Array.Empty<WorldObjectProjection>();
    }

    public sealed class WorldProjectionReconciler
    {
        public WorldProjectionChangeSet Reconcile(
            IEnumerable<WorldObjectProjection> current,
            IEnumerable<WorldObjectProjection> incoming)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (incoming == null)
            {
                throw new ArgumentNullException(nameof(incoming));
            }

            var currentById = Index(current, nameof(current));
            var incomingById = Index(incoming, nameof(incoming));
            var added = new List<WorldObjectProjection>();
            var updated = new List<WorldObjectProjection>();
            var unchanged = new List<WorldObjectProjection>();

            foreach (var pair in incomingById.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                if (!currentById.TryGetValue(pair.Key, out var existing))
                {
                    added.Add(pair.Value);
                    continue;
                }

                if (pair.Value.Revision < existing.Revision)
                {
                    throw new InvalidOperationException($"LowerRevision:{pair.Key}");
                }

                if (HasPresentationChange(existing, pair.Value))
                {
                    updated.Add(pair.Value);
                }
                else
                {
                    unchanged.Add(existing);
                }
            }

            var removed = currentById
                .Where(pair => !incomingById.ContainsKey(pair.Key))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Value)
                .ToArray();

            return new WorldProjectionChangeSet
            {
                Added = added.ToArray(),
                Updated = updated.ToArray(),
                Removed = removed,
                Unchanged = unchanged.ToArray(),
            };
        }

        private static Dictionary<string, WorldObjectProjection> Index(
            IEnumerable<WorldObjectProjection> projections,
            string parameterName)
        {
            var result = new Dictionary<string, WorldObjectProjection>(StringComparer.Ordinal);
            foreach (var projection in projections)
            {
                if (projection == null)
                {
                    throw new ArgumentException("ProjectionNull", parameterName);
                }

                StableDataId.EnsureValid(projection.StableId, parameterName);
                if (!result.TryAdd(projection.StableId, projection))
                {
                    throw new InvalidOperationException($"DuplicateStableId:{projection.StableId}");
                }
            }

            return result;
        }

        private static bool HasPresentationChange(
            WorldObjectProjection current,
            WorldObjectProjection incoming)
        {
            return current.Revision != incoming.Revision
                || !string.Equals(current.WorldZoneCode, incoming.WorldZoneCode, StringComparison.Ordinal)
                || !string.Equals(current.WorldObjectKey, incoming.WorldObjectKey, StringComparison.Ordinal)
                || !string.Equals(current.DisplayStateCode, incoming.DisplayStateCode, StringComparison.Ordinal)
                || !string.Equals(current.DataStatusCode, incoming.DataStatusCode, StringComparison.Ordinal)
                || !current.EvidenceCardIds.SequenceEqual(incoming.EvidenceCardIds, StringComparer.Ordinal);
        }
    }
}
