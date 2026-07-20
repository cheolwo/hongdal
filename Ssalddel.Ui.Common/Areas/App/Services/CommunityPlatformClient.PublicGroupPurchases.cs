using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public partial class CommunityPlatformClient
{
    private const string GroupPurchasePublicAppKey = "OrdererApp";

    public async Task<CommunityVoteListResponse> GetPublicGroupPurchaseVotesAsync(
        string? communityScope = null,
        string? hsCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryValue(query, "appKey", GroupPurchasePublicAppKey);
        AddQueryValue(query, "communityScope", communityScope);
        AddQueryValue(query, "hsCode", hsCode);

        using var response = await _httpClient.GetAsync(
            $"api/v1/community/votes?{string.Join("&", query)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CommunityVoteListResponse>(
                          cancellationToken: cancellationToken)
                      ?? new CommunityVoteListResponse();

        return new CommunityVoteListResponse
        {
            Items = payload.Items
                .Where(IsPublicGroupPurchase)
                .ToArray()
        };
    }

    public async Task<CommunityVoteResponse?> GetPublicGroupPurchaseVoteAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/v1/community/votes/{campaignId:D}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var campaign = await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(
            cancellationToken: cancellationToken);

        return campaign is not null && IsPublicGroupPurchase(campaign)
            ? campaign
            : null;
    }

    private static bool IsPublicGroupPurchase(CommunityVoteResponse campaign)
        => string.Equals(
            campaign.VoteKind,
            CommunityVoteKindCodes.GroupPurchaseDemand,
            StringComparison.Ordinal);
}
