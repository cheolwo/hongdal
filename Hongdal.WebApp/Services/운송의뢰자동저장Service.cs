using System.Text.Json;
using Hongdal.Ui.Common.Areas.App.Models;
using Hongdal.WebApp.Models;
using Microsoft.JSInterop;

namespace Hongdal.WebApp.Services;

public sealed class 운송의뢰자동저장Service
{
    private const string StorageKey = "hongdal.web.shipper-request-draft.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _jsRuntime;

    public 운송의뢰자동저장Service(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task SaveAsync(운송의뢰작성ViewModel viewModel, CancellationToken cancellationToken = default)
        => SaveAsync(viewModel.ToDraft(), cancellationToken);

    public async Task SaveAsync(운송모델작성Draft draft, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(draft, JsonOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, json);
    }

    public async Task<운송모델작성Draft?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<운송모델작성Draft>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
    }
}
