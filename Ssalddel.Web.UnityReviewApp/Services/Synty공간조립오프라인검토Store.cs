using System.Text.Json;
using Microsoft.JSInterop;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Web.UnityReviewApp.Services;

public sealed class Synty공간조립오프라인검토Store(IJSRuntime jsRuntime)
{
    internal const string StorageKey = "ssalddel.unity-review.composition-review.offline.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<Synty공간조립오프라인검토항목>> 목록Async(
        CancellationToken cancellationToken = default)
    {
        var json = await jsRuntime.InvokeAsync<string?>(
            "localStorage.getItem",
            cancellationToken,
            StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Synty공간조립오프라인검토항목>>(
                       json,
                       JsonOptions)
                   ?? [];
        }
        catch (JsonException)
        {
            await jsRuntime.InvokeVoidAsync(
                "localStorage.removeItem",
                cancellationToken,
                StorageKey);
            return [];
        }
    }

    public async Task 추가또는교체Async(
        string reviewItemStableId,
        Synty공간조립검토결정Request request,
        CancellationToken cancellationToken = default)
    {
        var queue = await 목록Async(cancellationToken);
        var existingIndex = queue.FindIndex(item =>
            string.Equals(item.ReviewItemStableId, reviewItemStableId, StringComparison.Ordinal));
        var entry = new Synty공간조립오프라인검토항목
        {
            ReviewItemStableId = reviewItemStableId,
            Request = request,
            QueuedAtUtc = DateTime.UtcNow
        };

        if (existingIndex >= 0)
        {
            queue[existingIndex] = entry;
        }
        else
        {
            queue.Add(entry);
        }

        await 저장Async(queue, cancellationToken);
    }

    public async Task 저장Async(
        IReadOnlyList<Synty공간조립오프라인검토항목> queue,
        CancellationToken cancellationToken = default)
    {
        if (queue.Count == 0)
        {
            await jsRuntime.InvokeVoidAsync(
                "localStorage.removeItem",
                cancellationToken,
                StorageKey);
            return;
        }

        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            cancellationToken,
            StorageKey,
            JsonSerializer.Serialize(queue, JsonOptions));
    }
}

public sealed class Synty공간조립오프라인검토항목
{
    public string ReviewItemStableId { get; set; } = string.Empty;
    public Synty공간조립검토결정Request Request { get; set; } = new();
    public DateTime QueuedAtUtc { get; set; }
}
