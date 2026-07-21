using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class WarehouseInboundAggregateModelTests
{
    [Fact]
    public void Model_입고와재고수명주기는_scalar참조로분리하고조회인덱스를둔다()
    {
        using var context = CreateContext();
        var inboundItem = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(입고상품)));
        var inventoryHistory = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(재고이력)));
        var inventoryMovement = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(재고이동)));

        Assert.Empty(inboundItem.GetForeignKeys());
        Assert.Empty(inventoryHistory.GetForeignKeys());
        Assert.Empty(inventoryMovement.GetForeignKeys());
        AssertIndex(inboundItem, nameof(입고상품.입고요청Id));
        AssertIndex(
            inventoryHistory,
            nameof(재고이력.입고상품Id),
            nameof(재고이력.처리일시));
        AssertIndex(
            inventoryMovement,
            nameof(재고이동.입고상품Id),
            nameof(재고이동.발생일시));
    }

    [Fact]
    public void Model_커뮤니티원장Id는_Mongo외부식별자이고_Ef관계가아니다()
    {
        using var context = CreateContext();

        foreach (var entityType in new[]
                 {
                     context.Model.FindEntityType(typeof(입고요청)),
                     context.Model.FindEntityType(typeof(입고상품))
                 })
        {
            var entity = Assert.IsAssignableFrom<IEntityType>(entityType);
            var ledgerProperty = Assert.IsAssignableFrom<IProperty>(
                entity.FindProperty(nameof(입고요청.커뮤니티원장Id)));
            Assert.DoesNotContain(
                entity.GetForeignKeys(),
                foreignKey => foreignKey.Properties.Contains(ledgerProperty));
        }
    }

    private static void AssertIndex(IEntityType entityType, params string[] propertyNames)
        => Assert.Contains(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name)
                .SequenceEqual(propertyNames, StringComparer.Ordinal));

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_warehouse_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
