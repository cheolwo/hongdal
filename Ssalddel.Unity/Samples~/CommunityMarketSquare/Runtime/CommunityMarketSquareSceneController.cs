using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Community;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare
{
    public sealed class CommunityMarketSquareSceneController : MonoBehaviour
    {
        [SerializeField] private CommunityMarketSquareView squareView = null!;
        private CommunitySquareDataFlowLoadCoordinator coordinator = null!;
        private CommunitySquarePresenter presenter = null!;
        private CancellationTokenSource lifetime = null!;
        private Task? inFlight;

        [Inject]
        public void Construct(
            CommunitySquareDataFlowLoadCoordinator loadCoordinator,
            CommunitySquarePresenter squarePresenter,
            CommunityMarketSquareView view)
        {
            coordinator = loadCoordinator; presenter = squarePresenter; squareView = view;
        }

        private void Awake() => lifetime = new CancellationTokenSource();
        private async void Start()
        {
            try { await RefreshAsync(); }
            catch (OperationCanceledException) { }
        }

        public Task RefreshAsync()
        {
            if (inFlight != null && !inFlight.IsCompleted) return inFlight;
            inFlight = LoadAsync();
            return inFlight;
        }

        private async Task LoadAsync()
        {
            squareView.ShowState(squareView.ItemCount == 0
                ? CommunityMarketSquareLoadStateCodes.Loading
                : CommunityMarketSquareLoadStateCodes.Refreshing);
            squareView.Render(presenter.Present(await coordinator.LoadAsync(lifetime.Token)));
        }

        private void OnDestroy()
        {
            lifetime.Cancel(); lifetime.Dispose();
        }
    }
}
