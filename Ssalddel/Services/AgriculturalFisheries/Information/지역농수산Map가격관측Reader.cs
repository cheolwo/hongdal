using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "농수산물 가격 전용 DB에서 지도 투영용 원천 지역 관측을 집계",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "원천의 산지·시장·Shipping Point 의미를 바꾸지 않고 지역 코드와 관측 기간만 집계합니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.RegionalAgriculturalMap,
    SsalddelCodeLayer.Application,
    "MAFRA 산지와 USDA AMS 시장·Shipping Point 관측을 relation별로 집계",
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "가격 전용 DB를 읽기 전용으로 조회하며 행정구역 연결을 판단하지 않습니다.")]
public sealed class 지역농수산Map가격관측Reader(
    AgriculturalFisheriesDbContext priceDb)
{
    private const string ShippingPointMarketType = "Shipping Point";

    internal async Task<IReadOnlyList<지역농수산MapSourceAggregate>> 조회Async(
        지역농수산MapNormalizedQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CountryCode == RegionalAgriculturalMapCountryCodes.Korea)
        {
            return await LoadKoreaOriginAggregatesAsync(request, cancellationToken);
        }

        var aggregates = new List<지역농수산MapSourceAggregate>();
        if (request.RelationTypeCodes.Contains(
                RegionalAgriculturalMapRelationTypeCodes.MarketObservation,
                StringComparer.Ordinal))
        {
            aggregates.AddRange(await LoadUnitedStatesMarketAggregatesAsync(
                request,
                cancellationToken));
        }

        if (request.RelationTypeCodes.Contains(
                RegionalAgriculturalMapRelationTypeCodes.ShippingPointOrPortOfEntry,
                StringComparer.Ordinal))
        {
            aggregates.AddRange(await LoadUnitedStatesShippingAggregatesAsync(
                request,
                cancellationToken));
        }

        return aggregates;
    }

    private async Task<IReadOnlyList<지역농수산MapSourceAggregate>> LoadKoreaOriginAggregatesAsync(
        지역농수산MapNormalizedQuery request,
        CancellationToken cancellationToken)
    {
        var query = priceDb.DomesticAuctionPriceObservations
            .AsNoTracking()
            .AsQueryable();
        if (request.ProductName is not null)
        {
            query = query.Where(item => item.ItemName == request.ProductName);
        }

        if (request.FromDate is { } fromDate)
        {
            query = query.Where(item => item.SettlementDate >= fromDate);
        }

        if (request.ToDate is { } toDate)
        {
            query = query.Where(item => item.SettlementDate <= toDate);
        }

        var rows = await query
            .GroupBy(item => new
            {
                item.SourceKey,
                item.OriginCode,
                item.OriginName
            })
            .Select(group => new
            {
                group.Key.SourceKey,
                group.Key.OriginCode,
                group.Key.OriginName,
                Count = group.Count(),
                EarliestDate = group.Min(item => item.SettlementDate),
                LatestDate = group.Max(item => item.SettlementDate)
            })
            .ToArrayAsync(cancellationToken);

        return rows.Select(row => new 지역농수산MapSourceAggregate(
            RegionalAgriculturalMapCodeSchemeCodes.KoreaMafraOrigin,
            RegionalAgriculturalMapRelationTypeCodes.ConfirmedOrigin,
            row.SourceKey,
            지역농수산MapCodeNormalizer.Normalize(row.OriginCode),
            row.OriginName.Trim(),
            row.Count,
            row.EarliestDate,
            row.LatestDate)).ToArray();
    }

    private async Task<IReadOnlyList<지역농수산MapSourceAggregate>> LoadUnitedStatesMarketAggregatesAsync(
        지역농수산MapNormalizedQuery request,
        CancellationToken cancellationToken)
    {
        var query = ApplyUnitedStatesFilters(
            priceDb.UsdaAmsMarketPriceObservations
                .AsNoTracking()
                .Where(item => item.MarketType != ShippingPointMarketType),
            request);
        var rows = await query
            .GroupBy(item => new
            {
                item.SourceKey,
                item.MarketLocationState,
                item.MarketLocationName
            })
            .Select(group => new
            {
                group.Key.SourceKey,
                group.Key.MarketLocationState,
                group.Key.MarketLocationName,
                Count = group.Count(),
                EarliestDate = group.Min(item => item.ReportBeginDate),
                LatestDate = group.Max(item => item.ReportEndDate)
            })
            .ToArrayAsync(cancellationToken);

        return rows.Select(row => new 지역농수산MapSourceAggregate(
            RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesPostalState,
            RegionalAgriculturalMapRelationTypeCodes.MarketObservation,
            row.SourceKey,
            지역농수산MapCodeNormalizer.Normalize(row.MarketLocationState),
            row.MarketLocationName.Trim(),
            row.Count,
            row.EarliestDate,
            row.LatestDate)).ToArray();
    }

    private async Task<IReadOnlyList<지역농수산MapSourceAggregate>> LoadUnitedStatesShippingAggregatesAsync(
        지역농수산MapNormalizedQuery request,
        CancellationToken cancellationToken)
    {
        var query = ApplyUnitedStatesFilters(
            priceDb.UsdaAmsMarketPriceObservations
                .AsNoTracking()
                .Where(item => item.MarketType == ShippingPointMarketType),
            request);
        var rows = await query
            .GroupBy(item => new
            {
                item.SourceKey,
                item.District
            })
            .Select(group => new
            {
                group.Key.SourceKey,
                group.Key.District,
                Count = group.Count(),
                EarliestDate = group.Min(item => item.ReportBeginDate),
                LatestDate = group.Max(item => item.ReportEndDate)
            })
            .ToArrayAsync(cancellationToken);

        return rows.Select(row => new 지역농수산MapSourceAggregate(
            RegionalAgriculturalMapCodeSchemeCodes.UnitedStatesAmsShippingDistrict,
            RegionalAgriculturalMapRelationTypeCodes.ShippingPointOrPortOfEntry,
            row.SourceKey,
            지역농수산MapCodeNormalizer.Normalize(row.District),
            row.District.Trim(),
            row.Count,
            row.EarliestDate,
            row.LatestDate)).ToArray();
    }

    private static IQueryable<UsdaAms시장가격관측> ApplyUnitedStatesFilters(
        IQueryable<UsdaAms시장가격관측> query,
        지역농수산MapNormalizedQuery request)
    {
        if (request.ProductName is not null)
        {
            query = query.Where(item => item.Commodity == request.ProductName);
        }

        if (request.FromDate is { } fromDate)
        {
            query = query.Where(item => item.ReportEndDate >= fromDate);
        }

        if (request.ToDate is { } toDate)
        {
            query = query.Where(item => item.ReportBeginDate <= toDate);
        }

        return query;
    }
}
