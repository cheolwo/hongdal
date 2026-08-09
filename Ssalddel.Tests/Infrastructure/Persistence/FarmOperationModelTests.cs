using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.농업;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class FarmOperationModelTests
{
    [Fact]
    public void Model_contains_canonical_farm_cultivation_and_sensor_aggregate()
    {
        using var context = CreateContext();

        Assert.Equal("농장", context.Model.FindEntityType(typeof(농장))?.GetTableName());
        Assert.Equal("농장구획", context.Model.FindEntityType(typeof(농장구획))?.GetTableName());
        Assert.Equal("재배작기", context.Model.FindEntityType(typeof(재배작기))?.GetTableName());
        Assert.Equal("농업센서", context.Model.FindEntityType(typeof(농업센서))?.GetTableName());
        Assert.Equal("농업센서관측", context.Model.FindEntityType(typeof(농업센서관측))?.GetTableName());
        Assert.Equal("농장작업", context.Model.FindEntityType(typeof(농장작업))?.GetTableName());

        AssertUniqueStableId<농장>(context);
        AssertUniqueStableId<농장구획>(context);
        AssertUniqueStableId<재배작기>(context);
        AssertUniqueStableId<농업센서>(context);
        AssertUniqueStableId<농장작업>(context);
    }

    private static void AssertUniqueStableId<TEntity>(SsalddelContext context)
    {
        var entity = context.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entity);
        Assert.Contains(entity!.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["StableId"]));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_model_test;User=root;Password=test;",
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
