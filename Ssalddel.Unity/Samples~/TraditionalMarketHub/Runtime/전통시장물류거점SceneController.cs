using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.TraditionalMarkets;
using UnityEngine;
using VContainer;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub
{
    public sealed class 전통시장물류거점SceneController : MonoBehaviour
    {
        private readonly object initializationSync = new object();
        private I전통시장물류거점조회UseCase hubQuery = null!;
        private 전통시장물류거점ScreenModelValidator validator = null!;
        private 전통시장물류거점View hubView = null!;
        private CancellationTokenSource? lifetime;
        private Task? activeInitialization;

        [Inject]
        public void Construct(
            I전통시장물류거점조회UseCase query,
            전통시장물류거점ScreenModelValidator modelValidator,
            전통시장물류거점View view)
        {
            hubQuery = query;
            validator = modelValidator;
            hubView = view;
        }

        private void Awake()
        {
            lifetime = new CancellationTokenSource();
        }

        private async void Start()
        {
            await InitializeAsync();
        }

        public Task InitializeAsync()
        {
            lock (initializationSync)
            {
                if (activeInitialization != null && !activeInitialization.IsCompleted)
                {
                    return activeInitialization;
                }

                activeInitialization = InitializeCoreAsync();
                return activeInitialization;
            }
        }

        private async Task InitializeCoreAsync()
        {
            if (hubView == null || !hubView.ValidateWiring())
            {
                Debug.LogError("전통시장물류거점View wiring이 완료되지 않았습니다.", this);
                return;
            }

            hubView.ShowLoading();
            try
            {
                var model = await hubQuery.조회Async(lifetime!.Token);
                var errors = validator.Validate(model);
                if (errors.Length > 0)
                {
                    var message = string.Join(", ", errors);
                    hubView.ShowError(message);
                    Debug.LogError("전통시장 물류거점 ScreenModel invalid: " + message, this);
                    return;
                }

                hubView.Render(model);
            }
            catch (OperationCanceledException) when (lifetime?.IsCancellationRequested == true)
            {
            }
            catch (Exception exception)
            {
                hubView.ShowError(exception.Message);
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
