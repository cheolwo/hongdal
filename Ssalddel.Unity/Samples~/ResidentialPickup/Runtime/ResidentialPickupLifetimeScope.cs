using System;
using Ssalddel.Unity.ResidentialPickup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.ResidentialPickup
{
    public sealed class ResidentialPickupLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private bool useOperationalApi;

        [SerializeField]
        private string operationalApiBaseUrl = "https://localhost:5001/";

        [SerializeField]
        private int operationalTimeoutSeconds = 15;

        [SerializeField]
        private ResidentialPickupSessionTokenProvider sessionTokenProvider = null!;

        protected override void Configure(IContainerBuilder builder)
        {
            if (useOperationalApi)
            {
                if (sessionTokenProvider == null)
                {
                    throw new InvalidOperationException("ResidentialPickupSessionProviderMissing");
                }

                builder.RegisterInstance(new ResidentialPickupApiOptions
                {
                    BaseUrl = operationalApiBaseUrl,
                    TimeoutSeconds = Math.Max(1, operationalTimeoutSeconds),
                });
                builder.RegisterComponent(sessionTokenProvider);
                builder.Register<OperationalResidentialPickupApiClient>(Lifetime.Scoped)
                    .As<IResidentialPickupPerspectiveApiClient>();
            }
            else
            {
                builder.Register<SimulatedResidentialPickupApiClient>(Lifetime.Scoped)
                    .As<IResidentialPickupPerspectiveApiClient>();
            }

            builder.Register<ResidentialPickupPerspectiveMapper>(Lifetime.Scoped);
            builder.Register<ResidentialPickupPerspectiveApiRepository>(Lifetime.Scoped)
                .As<IResidentialPickupPerspectiveRepository>();
            builder.Register<ResidentialPickupPerspectiveQueryUseCase>(Lifetime.Scoped);
            builder.Register<ResidentialPickupPerspectiveApplicator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<ResidentialPickupView>();
            builder.RegisterComponentInHierarchy<ResidentialPickupSceneController>();
        }

        public void ConfigureSimulationApi(ResidentialPickupSessionTokenProvider tokenProvider)
        {
            useOperationalApi = false;
            sessionTokenProvider = tokenProvider;
        }
    }
}
