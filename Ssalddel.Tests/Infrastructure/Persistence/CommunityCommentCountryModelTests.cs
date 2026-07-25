using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class CommunityCommentCountryModelTests
{
    [Fact]
    public void MigrationAndSnapshot_AddOptionalCountrySnapshotToBothCommentTables()
    {
        using var context = CreateContext();
        const string migrationId = "20260725143000_AddCommunityCommentDisplayCountry";
        Assert.Contains(migrationId, context.Database.GetMigrations());

        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[migrationId],
            context.Database.ProviderName!);
        var addedColumns = migration.UpOperations.OfType<AddColumnOperation>().ToArray();

        Assert.Equal(4, addedColumns.Length);
        Assert.Equal(2, addedColumns.Count(column => column.Table == "platform_community_post_comments"));
        Assert.Equal(2, addedColumns.Count(column => column.Table == "platform_community_post_attachment_comments"));
        Assert.All(
            addedColumns.Where(column => column.Name == nameof(PlatformCommunityPostComment.IsAuthorDisplayCountryPublic)),
            column => Assert.Equal(false, column.DefaultValue));

        AssertCommentModel(context.Model.FindEntityType(typeof(PlatformCommunityPostComment)));
        AssertAttachmentCommentModel(context.Model.FindEntityType(typeof(PlatformCommunityPostAttachmentComment)));

        var snapshotType = typeof(커뮤니티게시글참여UseCase).Assembly
            .GetType("Ssalddel.Migrations.SsalddelContextModelSnapshot");
        var snapshot = Assert.IsAssignableFrom<ModelSnapshot>(
            Activator.CreateInstance(snapshotType!, nonPublic: true));
        AssertCommentModel(snapshot.Model.FindEntityType(typeof(PlatformCommunityPostComment).FullName!));
        AssertAttachmentCommentModel(snapshot.Model.FindEntityType(typeof(PlatformCommunityPostAttachmentComment).FullName!));
    }

    private static void AssertCommentModel(IReadOnlyEntityType? entity)
    {
        Assert.Equal(
            2,
            entity?.FindProperty(nameof(PlatformCommunityPostComment.AuthorDisplayCountryCode))?.GetMaxLength());
        Assert.NotNull(entity?.FindProperty(nameof(PlatformCommunityPostComment.IsAuthorDisplayCountryPublic)));
    }

    private static void AssertAttachmentCommentModel(IReadOnlyEntityType? entity)
    {
        Assert.Equal(
            2,
            entity?.FindProperty(nameof(PlatformCommunityPostAttachmentComment.AuthorDisplayCountryCode))?.GetMaxLength());
        Assert.NotNull(entity?.FindProperty(nameof(PlatformCommunityPostAttachmentComment.IsAuthorDisplayCountryPublic)));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_comment_country_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)),
                options => options.MigrationsAssembly(typeof(커뮤니티게시글참여UseCase).Assembly.GetName().Name))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
