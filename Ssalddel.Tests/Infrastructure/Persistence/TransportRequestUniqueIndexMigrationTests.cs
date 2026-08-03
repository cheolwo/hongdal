using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Dispatch.Queue;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class TransportRequestUniqueIndexMigrationTests
{
    [Fact]
    public void Migration_빈의뢰Id를정규화한뒤고유인덱스를만든다()
    {
        using var context = CreateContext();
        const string migrationId = "20260727112931_AddTransportRequestUniqueIndex";
        Assert.Contains(migrationId, context.Database.GetMigrations());

        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[migrationId],
            context.Database.ProviderName!);
        var operations = migration.UpOperations.ToList();
        var legacyRequestIdBackfill = Assert.Single(
            operations.OfType<SqlOperation>(),
            operation => operation.Sql.Contains(
                "legacy-unlinked-transport-",
                StringComparison.Ordinal));
        var uniqueRequestIdIndex = Assert.Single(
            operations.OfType<CreateIndexOperation>(),
            operation => operation.Name == "ux_운송실행투영_request_id"
                         && operation.IsUnique);

        Assert.Contains("TRIM(`request_id`) = ''", legacyRequestIdBackfill.Sql);
        Assert.True(
            operations.IndexOf(legacyRequestIdBackfill)
            < operations.IndexOf(uniqueRequestIdIndex));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_transport_migration_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql => mysql.MigrationsAssembly(
                    typeof(운송의뢰배차대기Service).Assembly.GetName().Name))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
