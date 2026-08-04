using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class CommunityPostSourceEvidenceModelTests
{
    [Fact]
    public void ModelMigrationAndSnapshot_게시글의지도Observation근거를보존한다()
    {
        using var context = CreateContext();
        var post = context.Model.FindEntityType(typeof(PlatformCommunityPost));
        Assert.NotNull(post);
        Assert.Equal(200, post!.FindProperty(nameof(PlatformCommunityPost.SourceObservationStableId))?.GetMaxLength());
        Assert.Equal(80, post.FindProperty(nameof(PlatformCommunityPost.SourceDatasetCode))?.GetMaxLength());
        Assert.Equal(128, post.FindProperty(nameof(PlatformCommunityPost.SourceSnapshotRevision))?.GetMaxLength());
        Assert.Contains(post.GetIndexes(), index =>
            index.GetDatabaseName() == "IX_platform_community_posts_source_observation"
            && index.Properties.Select(property => property.Name).SequenceEqual(
                [
                    nameof(PlatformCommunityPost.SourceDatasetCode),
                    nameof(PlatformCommunityPost.SourceObservationStableId),
                    nameof(PlatformCommunityPost.IsDeleted)
                ]));

        const string migrationId = "20260803221253_AddCommunityPostSourceEvidence";
        Assert.Contains(migrationId, context.Database.GetMigrations());
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        var migration = migrationsAssembly.CreateMigration(
            migrationsAssembly.Migrations[migrationId],
            context.Database.ProviderName!);
        var addedColumns = migration.UpOperations
            .OfType<AddColumnOperation>()
            .Where(operation => operation.Table == "platform_community_posts")
            .Select(operation => operation.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(
            new[]
            {
                nameof(PlatformCommunityPost.SourceObservationStableId),
                nameof(PlatformCommunityPost.SourceDatasetCode),
                nameof(PlatformCommunityPost.SourceSnapshotRevision),
                nameof(PlatformCommunityPost.SourceEvidenceJson)
            },
            column => Assert.Contains(column, addedColumns));
        Assert.Contains(
            migration.UpOperations.OfType<CreateIndexOperation>(),
            operation => operation.Name == "IX_platform_community_posts_source_observation");

        var snapshotType = typeof(커뮤니티세계지도질문UseCase).Assembly
            .GetType("Ssalddel.Migrations.SsalddelContextModelSnapshot");
        var snapshot = Assert.IsAssignableFrom<ModelSnapshot>(
            Activator.CreateInstance(snapshotType!, nonPublic: true));
        var snapshotPost = snapshot.Model.FindEntityType(typeof(PlatformCommunityPost).FullName!);
        Assert.NotNull(snapshotPost);
        Assert.NotNull(snapshotPost!.FindProperty(nameof(PlatformCommunityPost.SourceObservationStableId)));
        Assert.NotNull(snapshotPost.FindProperty(nameof(PlatformCommunityPost.SourceEvidenceJson)));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_source_evidence_model_test;User=root;Password=test;",
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql => mysql.MigrationsAssembly(
                    typeof(커뮤니티세계지도질문UseCase).Assembly.GetName().Name))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
