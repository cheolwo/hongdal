using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Perspectives;
using Ssalddel.Unity.WorldProjection;
using Ssalddel.Unity.Transport;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.UrbanLogisticsCenter
{
    public sealed class 도심물류센터SceneController : MonoBehaviour
    {
        private RoleExperienceCoordinator roleCoordinator = null!;
        private NpcMovementQueryUseCase npcMovementQuery = null!;
        private 도심물류센터View zoneView = null!;
        private TransportCorridorQueryUseCase corridorQuery = null!;
        private TruckMovementApplicator truckApplicator = null!;
        private CancellationTokenSource? lifetime;

        [Inject]
        public void Construct(
            RoleExperienceCoordinator coordinator,
            NpcMovementQueryUseCase movementQuery,
            TransportCorridorQueryUseCase transportCorridorQuery,
            TruckMovementApplicator movementApplicator,
            도심물류센터View view)
        {
            roleCoordinator = coordinator;
            npcMovementQuery = movementQuery;
            corridorQuery = transportCorridorQuery;
            truckApplicator = movementApplicator;
            zoneView = view;
        }

        private void Awake()
        {
            lifetime = new CancellationTokenSource();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            if (!zoneView.ValidateWiring())
            {
                Debug.LogError("도심물류센터 View wiring이 완료되지 않았습니다.", this);
                return;
            }

            try
            {
                await roleCoordinator.SwitchAsync(
                    new 역할관점조회Request
                    {
                        RequestedRoleCode = RolePerspectiveCodes.Transporter,
                        WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
                    },
                    zoneView.GetRoleTargets(),
                    zoneView.GetInteractionSink(),
                    lifetime!.Token);

                var movement = await npcMovementQuery.실행Async(
                    new NpcMovementQuery
                    {
                        WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
                    },
                    lifetime.Token);
                if (movement != null)
                {
                    var unresolved = zoneView.ApplyNpcMovement(movement);
                    if (unresolved.Length > 0)
                    {
                        Debug.LogWarning("표현할 NPC View가 없습니다: " + string.Join(", ", unresolved), this);
                    }
                }

                zoneView.ApplyTransportCorridor(
                    await corridorQuery.실행Async(lifetime.Token),
                    truckApplicator);
            }
            catch (OperationCanceledException) when (lifetime?.IsCancellationRequested == true)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy()
        {
            lifetime?.Cancel();
            lifetime?.Dispose();
            lifetime = null;
        }
    }
}
