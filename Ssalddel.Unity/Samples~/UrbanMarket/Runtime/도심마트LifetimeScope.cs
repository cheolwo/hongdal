using Ssalddel.Unity.UrbanMarket;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.UrbanMarket
{
    public sealed class 도심마트LifetimeScope : LifetimeScope
    {
        [SerializeField]
        private bool useOperationalApi;

        [SerializeField]
        private string operationalApiBaseUrl = "https://localhost:5001/";

        [SerializeField]
        private int operationalTimeoutSeconds = 15;

        protected override void Configure(IContainerBuilder builder)
        {
            if (useOperationalApi)
            {
                builder.RegisterInstance(new UrbanMarketApiOptions
                {
                    BaseUrl = operationalApiBaseUrl,
                    TimeoutSeconds = Math.Max(1, operationalTimeoutSeconds),
                });
                builder.Register<OperationalUrbanMarketApiClient>(Lifetime.Scoped)
                    .As<I도심마트ApiClient>();
                builder.Register<도심마트ApiMapper>(Lifetime.Scoped);
                builder.Register<도심마트ApiRepository>(Lifetime.Scoped)
                    .As<I도심마트Repository>();
                builder.Register<Operational도심마트조회UseCase>(Lifetime.Scoped)
                    .As<I도심마트조회UseCase>();
            }
            else
            {
                builder.Register<Simulated도심마트조회UseCase>(Lifetime.Scoped)
                    .As<I도심마트조회UseCase>();
            }

            builder.Register<도심마트ScreenModelValidator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<도심마트View>();
            builder.RegisterComponentInHierarchy<도심마트SceneController>();
        }

        public void ConfigureOperationalApi(string baseUrl, int timeoutSeconds = 15)
        {
            useOperationalApi = true;
            operationalApiBaseUrl = baseUrl?.Trim() ?? string.Empty;
            operationalTimeoutSeconds = Math.Max(1, timeoutSeconds);
        }

        public void ConfigureSimulationApi()
        {
            useOperationalApi = false;
        }
    }
}
