using Hongdal.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.Community;

public sealed class PlatformCommunityPostRecommendationConfiguration : IEntityTypeConfiguration<PlatformCommunityPostRecommendation>
{
    public void Configure(EntityTypeBuilder<PlatformCommunityPostRecommendation> builder)
    {
        builder.ToTable("platform_community_post_recommendations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecommenderKey).HasMaxLength(120).IsRequired();

        builder.HasIndex(x => new { x.PostId, x.RecommenderKey }).IsUnique();
        builder.HasIndex(x => new { x.PostId, x.CreatedAtUtc });
    }
}
