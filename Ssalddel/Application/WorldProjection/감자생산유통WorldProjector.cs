using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Application.WorldProjection;

public sealed record 감자생산유통WorldProjectionInput(
    FarmProducerPerspectiveResponse FarmPerspective,
    AgriculturalFisheriesDomesticPriceResponse DomesticPrice,
    string? SelectedCultivationStableId,
    string SourceModeCode,
    string LinkageStatusCode,
    DateTimeOffset GeneratedAt);

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PotatoProductionDistributionWorld,
    SsalddelCodeLayer.Application,
    "감자 관련 source를 이름 기반으로 조인하지 않고 linkage 상태가 보존된 read-only World slice로 투영한다.",
    Effects = SsalddelCodeEffect.None,
    ContractType = typeof(감자생산유통WorldResponse),
    FlowOrder = 20,
    Boundary = "CanonicalLinked와 SimulationLinked는 호출자가 명시한 관계만 소비하며 prefab·화면 상태·화물 충돌로 관계를 만들지 않는다.")]
public sealed class 감자생산유통WorldProjector
{
    public Result<감자생산유통WorldResponse> Project(
        감자생산유통WorldProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!IsSourceModeSupported(input.SourceModeCode))
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneySourceModeInvalid");
        }

        if (!IsLinkageSupported(input.LinkageStatusCode))
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneyLinkageStatusInvalid");
        }

        if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.SimulationLinked
            && input.SourceModeCode != 감자생산유통SourceModeCodes.SimulationFixture)
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneySimulationSourceRequired");
        }

        if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.CanonicalLinked
            && input.SourceModeCode != 감자생산유통SourceModeCodes.OperationalProjection)
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneyOperationalSourceRequired");
        }

        var selection = FindCultivation(
            input.FarmPerspective,
            input.SelectedCultivationStableId);
        if (!string.IsNullOrWhiteSpace(input.SelectedCultivationStableId)
            && selection is null)
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneyCultivationNotFound");
        }

        if (RequiresSelection(input.LinkageStatusCode) && selection is null)
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneyLinkedCultivationRequired");
        }

        if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.ProductOnly
            && selection is not null)
        {
            return Result.Fail<감자생산유통WorldResponse>("PotatoJourneyProductOnlySelectionNotAllowed");
        }

        var product = MapProduct(input.DomesticPrice);
        var price = MapPrice(input.DomesticPrice);
        var farm = selection is null
            ? null
            : MapFarm(selection.Value, input.LinkageStatusCode);
        var lineage = BuildLineage(input, farm, price);
        var limitations = BuildLimitations(input, selection is not null, price);
        var revision = ComputeRevision(input, farm, price, lineage);

        return Result.Ok(new 감자생산유통WorldResponse(
            // Unity client가 reconcile에 사용하는 기존 stable ID는 이름 변경과 분리해 유지한다.
            "world-slice:potato-journey",
            revision,
            input.GeneratedAt,
            input.FarmPerspective.AuthorizedRoleCode,
            input.FarmPerspective.ViewerScopeCode,
            input.FarmPerspective.AuthorizationDecisionId,
            input.SourceModeCode,
            input.LinkageStatusCode,
            product,
            farm,
            price,
            null,
            null,
            null,
            lineage,
            limitations,
            true));
    }

    private static 감자상품WorldResponse MapProduct(
        AgriculturalFisheriesDomesticPriceResponse source)
    {
        var identity = 공통식품품목IdentityCatalog.GetRequired(
            공통식품품목IdentityCatalog.감자ProductStableId);
        var hs = identity.CodeRelations.Single(relation =>
            relation.CodeScheme == 공통식품품목CodeSchemes.Hs4);
        var item = source.Item;
        return new 감자상품WorldResponse(
            identity.CanonicalProductStableId,
            item?.ProductName ?? identity.DisplayName,
            item?.HsPrefix ?? hs.Code ?? 공통식품품목IdentityCatalog.감자Hs4,
            item?.MatchQualityCode ?? "ExactCommodity",
            item?.MatchQualityLabel ?? "동일 품목",
            item?.Note ?? "감자 HS prefix와 국내 가격 관측을 연결한 정보용 상품입니다.",
            true);
    }

    private static 감자가격관측WorldResponse MapPrice(
        AgriculturalFisheriesDomesticPriceResponse source)
    {
        var price = source.Price;
        var ready = source.Success && price?.Success == true;
        var status = ready
            ? 감자가격관측StatusCodes.Ready
            : source.StatusCode switch
            {
                "MappingRequired" => 감자가격관측StatusCodes.MappingRequired,
                _ => 감자가격관측StatusCodes.DataUnavailable,
            };

        return new 감자가격관측WorldResponse(
            status,
            source.HsCode,
            "KRW_PER_KG",
            "KRW",
            price?.DataSource ?? "한국농수산식품유통공사(aT) 일별 도·소매 가격정보",
            price?.StartDate ?? string.Empty,
            price?.EndDate ?? string.Empty,
            MapAggregate(price?.Wholesale),
            MapAggregate(price?.Retail),
            source.Notices,
            true);
    }

    private static 감자가격구간WorldResponse? MapAggregate(
        AtDomesticFoodPriceAggregate? source)
        => source is null
            ? null
            : new 감자가격구간WorldResponse(
                source.PriceTypeCode,
                source.PriceTypeLabel,
                source.AverageKrwPerKg,
                source.MinimumKrwPerKg,
                source.MaximumKrwPerKg,
                source.SampleCount,
                source.LatestSurveyDate);

    private static 감자재배WorldResponse MapFarm(
        CultivationSelection selection,
        string linkageStatusCode)
        => new(
            selection.Farm.StableId,
            selection.Farm.Revision,
            selection.Plot.StableId,
            selection.Plot.Revision,
            selection.Cultivation.StableId,
            selection.Cultivation.Revision,
            selection.Cultivation.CropName,
            selection.Cultivation.CropReferenceStableId,
            selection.Cultivation.CropReferenceSourceKey,
            selection.Cultivation.GrowthStatusCode,
            selection.Cultivation.PlantedOn,
            selection.Cultivation.ExpectedHarvestOn,
            linkageStatusCode,
            selection.Plot.Sensors);

    private static CultivationSelection? FindCultivation(
        FarmProducerPerspectiveResponse source,
        string? selectedCultivationStableId)
    {
        var stableId = selectedCultivationStableId?.Trim();
        if (string.IsNullOrWhiteSpace(stableId))
        {
            return null;
        }

        foreach (var farm in source.Farms)
        {
            foreach (var plot in farm.Plots)
            {
                var cultivation = plot.Cultivations.FirstOrDefault(item =>
                    string.Equals(item.StableId, stableId, StringComparison.Ordinal));
                if (cultivation is not null)
                {
                    return new CultivationSelection(farm, plot, cultivation);
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<감자생산유통SourceLineageResponse> BuildLineage(
        감자생산유통WorldProjectionInput input,
        감자재배WorldResponse? farm,
        감자가격관측WorldResponse price)
    {
        var sources = new List<감자생산유통SourceLineageResponse>();
        if (farm is not null)
        {
            sources.Add(new 감자생산유통SourceLineageResponse(
                "ssalddel:farm-producer-perspective",
                farm.CultivationStableId,
                farm.CultivationRevision.ToString(),
                null,
                input.SourceModeCode));
        }

        sources.Add(new 감자생산유통SourceLineageResponse(
            "public-data:kamis-domestic-price",
            "price-observation:potato.0701",
            $"{price.StartDate}:{price.EndDate}:{price.StatusCode}",
            ParseObservationDate(price.Wholesale?.LatestSurveyDate)
                ?? ParseObservationDate(price.Retail?.LatestSurveyDate),
            감자생산유통SourceModeCodes.OperationalProjection));
        return sources;
    }

    private static DateTimeOffset? ParseObservationDate(string? value)
        => DateTime.TryParseExact(
            value,
            "yyyyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal
                | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? new DateTimeOffset(DateTime.SpecifyKind(parsed, DateTimeKind.Utc))
            : null;

    private static IReadOnlyList<string> BuildLimitations(
        감자생산유통WorldProjectionInput input,
        bool hasSelection,
        감자가격관측WorldResponse price)
    {
        var limitations = new List<string>
        {
            "국내 가격은 정보용 시장 관측이며 계약 단가, 농가 수취가격 또는 마트 판매가가 아닙니다.",
        };

        if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.ProductOnly)
        {
            limitations.Add("현재 응답에는 Farm 재배작기·화물·창고·마트의 canonical 상품 관계가 없습니다.");
        }
        else if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.Unverified && hasSelection)
        {
            limitations.Add("선택한 재배작기에는 공통 ProductStableId가 없어 감자 상품과의 관계를 확정하지 않았습니다.");
        }
        else if (input.LinkageStatusCode == 감자생산유통LinkageStatusCodes.SimulationLinked)
        {
            limitations.Add("재배작기와 상품 관계는 Simulation scenario 안에서만 유효하며 운영 원장을 변경하지 않습니다.");
        }

        if (price.StatusCode != 감자가격관측StatusCodes.Ready)
        {
            limitations.Add("국내 가격 관측을 사용할 수 없어 가격 숫자를 표시하지 않습니다.");
        }

        return limitations;
    }

    private static string ComputeRevision(
        감자생산유통WorldProjectionInput input,
        감자재배WorldResponse? farm,
        감자가격관측WorldResponse price,
        IReadOnlyList<감자생산유통SourceLineageResponse> lineage)
    {
        var raw = string.Join("|",
            input.SourceModeCode,
            input.LinkageStatusCode,
            input.FarmPerspective.Revision,
            farm?.CultivationStableId ?? string.Empty,
            farm?.CultivationRevision ?? 0,
            price.StatusCode,
            price.StartDate,
            price.EndDate,
            string.Join(";", lineage.Select(item => $"{item.SourceKey}:{item.SourceRevision}")));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)))
            .ToLowerInvariant();
    }

    private static bool RequiresSelection(string linkageStatusCode)
        => linkageStatusCode is 감자생산유통LinkageStatusCodes.CanonicalLinked
            or 감자생산유통LinkageStatusCodes.SimulationLinked
            or 감자생산유통LinkageStatusCodes.Unverified;

    private static bool IsSourceModeSupported(string sourceModeCode)
        => sourceModeCode is 감자생산유통SourceModeCodes.OperationalProjection
            or 감자생산유통SourceModeCodes.SimulationFixture;

    private static bool IsLinkageSupported(string linkageStatusCode)
        => linkageStatusCode is 감자생산유통LinkageStatusCodes.CanonicalLinked
            or 감자생산유통LinkageStatusCodes.SimulationLinked
            or 감자생산유통LinkageStatusCodes.ProductOnly
            or 감자생산유통LinkageStatusCodes.Unverified
            or 감자생산유통LinkageStatusCodes.Unavailable;

    private readonly record struct CultivationSelection(
        FarmResponse Farm,
        FarmPlotResponse Plot,
        FarmCultivationResponse Cultivation);
}
