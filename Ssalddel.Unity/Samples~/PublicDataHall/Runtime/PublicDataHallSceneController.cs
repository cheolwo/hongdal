using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.PublicData;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.PublicDataHall
{
    public sealed class PublicDataHallSceneController : MonoBehaviour
    {
        [SerializeField]
        private PublicDataHallView hallView = null!;

        private PublicDataHallSurfaceRuntimeCoordinator coordinator = null!;
        private PublicDataHallRuntimeConfiguration configuration = null!;
        private CancellationTokenSource lifetime = null!;
        private Task? inFlight;

        [Inject]
        public void Construct(
            PublicDataHallSurfaceRuntimeCoordinator loadCoordinator,
            PublicDataHallRuntimeConfiguration runtimeConfiguration,
            PublicDataHallView view)
        {
            coordinator = loadCoordinator;
            configuration = runtimeConfiguration;
            hallView = view;
        }

        private void Awake()
        {
            lifetime = new CancellationTokenSource();
        }

        private async void Start()
        {
            try
            {
                await RefreshAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }

        public Task RefreshAsync()
        {
            if (inFlight != null && !inFlight.IsCompleted)
            {
                return inFlight;
            }

            inFlight = LoadAsync();
            return inFlight;
        }

        private async Task LoadAsync()
        {
            var loadingState = coordinator.CurrentStatus.IsShowingLastSuccess
                ? ZoneRuntimeStateCode.Refreshing
                : ZoneRuntimeStateCode.InitialLoading;
            hallView.ShowState(loadingState.ToString());

            var result = await coordinator.RefreshDataAsync(
                new PublicWorldMapQuery { DatasetCode = PublicWorldMapDatasetCodes.DayWork },
                new PublicWorldInterpretationContext(),
                new InterpretationPerspectiveContext(
                    "PublicObserver",
                    "ExplorePublicData",
                    "PublicDataHall",
                    configuration.Mode),
                new PublicDataHallPresentationContext
                {
                    LocaleCode = "ko-KR",
                    QualityTierCode = "Primitive",
                },
                WorldDataQueryContext.Global(
                    PublicWorldMapDatasetCodes.DayWork,
                    configuration.DataMode),
                lifetime.Token);

            if (result.Changes != null)
            {
                hallView.Apply(result.Changes);
            }

            var statusMessage = result.Status.SafeErrorCode;
            if (result.Status.IsShowingLastSuccess
                && result.Status.StateCode == ZoneRuntimeStateCode.RefreshError)
            {
                statusMessage += "\n마지막 성공 데이터 유지";
            }
            else if (result.Presentation != null)
            {
                statusMessage = "markers " + result.Presentation.Markers.Count;
            }

            hallView.ShowState(result.Status.StateCode.ToString(), statusMessage);
        }

        private void OnDestroy()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
