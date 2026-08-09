using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.PublicData
{
    public sealed class PublicObservationPresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string MarkerLabelText { get; set; } = string.Empty;
        public string MarkerVisualStateCode { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
    }

    public sealed class PublicDataHallPresentationChangeSet
    {
        public PublicObservationPresentationModel[] Added { get; set; } = Array.Empty<PublicObservationPresentationModel>();
        public PublicObservationPresentationModel[] Updated { get; set; } = Array.Empty<PublicObservationPresentationModel>();
        public string[] RemovedStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class PublicDataHallPresentationModel
    {
        public string StateCode { get; set; } = PublicDataHallLoadStateCodes.Idle;
        public string StatusMessage { get; set; } = string.Empty;
        public string InterpretationRevision { get; set; } = string.Empty;
        public string PresentationRevision { get; set; } = string.Empty;
        public PublicObservationPresentationModel[] Observations { get; set; } =
            Array.Empty<PublicObservationPresentationModel>();
        public PublicDataHallPresentationChangeSet? Changes { get; set; }
    }

    public sealed class PublicDataHallPresenter
    {
        public PublicDataHallPresentationModel Present(PublicDataHallLoadResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (result.Snapshot == null)
            {
                return new PublicDataHallPresentationModel
                {
                    StateCode = result.StateCode,
                    StatusMessage = result.Error == null ? "0 observations" : result.Error.GetType().Name,
                };
            }

            var snapshot = result.Snapshot;
            var interpretationRevision = snapshot.Lineage?.InterpretationRevision
                ?? "interpretation:legacy:" + snapshot.Revision;
            var presentationRevision = WorldDataFlowRevisionCalculator.CalculatePresentation(
                interpretationRevision, PublicWorldMapDataFlowVersions.Perspective,
                PublicWorldMapDataFlowVersions.VisualRule,
                PublicWorldMapDataFlowVersions.PresentationContract);
            var observations = snapshot.Observations.Select(value => Present(value, presentationRevision)).ToArray();
            var byId = observations.ToDictionary(value => value.StableId, StringComparer.Ordinal);

            return new PublicDataHallPresentationModel
            {
                StateCode = result.StateCode,
                StatusMessage = result.Error == null
                    ? observations.Length + " observations"
                    : "마지막 성공 데이터 유지 · " + result.Error.GetType().Name,
                InterpretationRevision = interpretationRevision,
                PresentationRevision = presentationRevision,
                Observations = observations,
                Changes = result.Changes == null ? null : new PublicDataHallPresentationChangeSet
                {
                    Added = result.Changes.Added.Select(value => byId[value.StableId]).ToArray(),
                    Updated = result.Changes.Updated.Select(value => byId[value.StableId]).ToArray(),
                    RemovedStableIds = result.Changes.Removed.Select(value => value.StableId).ToArray(),
                },
            };
        }

        private static PublicObservationPresentationModel Present(
            PublicWorldMapObservation source, string presentationRevision)
            => new PublicObservationPresentationModel
            {
                StableId = source.StableId,
                PresentationRevision = presentationRevision + ":" + source.StableId,
                LayerCode = source.LayerCode,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                MarkerLabelText = source.Title + "\n" + source.SourceName,
                MarkerVisualStateCode = string.IsNullOrWhiteSpace(source.MarkerStatusCode)
                    ? source.FreshnessCode
                    : source.MarkerStatusCode,
                DetailHref = source.DetailHref,
                BoundaryNotice = source.BoundaryNotice,
            };
    }

    public sealed class PublicDataHallDataFlowLoadCoordinator
    {
        private readonly PublicWorldMapDataFlowQueryUseCase query;
        private readonly PublicWorldMapReconciler reconciler;
        private PublicWorldMapSnapshot? lastSuccessful;

        public PublicDataHallDataFlowLoadCoordinator(
            PublicWorldMapDataFlowQueryUseCase query,
            PublicWorldMapReconciler reconciler)
        { this.query = query; this.reconciler = reconciler; }

        public string StateCode { get; private set; } = PublicDataHallLoadStateCodes.Idle;

        public async Task<PublicDataHallLoadResult> LoadAsync(
            PublicWorldMapQuery request, CancellationToken cancellationToken = default)
        {
            var refreshing = lastSuccessful != null;
            StateCode = refreshing ? PublicDataHallLoadStateCodes.Refreshing : PublicDataHallLoadStateCodes.Loading;
            try
            {
                var snapshot = await query.실행Async(request, cancellationToken).ConfigureAwait(false);
                var changes = reconciler.Reconcile(
                    lastSuccessful?.Observations ?? Array.Empty<PublicWorldMapObservation>(), snapshot.Observations);
                lastSuccessful = snapshot;
                StateCode = PublicDataHallLoadStateCodes.Success;
                return new PublicDataHallLoadResult { StateCode = StateCode, Snapshot = snapshot, Changes = changes };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception error)
            {
                StateCode = refreshing ? PublicDataHallLoadStateCodes.RefreshError : PublicDataHallLoadStateCodes.InitialLoadError;
                return new PublicDataHallLoadResult { StateCode = StateCode, Snapshot = lastSuccessful, Error = error };
            }
        }
    }
}
