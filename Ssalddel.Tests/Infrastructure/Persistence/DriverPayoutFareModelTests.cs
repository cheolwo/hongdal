using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.운송;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class DriverPayoutFareModelTests
{
    [Fact]
    public void 운임구성은_화주최종운임과_기사지급예정운임을_분리한다()
    {
        using var context = CreateContext();

        var entity = context.Model.FindEntityType(typeof(운임구성));

        Assert.NotNull(entity);
        Assert.Equal(
            "최종운임",
            entity!.FindProperty(nameof(운임구성.최종운임))?.GetColumnName());
        Assert.Equal(
            "driver_expected_payout",
            entity.FindProperty(nameof(운임구성.기사지급예정운임))?.GetColumnName());
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
