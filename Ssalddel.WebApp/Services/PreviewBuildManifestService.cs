using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Ssalddel.WebApp.Models;

namespace Ssalddel.WebApp.Services;

public sealed class PreviewBuildManifestService(NavigationManager navigation)
{
    private PreviewBuildManifest? _manifest;

    public async Task<PreviewBuildManifest> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_manifest is not null)
        {
            return _manifest;
        }

        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(navigation.BaseUri)
            };
            _manifest = await client.GetFromJsonAsync<PreviewBuildManifest>(
                            "preview-build.json",
                            cancellationToken)
                        ?? PreviewBuildManifest.Local;
        }
        catch (HttpRequestException)
        {
            _manifest = PreviewBuildManifest.Local;
        }
        catch (NotSupportedException)
        {
            _manifest = PreviewBuildManifest.Local;
        }

        return _manifest;
    }
}
