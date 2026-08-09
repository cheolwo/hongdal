using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

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
        private static readonly StableIdReconciler<WorldObjectProjection> Reconciler =
            new StableIdReconciler<WorldObjectProjection>(
                new StableIdReconciliationPolicy<WorldObjectProjection>(
                    projection => projection.StableId,
                    presentationEquivalent: (current, incoming) => !HasPresentationChange(current, incoming),
                    dataRevisionComparison: (incoming, current) => incoming.Revision.CompareTo(current.Revision)));

        public WorldProjectionChangeSet Reconcile(
            IEnumerable<WorldObjectProjection> current,
            IEnumerable<WorldObjectProjection> incoming)
        {
            StableIdChangeSet<WorldObjectProjection> changes;
            try
            {
                changes = Reconciler.Reconcile(current, incoming);
            }
            catch (StableIdReconciliationException error)
                when (error.ErrorCode == "LowerDataRevision")
            {
                throw new InvalidOperationException(
                    "LowerRevision:" + error.StableId,
                    error);
            }
            catch (StableIdReconciliationException error)
                when (error.ErrorCode == "DuplicateStableId")
            {
                throw new InvalidOperationException(
                    "DuplicateStableId:" + error.StableId,
                    error);
            }

            return new WorldProjectionChangeSet
            {
                Added = changes.Added,
                Updated = changes.Updated,
                Removed = changes.Removed,
                Unchanged = changes.Unchanged,
            };
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
