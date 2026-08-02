using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Platform;

namespace Ssalddel.WebApp.Services;

public sealed class GoogleMapsBrowserRuntimeClient(HttpClient httpClient)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    private Task<GoogleMapsBrowserRuntimeResponse?>? _runtimeSettingsTask;

    public Task<GoogleMapsBrowserRuntimeResponse?> TryGetAsync(
        CancellationToken cancellationToken = default)
    {
        _runtimeSettingsTask ??= FetchAsync();
        return _runtimeSettingsTask.WaitAsync(cancellationToken);
    }

    private async Task<GoogleMapsBrowserRuntimeResponse?> FetchAsync()
    {
        using var timeout = new CancellationTokenSource();
        timeout.CancelAfter(Timeout);

        try
        {
            using var response = await httpClient.GetAsync(
                GoogleMapsBrowserRuntimeRoutes.LocalDevelopment,
                timeout.Token);
            if (!response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<GoogleMapsBrowserRuntimeResponse>(
                cancellationToken: timeout.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
