using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.ResidentialPickup;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.ResidentialPickup
{
    public sealed class ResidentialPickupSceneController : MonoBehaviour
    {
        private ResidentialPickupPerspectiveQueryUseCase query = null!;
        private ResidentialPickupPerspectiveApplicator applicator = null!;
        private ResidentialPickupView zoneView = null!;
        private CancellationTokenSource? lifetime;
        private string currentRoleCode = ResidentialPickupRoleCodes.Orderer;

        [Inject]
        public void Construct(
            ResidentialPickupPerspectiveQueryUseCase perspectiveQuery,
            ResidentialPickupPerspectiveApplicator perspectiveApplicator,
            ResidentialPickupView view)
        {
            query = perspectiveQuery;
            applicator = perspectiveApplicator;
            zoneView = view;
        }

        private void Awake()
        {
            lifetime = new CancellationTokenSource();
        }

        private async void Start()
        {
            await SwitchRoleAsync(currentRoleCode);
        }

        public async Task SwitchRoleAsync(string roleCode)
        {
            if (!zoneView.ValidateWiring())
            {
                Debug.LogError("Residential Pickup View wiring is invalid.", this);
                return;
            }

            currentRoleCode = roleCode;
            zoneView.ShowLoading(roleCode);
            try
            {
                var snapshot = await query.실행Async(roleCode, lifetime!.Token);
                var unresolved = zoneView.Render(snapshot, applicator);
                if (unresolved.Length > 0)
                {
                    Debug.LogWarning(
                        "Residential pickup target missing: " + string.Join(", ", unresolved),
                        this);
                }
            }
            catch (OperationCanceledException) when (lifetime?.IsCancellationRequested == true)
            {
            }
            catch (Exception exception)
            {
                zoneView.ShowError(exception.Message);
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
