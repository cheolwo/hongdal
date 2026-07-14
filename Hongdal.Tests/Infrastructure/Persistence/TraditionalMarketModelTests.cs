using Hongdal.Domain.TraditionalMarkets;
using Hongdal.Infrastructure.Persistence.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;

namespace Hongdal.Tests.Infrastructure.Persistence;

public sealed class TraditionalMarketModelTests
{
    [Fact]
    public void Model_시장과동기화이력을_독립테이블로구성한다()
    {
        using var context = CreateContext();

        var market = context.Model.FindEntityType(typeof(TraditionalMarket));
        var facilities = context.Model.FindEntityType(typeof(TraditionalMarketFacilities));
        var syncRun = context.Model.FindEntityType(typeof(TraditionalMarketSyncRun));
        var logisticsHub = context.Model.FindEntityType(typeof(TraditionalMarketLogisticsHub));
        var neighborhoodCouncil = context.Model.FindEntityType(typeof(전통시장생활권협의체));
        var tradeAgenda = context.Model.FindEntityType(typeof(전통시장교역안건));

        Assert.NotNull(market);
        Assert.NotNull(facilities);
        Assert.NotNull(syncRun);
        Assert.NotNull(logisticsHub);
        Assert.NotNull(neighborhoodCouncil);
        Assert.NotNull(tradeAgenda);
        Assert.Equal("public_data_traditional_markets", market!.GetTableName());
        Assert.Equal("MarketCode", Assert.Single(market.FindPrimaryKey()!.Properties).Name);
        Assert.True(facilities!.IsOwned());
        Assert.Equal("public_data_traditional_market_sync_runs", syncRun!.GetTableName());
        Assert.Equal("traditional_market_logistics_hubs", logisticsHub!.GetTableName());
        Assert.Equal("MarketCode", Assert.Single(logisticsHub.FindPrimaryKey()!.Properties).Name);
        Assert.True(logisticsHub.FindProperty(nameof(TraditionalMarketLogisticsHub.Revision))!.IsConcurrencyToken);
        Assert.Equal("traditional_market_neighborhood_councils", neighborhoodCouncil!.GetTableName());
        Assert.Equal("traditional_market_trade_agendas", tradeAgenda!.GetTableName());
        Assert.True(neighborhoodCouncil.FindProperty(nameof(전통시장생활권협의체.Revision))!.IsConcurrencyToken);
        Assert.True(tradeAgenda.FindProperty(nameof(전통시장교역안건.Revision))!.IsConcurrencyToken);
        Assert.Equal("CouncilId", tradeAgenda.FindProperty(nameof(전통시장교역안건.협의체Id))!.GetColumnName());
        Assert.Contains(tradeAgenda.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(전통시장생활권협의체));
        Assert.Contains(logisticsHub.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(TraditionalMarket));
        Assert.Contains(market.GetIndexes(), index =>
            index.Properties.Select(x => x.Name).SequenceEqual(["Province", "CityCounty", "IsActive"]));
    }

    private static TraditionalMarketDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TraditionalMarketDbContext>()
            .UseMySql(
                "Server=localhost;Database=hongdal_traditional_market_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new TraditionalMarketDbContext(options);
    }
}
