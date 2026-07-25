using Ssalddel.Domain.Content;
using Ssalddel.Services.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class YouTubeFoodCommerceModelTests
{
    [Fact]
    public void Model은_음식채널프로필과영상상품후보를매핑한다()
    {
        using var context = CreateContext();
        var channel = context.Model.FindEntityType(typeof(YouTube감시채널));
        var candidate = context.Model.FindEntityType(typeof(YouTube영상상품후보));

        Assert.Equal("youtube_watched_channels", channel?.GetTableName());
        Assert.Equal(100, channel?.FindProperty(nameof(YouTube감시채널.Handle))?.GetMaxLength());
        Assert.Equal(2, channel?.FindProperty(nameof(YouTube감시채널.국가코드))?.GetMaxLength());
        Assert.False(channel?.FindProperty(nameof(YouTube감시채널.국가코드))?.IsNullable);
        Assert.Contains(channel!.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_youtube_watched_channels_country_active_sync"
            && index.Properties.Select(property => property.Name).SequenceEqual(
                [
                    nameof(YouTube감시채널.국가코드),
                    nameof(YouTube감시채널.활성화여부),
                    nameof(YouTube감시채널.마지막동기화일시Utc)
                ]));
        Assert.Equal("youtube_video_product_candidates", candidate?.GetTableName());
        Assert.Equal(200, candidate?.FindProperty(nameof(YouTube영상상품후보.상품키))?.GetMaxLength());
        Assert.Contains(candidate!.GetIndexes(), index =>
            index.IsUnique
            && index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(YouTube영상상품후보.YouTube채널영상Id), nameof(YouTube영상상품후보.상품키)]));
    }

    [Fact]
    public void MigrationBaseline과Snapshot은_유튜브음식커머스를포함한다()
    {
        using var context = CreateContext();
        const string migrationId = "20260723113440_AddCommunityPostEmailNotificationOutbox";
        Assert.Contains(migrationId, context.Database.GetMigrations());

        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[migrationId],
            context.Database.ProviderName!);
        var createdTable = Assert.Single(
            migration.UpOperations.OfType<CreateTableOperation>(),
            operation => operation.Name == "youtube_video_product_candidates");
        Assert.Equal("youtube_video_product_candidates", createdTable.Name);
        var expectedColumnNames = new[]
        {
            "channel_handle",
            "country_code",
            "default_language_code",
            "food_category_codes",
            "import_discovery_score",
            "is_food_channel",
            "purchase_discovery_score",
            "research_note",
            "research_source_url",
            "research_verified_at_utc"
        };
        var addedColumns = migration.UpOperations
            .OfType<AddColumnOperation>()
            .Where(operation => operation.Table == "youtube_watched_channels")
            .Where(operation => expectedColumnNames.Contains(operation.Name, StringComparer.Ordinal))
            .ToArray();
        Assert.Equal(expectedColumnNames.Length, addedColumns.Length);
        Assert.All(
            addedColumns,
            operation => Assert.Equal("youtube_watched_channels", operation.Table));
        var countryColumn = Assert.Single(
            addedColumns,
            operation => operation.Name == "country_code");
        Assert.False(countryColumn.IsNullable);
        Assert.Equal("ZZ", countryColumn.DefaultValue);
        Assert.Contains(
            migration.UpOperations.OfType<CreateIndexOperation>(),
            operation => operation.Name == "IX_youtube_watched_channels_country_active_sync"
                && operation.Columns.SequenceEqual(
                    ["country_code", "is_active", "last_synced_at_utc"]));

        var snapshotType = typeof(YouTube음식상품발견Service).Assembly
            .GetType("Ssalddel.Migrations.SsalddelContextModelSnapshot");
        var snapshot = Assert.IsAssignableFrom<ModelSnapshot>(
            Activator.CreateInstance(snapshotType!, nonPublic: true));
        Assert.Equal(
            "youtube_video_product_candidates",
            snapshot.Model.FindEntityType(typeof(YouTube영상상품후보).FullName!)?.GetTableName());
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_youtube_food_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)),
                options => options.MigrationsAssembly(typeof(YouTube음식상품발견Service).Assembly.GetName().Name))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
