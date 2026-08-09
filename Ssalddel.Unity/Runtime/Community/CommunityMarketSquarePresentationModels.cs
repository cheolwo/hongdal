using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Community
{
    public sealed class CommunitySquareItemPresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public string KindCode { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string DetailText { get; set; } = string.Empty;
        public string VisualStateCode { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
    }

    public sealed class CommunitySquarePresentationChangeSet
    {
        public CommunitySquareItemPresentationModel[] Added { get; set; } = Array.Empty<CommunitySquareItemPresentationModel>();
        public CommunitySquareItemPresentationModel[] Updated { get; set; } = Array.Empty<CommunitySquareItemPresentationModel>();
        public string[] RemovedStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class CommunitySquarePresentationModel
    {
        public string StateCode { get; set; } = CommunityMarketSquareLoadStateCodes.Idle;
        public string StatusMessage { get; set; } = string.Empty;
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public CommunitySquareItemPresentationModel[] Items { get; set; } =
            Array.Empty<CommunitySquareItemPresentationModel>();
        public CommunitySquarePresentationChangeSet? Changes { get; set; }
    }

    public sealed class CommunitySquarePresenter
    {
        public CommunitySquarePresentationModel Present(CommunityMarketSquareLoadResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.Snapshot == null)
            {
                return new CommunitySquarePresentationModel
                {
                    StateCode = result.StateCode,
                    StatusMessage = result.Error == null ? "0 public items" : result.Error.GetType().Name,
                };
            }

            var snapshot = result.Snapshot;
            var interpretationRevision = snapshot.Lineage?.InterpretationRevision
                ?? "interpretation:legacy:" + snapshot.Revision;
            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                interpretationRevision, CommunitySquareDataFlowVersions.Perspective,
                CommunitySquareDataFlowVersions.VisualRule,
                CommunitySquareDataFlowVersions.PresentationContract);
            var items = snapshot.Items.Select(value => Present(value, presentationRevision)).ToArray();
            var byId = items.ToDictionary(value => value.StableId, StringComparer.Ordinal);
            return new CommunitySquarePresentationModel
            {
                StateCode = result.StateCode,
                StatusMessage = result.Error == null
                    ? items.Length + " public items"
                    : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name,
                InterpretationRevision = interpretationRevision,
                PresentationRevision = presentationRevision,
                Items = items,
                Changes = result.Changes == null ? null : new CommunitySquarePresentationChangeSet
                {
                    Added = result.Changes.Added.Select(value => byId[value.StableId]).ToArray(),
                    Updated = result.Changes.Updated.Select(value => byId[value.StableId]).ToArray(),
                    RemovedStableIds = result.Changes.Removed.Select(value => value.StableId).ToArray(),
                },
            };
        }

        private static CommunitySquareItemPresentationModel Present(
            CommunitySquareWorldItem source, string presentationRevision)
            => new CommunitySquareItemPresentationModel
            {
                StableId = source.StableId,
                PresentationRevision = presentationRevision + ":" + source.StableId,
                KindCode = source.Kind,
                TitleText = source.Title,
                DetailText = source.Status + (source.Count > 0 ? " · " + source.Count : string.Empty),
                VisualStateCode = source.Kind,
                DetailHref = source.DetailHref,
            };
    }

    public sealed class CommunitySquareDataFlowLoadCoordinator
    {
        private readonly CommunitySquareDataFlowQueryUseCase query;
        private readonly CommunityMarketSquareReconciler reconciler;
        private CommunityMarketSquareSnapshot? lastSuccessful;
        public CommunitySquareDataFlowLoadCoordinator(
            CommunitySquareDataFlowQueryUseCase query,
            CommunityMarketSquareReconciler reconciler)
        { this.query = query; this.reconciler = reconciler; }

        public string StateCode { get; private set; } = CommunityMarketSquareLoadStateCodes.Idle;

        public async Task<CommunityMarketSquareLoadResult> LoadAsync(CancellationToken cancellationToken = default)
        {
            var refreshing = lastSuccessful != null;
            StateCode = refreshing ? CommunityMarketSquareLoadStateCodes.Refreshing : CommunityMarketSquareLoadStateCodes.Loading;
            try
            {
                var snapshot = await query.실행Async(cancellationToken).ConfigureAwait(false);
                var changes = reconciler.Reconcile(
                    lastSuccessful?.Items ?? Array.Empty<CommunitySquareWorldItem>(), snapshot.Items);
                lastSuccessful = snapshot;
                StateCode = CommunityMarketSquareLoadStateCodes.Success;
                return new CommunityMarketSquareLoadResult { StateCode = StateCode, Snapshot = snapshot, Changes = changes };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception error)
            {
                StateCode = refreshing ? CommunityMarketSquareLoadStateCodes.RefreshError : CommunityMarketSquareLoadStateCodes.InitialLoadError;
                return new CommunityMarketSquareLoadResult { StateCode = StateCode, Snapshot = lastSuccessful, Error = error };
            }
        }
    }
}
