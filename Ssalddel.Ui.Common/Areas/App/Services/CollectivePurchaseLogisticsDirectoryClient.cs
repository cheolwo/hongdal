using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Operations;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface ICollectivePurchaseLogisticsDirectoryClient
{
    Task<CollectivePurchaseLogisticsDirectoryResponse> SearchAsync(
        string productHandlingCode,
        string? searchText = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}

public sealed class CollectivePurchaseLogisticsDirectoryClient(HttpClient httpClient)
    : ICollectivePurchaseLogisticsDirectoryClient
{
    public async Task<CollectivePurchaseLogisticsDirectoryResponse> SearchAsync(
        string productHandlingCode,
        string? searchText = null,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productHandlingCode);

        var query = new List<string>
        {
            $"stageCode={Uri.EscapeDataString(CollectivePurchaseLogisticsStageCodes.SharedInventoryStorage)}",
            $"productHandlingCode={Uri.EscapeDataString(productHandlingCode.Trim())}",
            "page=1",
            $"pageSize={Math.Clamp(pageSize, 1, 100)}"
        };
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query.Add($"q={Uri.EscapeDataString(searchText.Trim())}");
        }

        using var response = await httpClient.GetAsync(
            $"api/v1/operations/third-party-logistics/providers/collective-purchase?{string.Join("&", query)}",
            cancellationToken);
        var result = await response.Content
            .ReadFromJsonAsync<CollectivePurchaseLogisticsDirectoryResponse>(
                cancellationToken: cancellationToken);

        return result ?? new CollectivePurchaseLogisticsDirectoryResponse
        {
            Success = false,
            ErrorMessage = "공동구매 3PL 후보 응답을 확인할 수 없습니다."
        };
    }
}
