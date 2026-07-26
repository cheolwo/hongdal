using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.기사;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class DriverSettlementAccountModelTests
{
    [Fact]
    public void 정산계좌는_기사별_한건이며_민감필드에_암호화_converter가_적용된다()
    {
        using var context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(기사정산계좌));

        Assert.NotNull(entity);
        Assert.Equal("기사정산계좌", entity!.GetTableName());
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique &&
            index.Properties.Select(x => x.Name).SequenceEqual([nameof(기사정산계좌.기사Id)]));
        Assert.NotNull(entity.FindProperty(nameof(기사정산계좌.예금주명))?.GetValueConverter());
        Assert.NotNull(entity.FindProperty(nameof(기사정산계좌.계좌번호))?.GetValueConverter());
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
