using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.PublicData;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.PublicDataHall
{
    public sealed class PublicDataHallSceneController : MonoBehaviour
    {
        [SerializeField]
        private PublicDataHallView hallView = null!;

        private PublicDataHallLoadCoordinator coordinator = null!;
        private CancellationTokenSource lifetime = null!;
        private Task? inFlight;

        [Inject]
        public void Construct(
            PublicDataHallLoadCoordinator loadCoordinator,
            PublicDataHallView view)
        {
            coordinator = loadCoordinator;
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
            var loadingState = coordinator.StateCode == PublicDataHallLoadStateCodes.Success
                ? PublicDataHallLoadStateCodes.Refreshing
                : PublicDataHallLoadStateCodes.Loading;
            hallView.ShowState(loadingState);
            var result = await coordinator.LoadAsync(
                new PublicWorldMapQuery { DatasetCode = PublicWorldMapDatasetCodes.DayWork },
                lifetime.Token);
            hallView.Render(result);
        }

        private void OnDestroy()
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }
}
