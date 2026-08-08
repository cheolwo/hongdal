using System;
using Ssalddel.Unity.Warehouse;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldLifetimeScope : LifetimeScope
    {
        [SerializeField] private bool useOperationalApi; [SerializeField] private string operationalApiBaseUrl = "https://localhost:5001/"; [SerializeField] private int timeoutSeconds = 15;
        protected override void Configure(IContainerBuilder builder)
        {
            if (useOperationalApi)
            {
                builder.RegisterInstance(new WarehouseWorldApiOptions { BaseUrl = operationalApiBaseUrl, TimeoutSeconds = Math.Max(1, timeoutSeconds) });
                builder.RegisterComponentInHierarchy<WarehouseRuntimeSessionTokenProvider>();
                builder.Register<OperationalWarehouseWorldApiClient>(Lifetime.Scoped).As<IWarehouseWorldApiClient>();
            }
            else builder.Register<SimulatedWarehouseWorldApiClient>(Lifetime.Scoped).As<IWarehouseWorldApiClient>();
            builder.Register<WarehouseWorldMapper>(Lifetime.Scoped); builder.Register<WarehouseWorldApiRepository>(Lifetime.Scoped).As<IWarehouseWorldRepository>();
            builder.Register<WarehouseWorldQueryUseCase>(Lifetime.Scoped); builder.Register<WarehouseWorldReconciler>(Lifetime.Scoped); builder.Register<WarehouseWorldLoadCoordinator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<WarehouseWorldView>(); builder.RegisterComponentInHierarchy<WarehouseWorldSceneController>();
        }
        public void ConfigureSimulation() => useOperationalApi = false;
        public void ConfigureOperational(string baseUrl, int timeout = 15) { useOperationalApi = true; operationalApiBaseUrl = baseUrl?.Trim() ?? string.Empty; timeoutSeconds = Math.Max(1, timeout); }
    }
}
