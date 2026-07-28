using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SellerApp.Services;

public sealed class SellerExportLedgerService(ISsalddelJsonApiClient client)
{
    public async Task<판매자수출원장목록응답> 목록조회Async(
        판매자수출원장목록조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var query = new List<string>
        {
            $"page={Math.Max(0, request.Page)}",
            $"pageSize={Math.Clamp(request.PageSize, 1, 100)}"
        };
        Add(query, "search", request.Search);
        Add(query, "status", request.Status);

        return await client.GetAsync<판매자수출원장목록응답>(
                   $"api/v1/seller/export-ledgers?{string.Join("&", query)}",
                   "판매자 수출 준비 원장 목록 조회",
                   cancellationToken: cancellationToken)
               ?? new 판매자수출원장목록응답
               {
                   Page = Math.Max(0, request.Page),
                   PageSize = Math.Clamp(request.PageSize, 1, 100)
               };
    }

    private static void Add(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
