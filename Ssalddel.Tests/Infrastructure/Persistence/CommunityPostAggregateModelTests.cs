using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class CommunityPostAggregateModelTests
{
    [Fact]
    public void Model_게시글Aggregate의소유관계와삭제정책을고정한다()
    {
        using var context = CreateContext();

        AssertRelationship<PlatformCommunityPostAttachment, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostAttachmentComment, PlatformCommunityPostAttachment>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostComment, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostRecommendation, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostTranslation, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostAudio, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade, unique: true);
        AssertRelationship<PlatformCommunityPostAudioSegment, PlatformCommunityPostAudio>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostAudioAccessLog, PlatformCommunityPostAudio>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<PlatformCommunityPostKeywordScan, PlatformCommunityPost>(
            context, DeleteBehavior.Cascade, unique: true);
    }

    [Fact]
    public void Model_알림이력은게시글삭제로유실되지않고전송대상만종속된다()
    {
        using var context = CreateContext();

        AssertRelationship<CommunityKeywordNotification, PlatformCommunityPost>(
            context, DeleteBehavior.Restrict);
        AssertRelationship<CommunityKeywordNotificationDelivery, CommunityKeywordNotification>(
            context, DeleteBehavior.Cascade);
        AssertRelationship<
            CommunityKeywordNotificationDelivery,
            Ssalddel.Domain.Notifications.SsalddelMobilePushInstallation>(
            context,
            DeleteBehavior.Restrict);
    }

    [Fact]
    public void Model_게시글조회수는_bigint_기본값_0으로_저장한다()
    {
        using var context = CreateContext();
        var entity = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(PlatformCommunityPost)));
        var property = Assert.IsAssignableFrom<IProperty>(
            entity.FindProperty(nameof(PlatformCommunityPost.ViewCount)));

        Assert.Equal(typeof(long), property.ClrType);
        Assert.Equal("bigint", property.GetColumnType());
        Assert.Equal(0L, property.GetDefaultValue());
    }

    [Fact]
    public void Model_마음모으기선택값은_기본적으로_비활성이다()
    {
        using var context = CreateContext();
        var entity = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(PlatformCommunityPost)));
        var property = Assert.IsAssignableFrom<IProperty>(
            entity.FindProperty(nameof(PlatformCommunityPost.IsInterestGatheringEnabled)));

        Assert.Equal(typeof(bool), property.ClrType);
        Assert.Equal("tinyint(1)", property.GetColumnType());
        Assert.Equal(false, property.GetDefaultValue());
    }

    private static void AssertRelationship<TDependent, TPrincipal>(
        SsalddelContext context,
        DeleteBehavior deleteBehavior,
        bool unique = false)
        where TDependent : class
        where TPrincipal : class
    {
        var dependent = Assert.IsAssignableFrom<IEntityType>(
            context.Model.FindEntityType(typeof(TDependent)));
        var foreignKey = Assert.Single(
            dependent.GetForeignKeys(),
            candidate => candidate.PrincipalEntityType.ClrType == typeof(TPrincipal));

        Assert.Equal(deleteBehavior, foreignKey.DeleteBehavior);
        Assert.Equal(unique, foreignKey.IsUnique);
        Assert.All(foreignKey.Properties, property => Assert.False(property.IsShadowProperty()));
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                "Server=localhost;Database=ssalddel_community_model_test;User=root;Password=test;",
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
