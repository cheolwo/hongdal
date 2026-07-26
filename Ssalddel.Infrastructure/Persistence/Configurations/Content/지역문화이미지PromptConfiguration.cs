using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Content;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Content;

public sealed class 지역문화이미지PromptConfiguration
    : IEntityTypeConfiguration<지역문화이미지Prompt>
{
    public void Configure(EntityTypeBuilder<지역문화이미지Prompt> builder)
    {
        builder.HasIndex(item => item.SubdivisionCode).IsUnique();
        builder.HasIndex(item => new
        {
            item.CountryCode,
            item.RegionTypeCode,
            item.RegionNameKo
        });
        builder.HasIndex(item => new
        {
            item.ReviewStatusCode,
            item.RequiresEvidenceReview
        });
    }
}
