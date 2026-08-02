using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Platform;

namespace Ssalddel.WebApp.Services;

public sealed class GoogleMapsBrowserRuntimeClient(HttpClient httpClient)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<GoogleMapsBrowserRuntimeResponse?> TryGetAsync(
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
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
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
