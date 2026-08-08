using System;
using Ssalddel.Unity.Community;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare
{
    public sealed class CommunityMarketSquareLifetimeScope : LifetimeScope
    {
        [SerializeField] private bool useOperationalApi;
        [SerializeField] private string operationalApiBaseUrl = "https://localhost:5001/";
        [SerializeField] private int timeoutSeconds = 15;

        protected override void Configure(IContainerBuilder builder)
        {
            if (useOperationalApi)
            {
                builder.RegisterInstance(new CommunityMarketSquareApiOptions { BaseUrl = operationalApiBaseUrl, TimeoutSeconds = Math.Max(1, timeoutSeconds) });
                builder.Register<OperationalCommunityMarketSquareApiClient>(Lifetime.Scoped).As<ICommunityMarketSquareApiClient>();
            }
            else builder.Register<SimulatedCommunityMarketSquareApiClient>(Lifetime.Scoped).As<ICommunityMarketSquareApiClient>();

            builder.Register<CommunityMarketSquareMapper>(Lifetime.Scoped);
            builder.Register<CommunityMarketSquareApiRepository>(Lifetime.Scoped).As<ICommunityMarketSquareRepository>();
            builder.Register<CommunityMarketSquareQueryUseCase>(Lifetime.Scoped);
            builder.Register<CommunityMarketSquareReconciler>(Lifetime.Scoped);
            builder.Register<CommunityMarketSquareLoadCoordinator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<CommunityMarketSquareView>();
            builder.RegisterComponentInHierarchy<CommunityMarketSquareSceneController>();
        }

        public void ConfigureSimulation() => useOperationalApi = false;
        public void ConfigureOperational(string baseUrl, int requestTimeoutSeconds = 15)
        {
            useOperationalApi = true; operationalApiBaseUrl = baseUrl?.Trim() ?? string.Empty; timeoutSeconds = Math.Max(1, requestTimeoutSeconds);
        }
    }
}
