using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Application.WorldProjection;

public interface I감자생산유통World조회UseCase
{
    Task<Result<감자생산유통WorldResponse>> 조회Async(
        감자생산유통World조회요청 request,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PotatoProductionDistributionWorld,
    SsalddelCodeLayer.Application,
    "인증된 Farm 관점과 감자 국내 가격 source를 조회해 linkage 상태가 명시된 read-only World slice를 반환한다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.ThirdPartyApiCall,
    ContractType = typeof(I감자생산유통World조회UseCase),
    FlowOrder = 30,
    Boundary = "운영 조회 실패를 Simulation fixture로 대체하지 않으며 재배작기 선택은 서버가 승인한 Farm perspective 안에서만 허용한다.")]
public sealed class 감자생산유통World조회UseCase(
    IFarmProducerPerspectiveUseCase farmPerspectiveUseCase,
    IAgriculturalFisheriesInformationService informationService,
    감자생산유통WorldProjector projector)
    : I감자생산유통World조회UseCase
{
    public async Task<Result<감자생산유통WorldResponse>> 조회Async(
        감자생산유통World조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.LookbackDays is < 1 or > 90)
        {
            return Result.Fail<감자생산유통WorldResponse>(
                new Error("PotatoJourneyLookbackDaysInvalid")
                    .WithMetadata("StatusCode", StatusCodes.Status400BadRequest));
        }

        var farmResult = await farmPerspectiveUseCase.QueryAsync(cancellationToken);
        if (farmResult.IsFailed)
        {
            return Result.Fail<감자생산유통WorldResponse>(farmResult.Errors);
        }

        var selectedCultivationStableId = request.CultivationStableId?.Trim();
        if (!string.IsNullOrWhiteSpace(selectedCultivationStableId)
            && !ContainsCultivation(farmResult.Value, selectedCultivationStableId))
        {
            return Result.Fail<감자생산유통WorldResponse>(
                new Error("PotatoJourneyCultivationNotFound")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }

        var linkageStatusCode = string.IsNullOrWhiteSpace(selectedCultivationStableId)
            ? 감자생산유통LinkageStatusCodes.ProductOnly
            : 감자생산유통LinkageStatusCodes.Unverified;
        var price = await informationService.GetDomesticPriceAsync(
            new AgriculturalFisheriesDomesticPriceRequest
            {
                HsCode = 공통식품품목IdentityCatalog.감자Hs4,
                ReferenceDate = request.ReferenceDate?.Trim() ?? string.Empty,
                LookbackDays = request.LookbackDays,
            },
            cancellationToken);

        var projection = projector.Project(new 감자생산유통WorldProjectionInput(
            farmResult.Value,
            price,
            selectedCultivationStableId,
            감자생산유통SourceModeCodes.OperationalProjection,
            linkageStatusCode,
            DateTimeOffset.UtcNow));
        if (projection.IsFailed
            && projection.Errors.Any(error => error.Message == "PotatoJourneyCultivationNotFound"))
        {
            return Result.Fail<감자생산유통WorldResponse>(
                new Error("PotatoJourneyCultivationNotFound")
                    .WithMetadata("StatusCode", StatusCodes.Status404NotFound));
        }

        return projection;
    }

    private static bool ContainsCultivation(
        FarmProducerPerspectiveResponse perspective,
        string cultivationStableId)
        => perspective.Farms
            .SelectMany(farm => farm.Plots)
            .SelectMany(plot => plot.Cultivations)
            .Any(cultivation => string.Equals(
                cultivation.StableId,
                cultivationStableId,
                StringComparison.Ordinal));
}
