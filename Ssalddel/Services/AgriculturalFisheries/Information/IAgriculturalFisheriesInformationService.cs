using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IAgriculturalFisheriesInformationService
{
    AgriculturalFisheriesInformationOverviewResponse GetOverview();

    AgriculturalFisheriesItemSearchResponse SearchItems(
        string? query,
        string? categoryCode,
        int page,
        int pageSize);

    AgriculturalFisheriesItemResponse? FindItem(string? hsCode);

    농수산시세정보원목록응답 GetMarketPriceSources(
        string? countryCode,
        string? marketStageCode);

    농수산시세비교판정응답 AssessMarketPriceComparability(
        string? leftSourceKey,
        string? rightSourceKey);

    Task<AgriculturalFisheriesDomesticPriceResponse> GetDomesticPriceAsync(
        AgriculturalFisheriesDomesticPriceRequest request,
        CancellationToken cancellationToken = default);
}
