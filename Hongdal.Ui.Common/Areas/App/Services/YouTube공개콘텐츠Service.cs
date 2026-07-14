using System.Net.Http.Json;
using Hongdal.Contracts.Common.Content;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class YouTube공개콘텐츠Service
{
    public const string 홍익학당ChannelId = "UCI8HW08rOSlvweOjJ9Gp2Ng";

    private readonly HttpClient httpClient;

    public YouTube공개콘텐츠Service(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyList<YouTube재생목록Dto>> 재생목록조회Async(
        string channelId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        var path = $"api/v1/content/youtube/playlists?channelId={Uri.EscapeDataString(channelId.Trim())}";
        return await httpClient.GetFromJsonAsync<IReadOnlyList<YouTube재생목록Dto>>(
                   path,
                   cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<YouTube재생목록영상Dto>> 재생목록영상조회Async(
        string playlistId,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);
        var safeTake = Math.Clamp(take, 1, 200);
        var path = $"api/v1/content/youtube/playlists/{Uri.EscapeDataString(playlistId.Trim())}/videos?take={safeTake}";
        return await httpClient.GetFromJsonAsync<IReadOnlyList<YouTube재생목록영상Dto>>(
                   path,
                   cancellationToken)
               ?? [];
    }
}
