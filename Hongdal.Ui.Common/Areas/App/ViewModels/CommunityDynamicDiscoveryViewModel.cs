using System.Net;
using System.Net.Http.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public interface ICommunityDynamicDiscoveryClient
{
    Task<CommunityPostContextDiscoveryResponse> DiscoverAsync(
        long postId,
        CommunityPostContextDiscoveryRequest request,
        CancellationToken cancellationToken = default);

    Task<CommunityDynamicTopicFeedResponse?> GetFeedAsync(
        string topicKey,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityDynamicDiscoveryClient(HttpClient httpClient)
    : ICommunityDynamicDiscoveryClient
{
    public async Task<CommunityPostContextDiscoveryResponse> DiscoverAsync(
        long postId,
        CommunityPostContextDiscoveryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"api/v1/community/posts/{postId}/opportunities/context-discovery",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityPostContextDiscoveryResponse>(cancellationToken)
               ?? throw new InvalidOperationException("게시글 문맥 후보 응답이 비어 있습니다.");
    }

    public async Task<CommunityDynamicTopicFeedResponse?> GetFeedAsync(
        string topicKey,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"api/v1/community/dynamic-topic-feeds/{Uri.EscapeDataString(topicKey)}?page={Math.Max(1, page)}&pageSize={Math.Clamp(pageSize, 1, 50)}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityDynamicTopicFeedResponse>(cancellationToken);
    }
}

public sealed class CommunityDynamicDiscoveryViewModel(ICommunityDynamicDiscoveryClient client)
    : ObservableObject
{
    private CommunityPostContextDiscoveryResponse? _context;
    private CommunityDynamicTopicFeedResponse? _feed;
    private bool _isLoading;
    private string? _errorMessage;

    public CommunityPostContextDiscoveryResponse? Context
    {
        get => _context;
        private set => SetProperty(ref _context, value);
    }

    public CommunityDynamicTopicFeedResponse? Feed
    {
        get => _feed;
        private set => SetProperty(ref _feed, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public async Task LoadPostContextAsync(
        long postId,
        decimal? latitude = null,
        decimal? longitude = null,
        bool confirmTransientLocationUse = false,
        CancellationToken cancellationToken = default)
    {
        await RunAsync(async () =>
        {
            Context = await client.DiscoverAsync(
                postId,
                new CommunityPostContextDiscoveryRequest
                {
                    CurrentLatitude = latitude,
                    CurrentLongitude = longitude,
                    RadiusKm = 7m,
                    ConfirmTransientLocationUse = confirmTransientLocationUse
                },
                cancellationToken);
        });
    }

    public async Task LoadTopicFeedAsync(
        string topicKey,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        await RunAsync(async () =>
        {
            Feed = await client.GetFeedAsync(topicKey, page, pageSize, cancellationToken);
            if (Feed is null)
            {
                ErrorMessage = "지원하지 않는 동적 주제입니다.";
            }
        });
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
