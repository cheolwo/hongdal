using System;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.PublicData;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ssalddel.Unity.Samples.PublicDataHall
{
    public sealed class PublicDataHallRuntimeConfiguration
    {
        public PublicDataHallRuntimeConfiguration(bool usesOperationalData)
        {
            Mode = usesOperationalData
                ? WorldInterpretationMode.Operational
                : WorldInterpretationMode.Simulation;
            DataMode = usesOperationalData
                ? DataRuntimeMode.Operational
                : DataRuntimeMode.Simulation;
        }

        public WorldInterpretationMode Mode { get; }

        public DataRuntimeMode DataMode { get; }
    }

    public sealed class PublicDataHallLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private bool useOperationalApi;

        [SerializeField]
        private string operationalApiBaseUrl = "https://localhost:5001/";

        [SerializeField]
        private int timeoutSeconds = 15;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new PublicDataHallRuntimeConfiguration(useOperationalApi));

            if (useOperationalApi)
            {
                builder.RegisterInstance(new PublicDataHallApiOptions
                {
                    BaseUrl = operationalApiBaseUrl,
                    TimeoutSeconds = Math.Max(1, timeoutSeconds),
                });
                builder.Register<OperationalPublicWorldMapApiClient>(Lifetime.Scoped)
                    .As<IPublicWorldMapApiClient>();
            }
            else
            {
                builder.Register<SimulatedPublicWorldMapApiClient>(Lifetime.Scoped)
                    .As<IPublicWorldMapApiClient>();
            }

            builder.Register<PublicWorldMapMapper>(Lifetime.Scoped);
            builder.Register<PublicWorldMapApiRepository>(Lifetime.Scoped)
                .As<IPublicWorldMapRepository>();
            builder.Register<PublicWorldMapQueryUseCase>(Lifetime.Scoped);
            builder.Register<PublicWorldMapDataMapper>(Lifetime.Scoped);
            builder.Register<PublicWorldMapApiDataRepository>(Lifetime.Scoped)
                .As<IPublicWorldMapDataRepository>();
            builder.Register<PublicWorldMapInterpreter>(Lifetime.Scoped);
            builder.Register<PublicWorldMapDataFlowQueryUseCase>(Lifetime.Scoped);
            builder.Register<PublicWorldMapReconciler>(Lifetime.Scoped);
            builder.Register<PublicDataHallLoadCoordinator>(Lifetime.Scoped);
            builder.Register<PublicDataHallDataFlowLoadCoordinator>(Lifetime.Scoped);
            builder.Register<PublicDataHallPresenter>(Lifetime.Scoped);
            builder.Register<PublicWorldMapRuntimeDataQuery>(Lifetime.Scoped);
            builder.Register<PublicSharedWorldInterpreter>(Lifetime.Scoped);
            builder.Register<PublicWorldPerspectiveInterpreter>(Lifetime.Scoped);
            builder.Register<PublicDataHallVisualPolicy>(Lifetime.Scoped);
            builder.Register<PublicDataHallSurfaceProjector>(Lifetime.Scoped);
            builder.Register<PublicDataHallSurfaceChangeSetCalculator>(Lifetime.Scoped);
            builder.Register<PublicDataHallSurfaceRuntimeCoordinator>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<PublicDataHallView>();
            builder.RegisterComponentInHierarchy<PublicDataHallSceneController>();
        }

        public void ConfigureSimulation()
        {
            useOperationalApi = false;
        }

        public void ConfigureOperational(string baseUrl, int requestTimeoutSeconds = 15)
        {
            useOperationalApi = true;
            operationalApiBaseUrl = baseUrl?.Trim() ?? string.Empty;
            timeoutSeconds = Math.Max(1, requestTimeoutSeconds);
        }
    }
}
