using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface ICommunityPostIngredientPriceHintClient
{
    Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
        string body,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityPostIngredientPriceHintClient(HttpClient httpClient)
    : ICommunityPostIngredientPriceHintClient
{
    public async Task<CommunityPostIngredientPriceHintResponse> GetHintsAsync(
        string body,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/community/post-authoring/ingredient-price-hints",
            new CommunityPostIngredientPriceHintRequest(body),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content
                   .ReadFromJsonAsync<CommunityPostIngredientPriceHintResponse>(
                       cancellationToken: cancellationToken)
               ?? new CommunityPostIngredientPriceHintResponse(
                   [],
                   "가격 힌트 응답이 비어 있습니다.",
                   DateTime.UtcNow);
    }
}
