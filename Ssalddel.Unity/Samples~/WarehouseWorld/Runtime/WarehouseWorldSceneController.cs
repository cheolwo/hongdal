using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Warehouse;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldSceneController : MonoBehaviour
    {
        [SerializeField] private long warehouseId = 7;
        [SerializeField] private WarehouseWorldView warehouseView = null!;
        private WarehouseWorldLoadCoordinator coordinator = null!; private CancellationTokenSource lifetime = null!; private Task? inFlight;
        [Inject] public void Construct(WarehouseWorldLoadCoordinator loadCoordinator, WarehouseWorldView view) { coordinator = loadCoordinator; warehouseView = view; }
        public void ConfigureWarehouse(long id) => warehouseId = id;
        private void Awake() => lifetime = new CancellationTokenSource();
        private async void Start() { try { await RefreshAsync(); } catch (OperationCanceledException) { } }
        public Task RefreshAsync() { if (inFlight != null && !inFlight.IsCompleted) return inFlight; inFlight = LoadAsync(); return inFlight; }
        private async Task LoadAsync()
        {
            warehouseView.ShowState(warehouseView.ObjectCount == 0 ? WarehouseWorldLoadStateCodes.Loading : WarehouseWorldLoadStateCodes.Refreshing);
            warehouseView.Render(await coordinator.LoadAsync(warehouseId, lifetime.Token));
        }
        private void OnDestroy() { lifetime.Cancel(); lifetime.Dispose(); }
    }
}
