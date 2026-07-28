using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Shipper.ImportFood;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace SellerApp.Services;

public sealed class SellerForeignFoodFacilityService(ISsalddelJsonApiClient client)
{
    public async Task<해외판매자식품시설목록응답> 목록Async(
        CancellationToken cancellationToken = default)
        => await client.GetAsync<해외판매자식품시설목록응답>(
               "api/v1/seller/foreign-food-facilities",
               "해외 식품시설 준비 원장 목록 조회",
               cancellationToken: cancellationToken)
           ?? new 해외판매자식품시설목록응답();

    public Task<해외판매자식품시설응답?> 조회Async(
        string profileId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<해외판매자식품시설응답>(
            $"api/v1/seller/foreign-food-facilities/{Uri.EscapeDataString(profileId)}",
            "해외 식품시설 준비 원장 조회",
            cancellationToken: cancellationToken);

    public Task<해외판매자식품시설응답?> 저장Async(
        string profileId,
        해외판매자식품시설저장요청 request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<해외판매자식품시설저장요청, 해외판매자식품시설응답>(
            HttpMethod.Put,
            $"api/v1/seller/foreign-food-facilities/{Uri.EscapeDataString(profileId)}",
            request,
            "해외 식품시설 준비 원장 저장",
            cancellationToken: cancellationToken);

    public Task<해외제조업소조회화면응답?> 공식등록조회Async(
        string facilityName,
        string? countryName = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            "pageNo=1",
            "numOfRows=10",
            $"OCTR_MNFT_BSSH_NM={Uri.EscapeDataString(facilityName.Trim())}"
        };
        if (!string.IsNullOrWhiteSpace(countryName))
        {
            query.Add($"NATN_NM={Uri.EscapeDataString(countryName.Trim())}");
        }

        return client.GetAsync<해외제조업소조회화면응답>(
            $"api/v1/shipper/import-food/oversea-manufacturers?{string.Join("&", query)}",
            "식약처 해외제조업소 공식 등록 조회",
            cancellationToken: cancellationToken);
    }
}
