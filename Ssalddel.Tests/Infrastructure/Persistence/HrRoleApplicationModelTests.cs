using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Ssalddel.Application.HumanResources;
using Ssalddel.Domain.HumanResources;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class HrRoleApplicationModelTests
{
    [Fact]
    public void Model은_지원원장과멱등활성지원Index를구성한다()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(HrRoleApplicationRecord));

        Assert.NotNull(entity);
        Assert.Equal("hr_role_applications", entity!.GetTableName());
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(HrRoleApplicationRecord.ApplicantUserId), nameof(HrRoleApplicationRecord.SubmissionRequestId)]));
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(HrRoleApplicationRecord.ActiveApplicationKey)]));
    }

    [Fact]
    public void Migration과Snapshot은_역할지원원장을포함한다()
    {
        using var context = CreateContext();
        const string migrationId = "20260720180000_AddHrRoleApplications";
        Assert.Contains(migrationId, context.Database.GetMigrations());

        var assembly = context.GetService<IMigrationsAssembly>();
        var migration = assembly.CreateMigration(assembly.Migrations[migrationId], context.Database.ProviderName!);
        var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>());
        Assert.Equal("hr_role_applications", table.Name);

        var snapshotType = typeof(HR역할지원CommandUseCase).Assembly
            .GetType("Ssalddel.Migrations.SsalddelContextModelSnapshot");
        var snapshot = Assert.IsAssignableFrom<ModelSnapshot>(
            Activator.CreateInstance(snapshotType!, nonPublic: true));
        Assert.Equal(
            "hr_role_applications",
            snapshot.Model.FindEntityType(typeof(HrRoleApplicationRecord).FullName!)?.GetTableName());
    }

    [Fact]
    public void 관리자통합검토Query는_MySql에서지원과배정UnionSql로번역된다()
    {
        using var context = CreateContext();
        var sql = new HR역할검토조회UseCase(context).검토Query().ToQueryString();

        Assert.Contains("UNION ALL", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hr_role_applications", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("hr_role_assignments", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_hr_application_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)),
                options => options.MigrationsAssembly(typeof(HR역할지원CommandUseCase).Assembly.GetName().Name))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
